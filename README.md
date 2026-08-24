# TextToAI

A lightweight Windows utility that sends selected text to an AI model with a single hotkey. Select text in any application, press your hotkey, and get AI-processed results instantly.

## Features

- **Two global hotkeys** - each with its own prompt, works from any application
- **System tray** - runs quietly in the background
- **Configurable prompts** - customize how text is processed
- **OpenRouter by default** - one key for Claude, Gemini, DeepSeek, GPT and hundreds more
- **OpenAI optional** - switch providers in Settings; a key is kept for each
- **Any model** - pick a preset or type any model name your provider supports
- **Fast** - minimal latency, shows response time

## Installation

### Requirements
- Windows 10/11
- .NET 10.0 Runtime (or newer)
- An API key from [OpenRouter](https://openrouter.ai/keys) (default) or [OpenAI](https://platform.openai.com/api-keys)

### Download
Download the latest `TextToAI.exe` from [Releases](../../releases).

### Build from Source
1. Open `TextToAI.sln` in Visual Studio 2022
2. Build → Build Solution (Ctrl+Shift+B)
3. Run from `bin/Debug/net10.0-windows/TextToAI.exe`

### Publish Single Executable
```powershell
dotnet publish -c Release
```
Output: `bin/Release/net10.0-windows/win-x64/publish/TextToAI.exe`

## Usage

1. **Start the app** - icon appears in system tray
2. **Configure settings** - right-click tray icon → Settings. The dialog has two tabs:
   - **Provider** - pick OpenRouter or OpenAI, enter that provider's API key, choose a model
   - **Hotkeys** - set up to two actions, each a hotkey plus the prompt it runs
3. **Select text** in any application
4. **Press a hotkey** - popup shows AI response
5. **Copy or Dismiss** the result

## Hotkeys and prompts

Two independent actions are available on the **Hotkeys** tab, so the same selection can be sent
through different prompts:

| | Default hotkey | Default prompt |
|---|---|---|
| Action 1 | `Ctrl+Shift+G` | Process the following text |
| Action 2 | `Ctrl+Shift+H` | Summarize this text in 2-3 sentences |

Click **Record**, press the key combination, and it is captured (press `Escape` to cancel).
Action 1 always needs a hotkey; **Clear** the Action 2 hotkey to disable that action entirely.
Both prompts use `{text}` as the placeholder for the selected text.

## Providers

**OpenRouter (default)** routes to models from many vendors through one key and one bill.
Models are named `vendor/model`, e.g. `anthropic/claude-sonnet-4.5`, `google/gemini-2.5-pro`,
`deepseek/deepseek-chat`, `openai/gpt-4o`. Browse the full list at
[openrouter.ai/models](https://openrouter.ai/models) - any slug from there can be typed into
the Model box.

**OpenAI** talks to `api.openai.com` directly with model names like `gpt-4o` or `gpt-4o-mini`.
Choose it in Settings if you already have an OpenAI key.

A key is stored separately for each provider, so switching back and forth never loses one.

> **Upgrading from 1.0?** Your existing OpenAI key is migrated automatically on first launch
> and the app stays on OpenAI - nothing to re-enter. Your old `hotkey` and `prompt` become
> Action 1, and Action 2 is added with its default. OpenRouter is the default for fresh
> installs only.

## Configuration

Settings are stored in `%AppData%\TextToAI\config.json`:

```json
{
  "provider": "OpenRouter",
  "openRouterApiKey": "sk-or-v1-...",
  "openAiApiKey": "",
  "model": "anthropic/claude-sonnet-4.5",
  "startWithWindows": false,
  "actions": [
    {
      "hotkey": "Ctrl+Shift+G",
      "prompt": "Translate to English:\n\n{text}"
    },
    {
      "hotkey": "Ctrl+Shift+H",
      "prompt": "Summarize this text in 2-3 sentences:\n\n{text}"
    }
  ]
}
```

`provider` is either `"OpenRouter"` or `"OpenAI"` and selects which key and endpoint are used.
Each entry in `actions` binds one hotkey to one prompt; an entry with an empty `hotkey` is ignored.

## Prompt Examples

**Translate:**
```
Translate the following text to English:

{text}
```

**Summarize:**
```
Summarize this text in 2-3 sentences:

{text}
```

**Fix grammar:**
```
Fix grammar and spelling errors:

{text}
```

**Explain code:**
```
Explain what this code does:

{text}
```

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Run Action 1 on selected text | Your configured hotkey (default Ctrl+Shift+G) |
| Run Action 2 on selected text | Your configured hotkey (default Ctrl+Shift+H) |
| Copy result | Click "Copy" button |
| Dismiss popup | Click "Dismiss" or press Escape |

## Troubleshooting

**Hotkey not working:**
- Another app may be using the same hotkey - a tray balloon names the ones that failed to register
- Try a different key combination on the Hotkeys tab
- The two actions cannot share a hotkey; Settings rejects a duplicate on save

**"No text selected" error:**
- Make sure text is actually selected before pressing hotkey
- Some applications may not support standard copy (Ctrl+C)

**API errors:**
- `Invalid <provider> API key` - the key belongs to the other provider, or has been revoked
- `Insufficient credits` - top up your OpenRouter account
- `Model not found` - check the model name; OpenRouter models need the `vendor/model` form
- `Connection failed` - check internet connection

## License

MIT
