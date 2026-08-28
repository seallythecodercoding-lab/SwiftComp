# SwiftComp — 1-click to compile any project to `output/`

Ultra-simple **native WPF** (.NET 8) compiler. Drag your project folder → click **COMPILE** → everything goes to `output/`.

![Windows](https://img.shields.io/badge/Windows-10%2B-blue) ![WPF](https://img.shields.io/badge/WPF-.NET%208-purple) ![SwiftComp](https://img.shields.io/badge/SwiftComp-1.0-green)

## ✨ How to use

### 1. Easiest — drag onto the EXE
In Explorer, drag your project folder and drop it **onto `SwiftComp.exe`**
→ opens pre-selected → click **COMPILE → output/**

### 2. Inside the app
Double-click `SwiftComp.exe` → drag folder into the white area → **COMPILE**

### 3. Command line
```powershell
.\SwiftComp.exe "C:\your\project"
```

## 🔍 Auto-detection

| Project | Command |
|---------|---------|
| `.sln` / `.csproj` | `dotnet build -c Release -o output` |
| `.vcxproj` | `msbuild /p:OutDir=output\` |
| `package.json` | `npm install` + `npm run build` → copies `dist`/`build`/`.next` → `output/` |
| `pyproject.toml` | `python -m build --outdir output` |
| Generic | copies everything → `output/` |

## 📂 Structure

```
SwiftComp/
├── SwiftComp.exe          # ready to use
├── SwiftComp.csproj
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / .xaml.cs
├── output/                # generated after compile (gitignored)
└── README.md
```

## 🚀 Build SwiftComp itself

```powershell
dotnet build SwiftComp.csproj -c Release -o output
.\output\SwiftComp.exe
```

## 📦 Output

```
your-project/
└── output/
    ├── YourApp.exe
    └── ...
```

## 📝 License

MIT — do whatever you want.

---
Built with WPF + Fiberglass
