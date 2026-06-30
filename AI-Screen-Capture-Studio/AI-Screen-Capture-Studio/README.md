# AI Screen Capture Studio

A Windows desktop tool (.NET 8 / WPF / MVVM) that captures the screen, lets you
annotate it on a non-destructive layer stack, and uses a vision LLM to generate
step-by-step usage guidelines and editable annotations — with a review step so
nothing AI-generated is ever burned onto your original screenshot.

## Features

- **Capture**: full screen, drag-selected region, or the active window (DWM frame
  bounds, multi-monitor and per-monitor DPI aware).
- **Global hotkeys** (configurable): `PrintScreen` full screen, `Ctrl+Shift+S`
  region, `Ctrl+Alt+S` active window.
- **Editor**: Select / Arrow / Rectangle / Highlight / Text / Numbered-step
  tools, move & resize, undo/redo, copy/paste, zoom.
- **3-layer model**: *Original screenshot* (locked) · *AI annotations* · *User
  annotations*. AI output lands on its own editable layer.
- **Export**: PNG, JPG, single-page PDF, or straight to the clipboard
  (flattened).
- **AI assistant**: guideline generator + annotation generator returning strict
  JSON with per-element **confidence scores**; low-confidence items are flagged.
- **Review mode**: inspect functions, steps, and proposed annotations, then
  Accept All / Reject All before they touch the canvas.
- **Provider abstraction**: OpenAI, Claude (Anthropic), Gemini, Ollama (local),
  and any OpenAI-compatible **Custom** endpoint — selected via a factory and DI.
- **Secure keys**: API keys are encrypted at rest with Windows **DPAPI**
  (current-user scope); only ciphertext is written to the JSON settings file.
- **OCR**: local Tesseract, with a graceful message when language data is absent.
- **AI history**: every run stored locally with a thumbnail and the raw response.
- **System tray**: runs in the background; capture from the tray menu or hotkeys.

## Quick start

This project targets Windows and **must be built on Windows** (WPF). The simplest
path is GitHub Actions:

1. Push this repository to GitHub.
2. The workflow in `.github/workflows/build.yml` builds on `windows-latest` and
   uploads a self-contained `win-x64` build (with English OCR data) as an
   artifact.
3. Download the artifact, run `AIScreenCaptureStudio.exe`.

To build locally on Windows:

```powershell
dotnet restore AIScreenCaptureStudio.sln
dotnet build AIScreenCaptureStudio.sln -c Release
dotnet run --project src/AIScreenCaptureStudio/AIScreenCaptureStudio.csproj
```

## Configuring AI

Open the tray menu → **Settings → AI Settings**, pick a provider, set the
endpoint/model, paste your API key, and click **Test Connection**. Use **Use
defaults** to prefill sensible endpoints/models per provider.

See [docs/Architecture.md](docs/Architecture.md),
[docs/Build.md](docs/Build.md), and [docs/Deployment.md](docs/Deployment.md).

## Tech

.NET 8 · WPF · MVVM · Microsoft.Extensions.DependencyInjection ·
Hardcodet.NotifyIcon.Wpf · PdfSharp · Tesseract · System.Drawing.Common.
