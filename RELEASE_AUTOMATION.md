# Release automation

This repository builds the release archive on the Moon Sweepers Windows runner, attaches it to a published GitHub Release,
and updates the existing Nexus Mods file.

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
- Repository variables `NEXUSMODS_FILE_ID` and `NEXUSMODS_MOD_ID` (`NEXUSMODS_MOD_ID` is the numeric ID from the public Nexus page URL; the workflow resolves it to the v3 API ID automatically)

Later **published** GitHub Releases will then update that Nexus file automatically. A manual workflow dispatch also
performs the Nexus upload, providing a safe retry path if a release build succeeds but the Nexus update needs to be
retried. The workflow also has a changelog-only repair option, which adds the release notes to Nexus without uploading
the archive again. A normal push to GitHub does not publish to Nexus. If any of those values are missing, the workflow
still builds and attaches the GitHub archive but skips Nexus safely.

## Publishing a release

1. Update `PluginVersion` in `Plugin.cs`.
2. Add a matching `Version x.y.z` section to `CHANGELOG.md`.
3. Update the README, Nexus description, and relevant wiki/website documentation.
4. Build and test the release locally.
5. Commit and push the changes.
6. Create and **publish** a GitHub Release with a matching tag such as `v1.0.0`.

The release workflow builds `WhoLeftTheLightOn-x.y.z.zip`, attaches it to GitHub, and uploads to Nexus only when Nexus is configured.
