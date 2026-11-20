# Who Left The Light On?

[Website](https://handofhate.github.io/WhoLeftTheLightOn/) · [Wiki](https://github.com/handofhate/WhoLeftTheLightOn/wiki) · [Nexus Mods](https://www.nexusmods.com/moonsweepers/mods/157) · [Report a problem](https://github.com/handofhate/WhoLeftTheLightOn/issues/new/choose)

Who Left The Light On? remembers useful lighting and vehicle preferences in Moon Sweepers and restores them when they are needed. It works as a normal BepInEx plugin or as a Lunar Link hot module.

## Features

- Remembers the player's last helmet lighting mode: Off, Flashlight, or Night Vision.
- Remembers vehicle lights separately for each vehicle, including before the vehicle is mounted.
- Remembers adjustable suction power separately for each vehicle.
- Provides a per-vehicle Vehicle Boost Lock. When enabled, Left Shift toggles persistent boost mode; braking, stopping, and exiting do not clear the vehicle's setting. Left Shift while driving turns it off again.
- Stores preferences locally and does not modify save-game data.
- Supports Lunar Link hot loading, live reloads, and in-game updates.

## Settings

The settings are exposed in the mod's expanded row in Lunar Link's **HOT PLUGINS** tab and in compatible mod-manager settings screens. All four functional settings default to **On**:

| Setting | What it does |
|---|---|
| **Remember Helmet Lighting** | Restores the last Off, Flashlight, or Night Vision mode. |
| **Remember Vehicle Lights** | Remembers each vehicle's lights independently. |
| **Remember Vehicle Suction** | Remembers each vehicle's adjustable suction setting independently. |
| **Vehicle Boost Lock** | Makes Left Shift toggle persistent boost mode for the current vehicle. |

The **Reset → Clear Remembered Vehicle Settings** action is a one-shot reset. Set it to On and apply the setting once; it clears stored vehicle light, suction, and boost preferences, then returns itself to Off. Helmet-light memory is not cleared by this action.

Settings are stored in `BepInEx/config/tylerun.moonsweepers.wholeftthelighton.cfg`. Remembered runtime preferences are stored in `BepInEx/config/WhoLeftTheLightOn.preferences`.

## Installation

For a normal BepInEx installation, close the game and place the DLL here:

```text
BepInEx/plugins/WhoLeftTheLightOn/WhoLeftTheLightOn.dll
```

With Lunar Link installed, use the hot-module installation instead:

```text
BepInEx/hot/WhoLeftTheLightOn.dll
```

Do not leave an active copy in both locations. A normal copy and a hot copy can load twice. Use Lunar Link's F9 window to enable, reload, disable, or update the hot module.

## Requirements

- Moon Sweepers with BepInEx 6 IL2CPP installed and initialized.
- The Moon Sweepers BepInEx compatibility patcher.
- Lunar Link is optional, but required for hot loading and in-game updates.

## Updating

Normal installation: download the latest release, close the game, and replace the DLL.

Lunar Link installation: open F9 → **UPDATES**, check for updates, and choose **Update**. The hot copy can reload without a game restart.

## Building

The project builds against the game's BepInEx interop assemblies and the .NET 6 SDK:

```powershell
./build.ps1 -MoonSweepersGameRoot "C:\Program Files (x86)\Steam\steamapps\common\Moon Sweepers"
./build.ps1 -Install
```

For release packaging, use `tools/Package-WhoLeftTheLightOn.ps1`. See [RELEASE_AUTOMATION.md](RELEASE_AUTOMATION.md) for the GitHub Release and Nexus workflow.
