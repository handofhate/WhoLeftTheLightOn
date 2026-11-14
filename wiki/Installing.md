# Installing

## What you need

- **Moon Sweepers** with **[BepInEx 6.0.0-be.791](https://builds.bepinex.dev/projects/bepinex_be)** (Unity.IL2CPP, win-x64), run once so BepInEx has set itself up.
- The **[MoonSweepers BepInEx Patcher](https://www.nexusmods.com/moonsweepers/mods/9)**, which makes BepInEx work on Moon Sweepers.

Only you install it: it runs on your own game, and other players don't need it.

## Install

1. Download the zip from [Nexus Mods](https://www.nexusmods.com/moonsweepers/mods/157) or the [latest GitHub release](https://github.com/handofhate/WhoLeftTheLightOn/releases/latest).
2. Close the game and unzip it into the game folder (the one that holds `BepInEx/`). You get
   `BepInEx/plugins/WhoLeftTheLightOn/WhoLeftTheLightOn.dll`. A mod manager such as Vortex does this for you.
3. Start the game. The BepInEx log (`BepInEx/LogOutput.log`) says "Who Left The Light On? x.y.z loaded".

To run it hot with LunarLink instead, see [[With LunarLink]].

## Updating

Unzip the new version over the old one with the game closed, or let your mod manager update it. With LunarLink, F9 →
UPDATES does it in place without a restart ([[With LunarLink]]).

## Uninstalling

Close the game and delete `BepInEx/plugins/WhoLeftTheLightOn/` (or `BepInEx/hot/WhoLeftTheLightOn.dll`). Its preferences file,
`BepInEx/config/WhoLeftTheLightOn.preferences`, can go too.
