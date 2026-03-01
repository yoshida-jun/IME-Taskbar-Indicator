# IME Color Indicator - Developer Documentation

Technical specifications, build instructions, and release procedures.

## Project Structure

```
IMEColorIndicator/
├── .github/
│   └── workflows/
│       ├── build.yml     # Automated build and test workflow
│       └── release.yml   # Release automation workflow
├── IMEColorIndicator/
│   ├── App.xaml.cs           # Application entry point
│   ├── ColorBarWindow.xaml.cs # Color bar display logic
│   ├── SettingsWindow.xaml.cs # Settings window logic
│   ├── ImeMonitor.cs         # IME state monitoring
│   ├── Settings.cs           # Settings save/load
│   ├── AutoStartHelper.cs    # Windows auto-start management
│   ├── Updater.cs            # Auto-update functionality
│   └── LocalizationHelper.cs # Localization support
├── test.ps1              # Build and test script
├── build-release.ps1     # Release build script
├── README.md             # User documentation
├── DEVMEJP.md            # Developer documentation (Japanese)
└── DEVMEUS.md            # Developer documentation (English)
```

## Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | WPF (.NET 10.0) |
| **Language** | C# 13 |
| **IME Detection** | Windows IMM32 API (`ImmGetContext`, `ImmGetOpenStatus`) |
| **Click-Through** | Win32 API (`WS_EX_TRANSPARENT`, `WS_EX_LAYERED`) |
| **Settings Storage** | JSON format (`System.Text.Json`) |
| **Storage Location** | `%APPDATA%\IMEColorIndicator\settings.json` |
| **Auto-Start** | Windows Registry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`) |
| **Auto-Update** | GitHub Releases API + batch script |
| **Localization** | System language detection via `GetUserDefaultUILanguage()` |

## Build Instructions

### Prerequisites

- .NET 10.0 SDK
- Windows 10/11 (required for WPF development)

### Development Build

```bash
cd IMEColorIndicator
dotnet restore
dotnet build
```

Executable will be generated at `bin\Debug\net10.0-windows\IMEColorIndicator.exe`.

### Release Build (Single-File Executable)

```powershell
# Run build script in PowerShell
.\build-release.ps1
```

Or manually:

```powershell
dotnet publish IMEColorIndicator/IMEColorIndicator.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:PublishTrimmed=false `
  -o ./publish
```

Single-file executable will be generated at `publish\IMEColorIndicator.exe`.

**Important**: Keep `PublishTrimmed=false` to avoid runtime errors with reflection.

### Test Execution

```powershell
# Run build and tests
.\test.ps1
```

Always build and test locally before pushing to ensure proper functionality.

## CI/CD

Automated build and release with GitHub Actions.

### Automated Build (.github/workflows/build.yml)

**Trigger**: Push or PR to master/main branch

**Process**:
1. Setup .NET 10.0 SDK
2. `dotnet restore`
3. `dotnet build -c Release`
4. `dotnet test` (if test project exists)

### Automated Release (.github/workflows/release.yml)

**Trigger**: Push `v*` tag (e.g., `v0.3.1`)

**Process**:
1. Setup .NET 10.0 SDK
2. `dotnet restore`
3. `dotnet build -c Release`
4. `dotnet publish` (single-file, self-contained)
5. Generate `version.txt` (tag name without `v` prefix)
6. Upload to GitHub Releases:
   - `IMEColorIndicator.exe`
   - `version.txt`

## Release Process

### 1. Build and Test Locally (Required)

```powershell
# Build
.\build-release.ps1

# Test the executable
.\publish\IMEColorIndicator.exe
```

**Verify proper functionality before proceeding**

### 2. Update Version Number

Edit [IMEColorIndicator/IMEColorIndicator.csproj](IMEColorIndicator/IMEColorIndicator.csproj):

```xml
<Version>0.3.1</Version>
<AssemblyVersion>0.3.1.0</AssemblyVersion>
<FileVersion>0.3.1.0</FileVersion>
```

### 3. Commit and Push Changes

```bash
git add -A
git commit -m "Bump version to 0.3.1"
git push origin master
```

### 4. Create and Push Tag

```bash
git tag v0.3.1
git push origin v0.3.1
```

### 5. GitHub Actions Automatically Runs

- GitHub Actions automatically builds and releases
- On success, executable and version file are uploaded to GitHub Releases
- Release page: `https://github.com/{OWNER}/{REPO}/releases/tag/v0.3.1`

### 6. Verify Release

1. Download executable from GitHub Releases page
2. Run and verify proper functionality
3. If issues occur, delete tag and retry:
   ```bash
   git tag -d v0.3.1
   git push origin :refs/tags/v0.3.1
   ```

## Auto-Update Mechanism

### Check Interval

Defined in `Updater.cs` as `CHECK_INTERVAL` (default: 1 minute)

For production, recommend 30 minutes to 1 hour:

```csharp
private static readonly TimeSpan CHECK_INTERVAL = TimeSpan.FromMinutes(30);
```

### Update Flow

1. Background periodic check of `version.txt`
2. If remote version > local version:
   - Download new executable
   - Save as temporary file (`.new.exe`)
   - Create and execute update script (`.update.bat`)
   - Exit application
3. Batch script:
   - Wait 2 seconds
   - Delete old executable
   - Replace with new executable
   - Launch new executable
   - Delete itself (batch script)

### Notes

- In single-file publish environments, `Process.GetCurrentProcess().MainModule?.FileName` may return `null`, so prioritize `Environment.ProcessPath`
- Updater instantiation is protected with try-catch to ensure app launches even if update feature fails

## Troubleshooting

### Build Error: "Assets file doesn't have target for 'net10.0-windows/win-x64'"

**Cause**: `--no-build` flag used in publish step

**Fix**: Remove `--no-build` from `dotnet publish`

### Runtime Crash: Exits Immediately

**Cause**: `Process.GetCurrentProcess().MainModule?.FileName` returns `null` in single-file publish environment

**Fix**: Prioritize `Environment.ProcessPath` (fixed in v0.3.1)

### Auto-Update Not Working

**Check**:
1. Is "Enable Auto Update" turned ON in settings?
2. Do `version.txt` and `IMEColorIndicator.exe` exist in GitHub Releases?
3. Is version number correctly written (without `v` prefix)?
4. Is it blocked by firewall?

## License

MIT License

## Development History

- **v0.3.1** (2026-01-30): Fix crash in single-file publish environment
- **v0.3.0** (2026-01-30): Add language switcher and auto-update
- **v0.2.0** (2026-01-29): Localization support, bilingual README
- **v0.1.0** (2026-01-29): Initial release, CI/CD setup
