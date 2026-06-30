# Architecture Overview

## Layers

```
Views (XAML)  ──data binding──▶  ViewModels  ──interfaces──▶  Services / Providers
     │                               │                              │
  no logic                    no WPF types                 IO, Win32, HTTP, AI
```

- **Views** (`Views/`) are XAML + thin code-behind. The only non-trivial
  code-behind is `EditorWindow` (canvas hit-testing, draw/move/resize, and
  flatten rendering) and `RegionSelectionWindow` (overlay drag).
- **ViewModels** (`ViewModels/`) hold all state and commands and never reference
  window types. Window creation is delegated to `IWindowManager`.
- **Services** (`Services/`) implement capture, storage, export, OCR, clipboard,
  hotkeys, history, dialogs, and window management behind interfaces.
- **Providers** (`Providers/`) implement the AI provider abstraction.

## Dependency injection

`App.OnStartup` builds a `ServiceCollection`, registers every service, all five
providers as `IAIProvider`, the factory, the `AIService`, view models, and
windows, then resolves the tray-hosting `MainWindow`. `EditorViewModel` is
created per-capture via `ActivatorUtilities.CreateInstance` so the `CaptureResult`
can be passed alongside injected dependencies.

## The 3-layer editor model

`EditorViewModel` builds three `Layer` objects:

| Z | Layer               | Locked | Contents                          |
|---|---------------------|--------|-----------------------------------|
| 0 | Original screenshot | yes    | the captured bitmap (no objects)  |
| 1 | AI annotations      | no     | objects created from AI specs     |
| 2 | User annotations    | no     | objects the user draws            |

Annotations are editable model objects (`AnnotationBase` subclasses) bound onto
`Canvas`-backed `ItemsControl`s. AI output is **never** rasterised onto the
original; it is converted to editable objects on layer 1 only after the user
accepts it in review. Export flattens the visible layers on demand via
`EditorWindow.RenderFlattened()`.

## AI request flow

```
EditorViewModel ─▶ IAIService ─▶ IAIProviderFactory ─▶ IAIProvider
                        │                                   │
              decrypt key (DPAPI)                  HTTP + image (base64 PNG)
                        │                                   │
                 record history  ◀── strict-JSON parse ◀────┘
```

Every provider returns text that is parsed into `AIAnalysisResult` by extracting
the first balanced JSON object (`AIProviderBase.ParseResult`). Free-form prose is
never trusted for structure.

## Adding a provider

1. Implement `IAIProvider` (extend `AIProviderBase`).
2. Add a value to `AIProviderType`.
3. Register it in `App.ConfigureServices` as `IAIProvider`.

No UI or view-model changes are required — the factory resolves by type and the
settings screen is data-driven off the enum.

## Security

API keys are encrypted with `ProtectedData` (DPAPI, `CurrentUser` scope) and a
fixed entropy value. Only the Base64 ciphertext is serialized into
`%APPDATA%/AIScreenCaptureStudio/settings.json`. The plaintext key exists only in
the AI settings view model while the dialog is open.
