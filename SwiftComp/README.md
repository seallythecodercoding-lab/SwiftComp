# SwiftComp — 1 clique para compilar qualquer projeto em `output/`

Compilador **WPF nativo** (.NET 8) ultra simples. Arraste a pasta do projeto → clique **COMPILAR** → tudo vai para `output/`.

![Windows](https://img.shields.io/badge/Windows-10%2B-blue) ![WPF](https://img.shields.io/badge/WPF-.NET%208-purple) ![SwiftComp](https://img.shields.io/badge/SwiftComp-1.0-green)

## ✨ Como usar

### 1. Mais fácil — arraste sobre o EXE
No Explorer, arraste a pasta do projeto e solte **em cima do `SwiftComp.exe`**
→ abre já selecionado → clique **COMPILAR → output/**

### 2. Dentro do app
Duplo clique em `SwiftComp.exe` → arraste a pasta para a área branca → **COMPILAR**

### 3. Linha de comando
```powershell
.\SwiftComp.exe "C:\seu\projeto"
```

## 🔍 Auto-detecção

| Projeto | Comando |
|---------|---------|
| `.sln` / `.csproj` | `dotnet build -c Release -o output` |
| `.vcxproj` | `msbuild /p:OutDir=output\` |
| `package.json` | `npm install` + `npm run build` → copia `dist`/`build`/`.next` → `output/` |
| `pyproject.toml` | `python -m build --outdir output` |
| Genérico | copia tudo → `output/` |

## 📂 Estrutura

```
SwiftComp/
├── SwiftComp.exe          # pronto pra usar
├── SwiftComp.csproj
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / .xaml.cs
├── output/                # gerado após compilar (ignorado no git)
└── README.md
```

## 🚀 Build do próprio SwiftComp

```powershell
dotnet build SwiftComp.csproj -c Release -o output
.\output\SwiftComp.exe
```

## 📦 Output

```
seu-projeto/
└── output/
    ├── SeuApp.exe
    └── ...
```

## 📝 Licença

MIT — faça o que quiser.

---
Feito com WPF + Fiberglass
