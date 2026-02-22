# TextToAI

A lightweight Windows utility that sends selected text to OpenAI with a single hotkey. Select text in any application, press your hotkey, and get AI-processed results instantly.

## Features

- **Global hotkey** - works from any application
- **System tray** - runs quietly in the background
- **Configurable prompt** - customize how text is processed
- **Multiple models** - supports GPT-4o, GPT-4o-mini, GPT-4-turbo, GPT-3.5-turbo
- **Fast** - minimal latency, shows response time

## Installation

### Requirements
- Windows 10/11
- .NET 10.0 Runtime (or newer)
- OpenAI API key

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
2. **Configure settings** - right-click tray icon → Settings
   - Enter your OpenAI API key
   - Choose a model
   - Set your hotkey (default: Ctrl+Shift+G)
   - Write your prompt (use `{text}` as placeholder)
3. **Select text** in any application
4. **Press hotkey** - popup shows AI response
5. **Copy or Dismiss** the result

## Configuration

Settings are stored in `%AppData%\TextToAI\config.json`:

```json
{
  "apiKey": "sk-...",
  "model": "gpt-4o",
  "hotkey": "Ctrl+Shift+G",
  "prompt": "Translate to English:\n\n{text}"
}
```

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
| Process selected text | Your configured hotkey |
| Copy result | Click "Copy" button |
| Dismiss popup | Click "Dismiss" or press Escape |

## Troubleshooting

**Hotkey not working:**
- Another app may be using the same hotkey
- Try a different key combination in Settings

**"No text selected" error:**
- Make sure text is actually selected before pressing hotkey
- Some applications may not support standard copy (Ctrl+C)

**API errors:**
- Check your API key is valid
- Verify you have API credits available
- Check internet connection

## License

MIT
