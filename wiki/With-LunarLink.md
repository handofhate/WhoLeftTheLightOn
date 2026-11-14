# With LunarLink

Who Left The Light On is a normal BepInEx plugin and needs nothing else. It's also **hot-compatible**: with
[LunarLink](https://swheettaos.github.io/lunarlink/) installed it can be loaded, reloaded, turned off and updated while the game runs.

| | On its own | With LunarLink |
|---|---|---|
| **Install it in** | `BepInEx/plugins/WhoLeftTheLightOn/` | `BepInEx/hot/WhoLeftTheLightOn.dll` (the DLL straight in, not in a subfolder) |
| **Updating** | Download, close the game, unzip, start again | F9 → UPDATES → **Update**: installed in place, **no restart** |
| **Turning it off** | Remove the DLL, restart | F9 → HOT PLUGINS → **Disable**: unloaded from memory at once |

## Running it hot

1. Install [LunarLink](https://github.com/swheettaos/lunarlink/releases/latest).
2. Put `WhoLeftTheLightOn.dll` straight in `BepInEx/hot/`, and **remove it from `BepInEx/plugins/`**. A plugin in both
   folders would load twice, so LunarLink refuses the hot copy.
3. Start the game, load a moon, and turn it on: **F9 → HOT PLUGINS → Enable**. A new hot plugin starts off; it stays
   on from then on.

## Updates

Who Left The Light On tells LunarLink where its releases are (`github:handofhate/WhoLeftTheLightOn`), so **F9 → UPDATES** lists it.
Press **Check for updates** (or tick "Check for updates at game start"); when a new version is out, **Update**
downloads it from GitHub and, for the hot copy, reloads it in place. A copy in `BepInEx/plugins/` is updated too, but
needs a restart to load.

More: [LunarLink's wiki](https://github.com/swheettaos/lunarlink/wiki), and its [Installing Plugins](https://github.com/swheettaos/lunarlink/wiki/Installing-Plugins) page.
