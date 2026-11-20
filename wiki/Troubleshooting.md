# Troubleshooting

**Check it loaded.** `BepInEx/LogOutput.log` should have "Who Left The Light On? 1.0.0 loaded" or "loaded through Lunar Link".

## A setting does not change

Open the expanded mod row in F9 → **HOT PLUGINS**, change the setting, and press **Apply**. The reset action is one-shot:
set **Clear Remembered Vehicle Settings** to On, apply it once, and confirm it returned to Off.

The settings file is `BepInEx/config/tylerun.moonsweepers.wholeftthelighton.cfg`.

## A remembered value does not restore

Make sure the relevant setting is On. Helmet lighting is restored once your character is ready. Vehicle lights are restored
when the vehicle has spawned; suction and boost state are applied when the vehicle is ready for use.

Vehicle preferences are keyed by the game's vehicle storage ID. If a vehicle was replaced or newly purchased, it has its own
new preference until you use it.

## The wrong preference comes back

Use **Clear Remembered Vehicle Settings** in the LunarLink settings, or close the game and remove
`BepInEx/config/WhoLeftTheLightOn.preferences` to start all preferences fresh.

## Vehicle Boost Lock is not behaving as expected

With **Vehicle Boost Lock** enabled, Left Shift toggles the lock while driving. Braking, stopping, and exiting do not clear
the saved per-vehicle mode. Left Shift while driving turns it off. The feature does not refill boost or change the game's
boost physics.

## Running it with LunarLink does nothing

A new hot plugin starts **off**: F9 → HOT PLUGINS → **Enable**. Do not keep an active copy in both `BepInEx/hot/` and
`BepInEx/plugins/`; choose one installation mode.

Still stuck? [Open an issue](https://github.com/handofhate/WhoLeftTheLightOn/issues/new/choose) with the relevant log lines,
the settings file, and the preferences file.
