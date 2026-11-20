Version 1.0.0

- Expanded the mod from helmet-light memory into a complete lighting and vehicle-preference helper.
- Added per-vehicle light memory, including restoring lights on spawned vehicles before they are mounted.
- Added per-vehicle adjustable suction memory.
- Added per-vehicle Vehicle Boost Lock. Left Shift toggles the lock while driving; braking, stopping, and exiting no longer clear the vehicle's remembered boost mode.
- Made Vehicle Boost Lock enabled by default for new vehicles.
- Added Lunar Link and compatible mod-manager settings:
  - Remember Helmet Lighting
  - Remember Vehicle Lights
  - Remember Vehicle Suction
  - Vehicle Boost Lock
- Added the one-shot Clear Remembered Vehicle Settings reset action.
- Added safe settings migration and persistent per-vehicle preference storage.
- Added live Lunar Link reload support and documented hot-module installation and update behavior.
- Verified the release build against the Moon Sweepers 1.0.6 preview environment.

Version 0.1.1

- Added Lunar Link's GitHub release update feed.

Version 0.1.0

- Initial release.
- Remembers the player's last flashlight or night-vision mode between game sessions.
- Supports normal BepInEx loading and optional Lunar Link hot swapping.
