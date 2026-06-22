# XIVClearSS

A Dalamud plugin for FFXIV that temporarily switches your game to a custom high resolution so you can take screenshots beyond what the in-game settings allow, then restores your original resolution when you're done.

## Requirements

> **The game must be in Windowed mode** for resolution switching to work. Fullscreen and Borderless Windowed are not supported. You can change this in `System Configuration → Screen Settings → Screen Mode`.

## Installation (Dev Plugin)

1. Build the plugin with `make build`
2. In-game, open `/xlsettings` → **Experimental** tab
3. Under **Dev Plugin Locations**, add the path to the DLL:
   ```
   /Users/<you>/Library/Application Support/XIV on Mac/dalamud/Hooks/dev/plugins/XIVClearSS.dll
   ```
4. Open `/xlplugins` → **Dev Plugins** tab and enable XIVClearSS

## Usage

### Opening the window
```
/clearss
```

### Commands
| Command | Action |
|---|---|
| `/clearss` | Open the settings window |
| `/clearss go` | Switch to your configured high resolution |
| `/clearss stop` | Restore your original resolution |

### Workflow

1. Open `/clearss` and pick a resolution preset (2K / 4K / 8K) or enter a custom size
2. Optionally set a **toggle hotkey** so you don't need to type commands
3. Get into position in-game (GPose, cutscene, etc.)
4. Press your hotkey or run `/clearss go` — the window will resize to your target resolution
5. Take your screenshot using the game's built-in screenshot key (default: `ScrollLock`)
6. Press your hotkey again or run `/clearss stop` to restore your original resolution

### Resolution Presets

| Preset | Dimensions | Megapixels |
|---|---|---|
| 2K | 2560 × 1440 | 3.7 MP |
| 4K | 3840 × 2160 | 8.3 MP |
| 8K | 7680 × 4320 | 33 MP |

> **Note on 8K:** Very high resolutions put significant load on the GPU. 8K should work but may cause instability on lower-end hardware. Start with 4K to verify things work before trying 8K.

### Hotkey Setup

1. Open `/clearss`
2. Click the button next to **Toggle Hotkey**
3. Press the key you want to use (press Esc to cancel)
4. The hotkey now toggles between high-res and your original resolution

## Tips

- Always restore before reloading the plugin (`/clearss stop` or the Stop button) to avoid leaving your resolution in an unexpected state
- The plugin automatically restores your original resolution when it is unloaded or the game closes
- Custom dimensions can be typed directly into the W / H fields if you need a non-standard size
- The game's screenshots are saved to `Documents/My Games/FINAL FANTASY XIV - A Realm Reborn/screenshots/`
