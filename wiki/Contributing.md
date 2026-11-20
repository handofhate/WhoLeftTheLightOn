# Contributing

Bug reports, ideas and pull requests are welcome: [open an issue](https://github.com/handofhate/WhoLeftTheLightOn/issues/new/choose).

## Building

The project builds against your own game's files (BepInEx's interop assemblies), so you need the game
with BepInEx installed and run once, and the .NET SDK.

```powershell
./build.ps1 -MoonSweepersGameRoot "C:\Program Files (x86)\Steam\steamapps\common\Moon Sweepers"
./build.ps1 -Install    # also copies the DLL into BepInEx/plugins/WhoLeftTheLightOn/
```

## Releases

See [RELEASE_AUTOMATION.md](https://github.com/handofhate/WhoLeftTheLightOn/blob/main/RELEASE_AUTOMATION.md): bump `PluginVersion` in `Plugin.cs`, add a matching
`Version x.y.z` section to `CHANGELOG.md`, commit and push the source, then create and publish a GitHub Release with the
tag `vx.y.z`. A push alone does not publish to Nexus. The published-release workflow builds the zip, attaches it, and
updates the existing Nexus file when the repository's Nexus secret and variables are configured.

## Docs

- The website is `docs/index.html`, published by GitHub Pages from `docs/` on `main`.
- The wiki is written in `wiki/` and published by `.github/workflows/wiki.yml` on every push to `main`
  (`tools/wiki.py` fills in the changelog). Edit the files in `wiki/`, not the wiki itself: it's overwritten.
