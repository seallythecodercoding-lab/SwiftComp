using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;

namespace SwiftComp;

public partial class MainWindow : Window
{
    private string source = "";
    private string output = "";
    private string projectType = "";

    public MainWindow() : this(null) { }

    public MainWindow(string? preselected)
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(preselected) && Directory.Exists(preselected))
            SetSource(preselected);
    }

    private void SetSource(string path)
    {
        source = path;
        output = Path.Combine(path, "output");
        projectType = Detect(path);
        PathText.Text = path;
        OutText.Text = output;
        TipoBadge.Text = projectType;
        CmdText.Text = projectType switch
        {
            ".NET" => "dotnet build -c Release -o output",
            "C++" => "msbuild /p:OutDir=output\\",
            "Node" => "npm run build → output/",
            "Python" => "python -m build --outdir output",
            _ => "xcopy → output/"
        };
        EmptyState.Visibility = Visibility.Collapsed;
        SelectedState.Visibility = Visibility.Visible;
        CompileBtn.IsEnabled = true;
        StatusText.Text = $"Pronto para compilar: {projectType}";
        Log($"Tipo detectado: {projectType}");
        Log($"Saída: {output}");
    }

    private static string Detect(string path)
    {
        var files = Directory.GetFiles(path).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var all = Directory.GetFiles(path);
        if (all.Any(f => f.EndsWith(".sln") || f.EndsWith(".csproj"))) return ".NET";
        if (all.Any(f => f.EndsWith(".vcxproj"))) return "C++";
        if (files.Contains("pyproject.toml") || files.Contains("requirements.txt")) return "Python";
        if (files.Contains("package.json"))
        {
            try
            {
                var txt = File.ReadAllText(Path.Combine(path, "package.json"));
                if (txt.Contains("\"next\"")) return "Next.js";
                if (txt.Contains("\"astro\"")) return "Astro";
                return "Node";
            }
            catch { return "Node"; }
        }
        return "Genérico";
    }

    private void Pick_Click(object s, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Selecione a pasta do projeto" };
        if (dlg.ShowDialog() == true) SetSource(dlg.FolderName);
    }

    private void Window_DragOver(object s, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }
    private void Window_Drop(object s, DragEventArgs e) => HandleDrop(e);
    private void Card_DragOver(object s, DragEventArgs e) { e.Effects = DragDropEffects.Copy; DropCard.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x25, 0x63, 0xEB)); e.Handled = true; }
    private void Card_DragLeave(object s, DragEventArgs e) { DropCard.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE2, 0xE8, 0xF0)); }
    private void Card_Drop(object s, DragEventArgs e) { Card_DragLeave(s, e); HandleDrop(e); }

    private void HandleDrop(DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Length > 0)
        {
            var p = paths[0];
            if (File.Exists(p)) p = Path.GetDirectoryName(p)!;
            if (Directory.Exists(p)) SetSource(p);
        }
    }

    private async void Compile_Click(object s, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(source)) return;
        CompileBtn.IsEnabled = false;
        Prog.Visibility = Visibility.Visible;
        StatusText.Text = "Compilando...";
        LogText.Text = "";
        Log($"Iniciando compilação de {projectType}...");
        Log($"Origem: {source}");

        try
        {
            if (Directory.Exists(output))
            {
                Log("Limpando output anterior...");
                Directory.Delete(output, true);
            }
            Directory.CreateDirectory(output);

            switch (projectType)
            {
                case ".NET": await RunAsync("dotnet", $"build -c Release -o \"{output}\"", source); break;
                case "C++":
                    var vcx = Directory.GetFiles(source, "*.vcxproj", SearchOption.AllDirectories).FirstOrDefault();
                    if (vcx == null) throw new FileNotFoundException("Nenhum .vcxproj");
                    await RunAsync("msbuild", $"\"{vcx}\" /p:Configuration=Release /p:OutDir=\"{output}\\\"", source);
                    break;
                case "Node":
                case "Next.js":
                case "Astro":
                    if (!Directory.Exists(Path.Combine(source, "node_modules")))
                    {
                        Log("npm install...");
                        await RunAsync("npm", "install", source);
                    }
                    Log("npm run build...");
                    await RunAsync("npm", "run build", source);
                    foreach (var cand in new[] { "dist", "build", ".next", "out" })
                    {
                        var src = Path.Combine(source, cand);
                        if (Directory.Exists(src)) { Log($"Copiando {cand}/ → output/..."); CopyDir(src, output); break; }
                    }
                    break;
                case "Python":
                    try { await RunAsync("python", $"-m build --outdir \"{output}\"", source); }
                    catch { Log("Fallback: copiando..."); CopyGeneric(); }
                    break;
                default: CopyGeneric(); break;
            }

            Log("✓ SUCESSO! Arquivos em output/");
            StatusText.Text = "✓ Concluído!";
            Prog.Visibility = Visibility.Collapsed;
            CompileBtn.IsEnabled = true;
            // abre output sozinho
            Process.Start(new ProcessStartInfo { FileName = output, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log($"✗ Erro: {ex.Message}");
            StatusText.Text = "Erro - veja logs";
            Prog.Visibility = Visibility.Collapsed;
            CompileBtn.IsEnabled = true;
        }
    }

    private void CopyGeneric()
    {
        Log("Copiando arquivos → output/...");
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "output", ".git", "node_modules", ".vs", "bin", "obj" };
        foreach (var item in Directory.GetFileSystemEntries(source))
        {
            var name = Path.GetFileName(item);
            if (exclude.Contains(name)) continue;
            var dest = Path.Combine(output, name);
            if (Directory.Exists(item)) CopyDir(item, dest);
            else File.Copy(item, dest, true);
        }
    }

    private static void CopyDir(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(src, f);
            var d = Path.Combine(dst, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(d)!);
            File.Copy(f, d, true);
        }
    }

    private async Task RunAsync(string exe, string args, string cwd)
    {
        Log($"$ {exe} {args}");
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {exe} {args}",
            WorkingDirectory = cwd,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        using var p = new Process { StartInfo = psi };
        p.OutputDataReceived += (_, e) => { if (e.Data != null) Dispatcher.Invoke(() => Log(e.Data)); };
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) Dispatcher.Invoke(() => Log(e.Data)); };
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0) throw new Exception($"{exe} falhou (exit {p.ExitCode})");
    }

    private void Log(string msg)
    {
        LogText.Text += $"[{DateTime.Now:HH:mm:ss}] {msg}\n";
        LogScroll.ScrollToBottom();
    }

    private void OpenOut_Click(object s, RoutedEventArgs e)
    {
        if (Directory.Exists(output)) Process.Start(new ProcessStartInfo { FileName = output, UseShellExecute = true });
        else MessageBox.Show("output/ ainda não existe. Compile primeiro.", "Aviso");
    }
    private void OpenSrc_Click(object s, RoutedEventArgs e)
    {
        if (Directory.Exists(source)) Process.Start(new ProcessStartInfo { FileName = source, UseShellExecute = true });
    }
}
