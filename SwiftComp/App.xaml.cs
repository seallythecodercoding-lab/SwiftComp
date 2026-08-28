using System.Windows;
namespace SwiftComp;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Window w;
        if (e.Args.Length > 0 && System.IO.Directory.Exists(e.Args[0]))
            w = new MainWindow(e.Args[0]);
        else
            w = new MainWindow();
        w.Show();
    }
}
