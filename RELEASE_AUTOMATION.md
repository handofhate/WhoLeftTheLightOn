# Release automation

This repository builds the release archive on the Moon Sweepers Windows runner and attaches it to a published GitHub Release.

## Repository setup

The self-hosted runner needs these labels:

- `self-hosted`
- `windows`
- `x64`
- `moonsweepers`

Set the repository variable `MOONSWEEPERS_GAME_ROOT` to the game's install directory. `DOTNET_PATH` is optional; if it is blank, the workflow uses `dotnet` from PATH.

## Nexus setup

The first Nexus upload must be done manually because the Nexus action updates an existing file; it does not create a new Nexus mod page or first file. After the first file is published, add:

- Repository secret `NEXUSMODS_API_KEY`
- Repository variables `NEXUSMODS_FILE_ID` and `NEXUSMODS_MOD_ID`

Later published GitHub Releases will then update that Nexus file automatically. If any of those values are missing, the workflow still builds and attaches the GitHub archive but skips Nexus safely.

## Publishing a release

1. Update `PluginVersion` in `Plugin.cs`.
2. Add a matching `Version x.y.z` section to `CHANGELOG.md`.
3. Commit and push the changes.
4. Create and publish a GitHub Release with a matching tag such as `v0.1.0`.

The release workflow builds `WhoLeftTheLightOn-x.y.z.zip`, attaches it to GitHub, and uploads to Nexus only when Nexus is configured.
