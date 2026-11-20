# With LunarLink

Who Left The Light On? is a normal BepInEx plugin and needs nothing else. It is also **hot-compatible**: with
[LunarLink](https://swheettaos.github.io/lunarlink/) installed it can be loaded, reloaded, turned off, configured, and updated while the game runs.

| | On its own | With LunarLink |
|---|---|---|
| **Install it in** | `BepInEx/plugins/WhoLeftTheLightOn/` | `BepInEx/hot/WhoLeftTheLightOn.dll` (the DLL straight in, not in a subfolder) |
| **Updating** | Download, close the game, unzip, start again | F9 → UPDATES → **Update**: installed and reloaded in place |
| **Settings** | Edit `BepInEx/config/tylerun.moonsweepers.wholeftthelighton.cfg` | F9 → HOT PLUGINS → expand the mod row |
| **Turning it off** | Remove the DLL, restart | F9 → HOT PLUGINS → **Disable**: unloaded from memory at once |

## Running it hot

1. Install [LunarLink](https://github.com/swheettaos/lunarlink/releases/latest).
2. Put `WhoLeftTheLightOn.dll` straight in `BepInEx/hot/`, and **remove it from `BepInEx/plugins/`**. A plugin in both folders can load twice, so LunarLink refuses the hot copy.
3. Start the game, load a moon, and turn it on: **F9 → HOT PLUGINS → Enable**. A new hot plugin starts off; it stays on from then on.

## Settings in the F9 window

Expand the Who Left The Light On? row to find:

- **Remember Helmet Lighting**
- **Remember Vehicle Lights**
- **Remember Vehicle Suction**
- **Vehicle Boost Lock**
- **Clear Remembered Vehicle Settings** — a one-shot reset that returns itself to Off after Apply.

The four functional settings default to On. Hot settings are applied by LunarLink's normal **Apply** action and may reload the module.

## Updates

Who Left The Light On? tells LunarLink where its releases are (`github:handofhate/WhoLeftTheLightOn`), so **F9 → UPDATES** lists it.
Press **Check for updates** (or enable the start-up check); when a new version is available, choose **Update**. For the hot copy, LunarLink downloads the published GitHub release and reloads it in place.

More: [LunarLink's wiki](https://github.com/swheettaos/lunarlink/wiki), and its [Installing Plugins](https://github.com/swheettaos/lunarlink/wiki/Installing-Plugins) page.
