# Build Instructions

## Prerequisites

- **Windows 10/11** (WPF cannot be compiled or run on Linux/macOS).
- **.NET 8 SDK** (`dotnet --version` ≥ 8.0).

## Build & run

```powershell
dotnet restore AIScreenCaptureStudio.sln
dotnet build   AIScreenCaptureStudio.sln -c Release
dotnet run --project src/AIScreenCaptureStudio/AIScreenCaptureStudio.csproj
```

## Publish a self-contained build

```powershell
dotnet publish src/AIScreenCaptureStudio/AIScreenCaptureStudio.csproj `
  -c Release -r win-x64 --self-contained true -o publish
```

The output folder contains `AIScreenCaptureStudio.exe` and all runtime
dependencies.

## OCR data

Tesseract needs language data. Create a `tessdata` folder next to the executable
and add `eng.traineddata` (and any other languages):

```powershell
mkdir publish\tessdata
Invoke-WebRequest `
  "https://raw.githubusercontent.com/tesseract-ocr/tessdata_fast/main/eng.traineddata" `
  -OutFile publish\tessdata\eng.traineddata
```

The CI workflow does this automatically. If the data is missing the app still
runs; OCR simply reports that it is unavailable.

## CI

`.github/workflows/build.yml` runs restore → build → publish on `windows-latest`,
downloads English OCR data, and uploads the `win-x64` build as an artifact on
every push to `main`/`master` (and via manual dispatch).
