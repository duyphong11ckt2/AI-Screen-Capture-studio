# Deployment Guide

## Distributing to end users

1. Run the **Publish** step (or download the CI artifact).
2. Zip the `publish/` folder and share it, or wrap it in an installer
   (Inno Setup / WiX) if you want Start-menu and uninstall entries.
3. The app is self-contained: no .NET runtime install is required on the target
   machine.

## First run

- The app starts hidden in the **system tray**. Right-click the tray icon for the
  capture menu, History, and Settings.
- Default hotkeys: `PrintScreen`, `Ctrl+Shift+S`, `Ctrl+Alt+S`. If another app
  already owns a hotkey, registration is skipped for that one — change it in
  **Settings → Hotkeys**.
- Captures are saved under `Pictures/AI Screen Capture Studio` by default.

## Per-user data

Stored under `%APPDATA%/AIScreenCaptureStudio/`:

- `settings.json` — all settings, including the **DPAPI-encrypted** API key.
- `history/` — AI run history index plus thumbnails.

Because DPAPI is scoped to the current Windows user, the encrypted key cannot be
decrypted by another user or on another machine — copying `settings.json`
elsewhere will simply blank the key, which is the intended behaviour.

## Org rollout notes

- Distribute per-user API keys; never embed a shared key in the package.
- If you ship OCR, include the relevant `*.traineddata` files in `tessdata/`.
- Vision API calls send screenshots to the configured provider — review data-
  handling/compliance before enabling cloud providers, or use the local **Ollama**
  provider to keep images on-device.
