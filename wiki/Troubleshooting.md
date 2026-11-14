# Troubleshooting

**Check it loaded.** `BepInEx/LogOutput.log` should have "Who Left The Light On? x.y.z loaded" (or "loaded through Lunar
Link"), followed by "Last light mode: …". No line at all means BepInEx didn't load it: check the DLL is at
`BepInEx/plugins/WhoLeftTheLightOn/WhoLeftTheLightOn.dll` and that BepInEx itself works (its log exists and other
mods load).

**The mode isn't restored.** The mode is saved only after it has held for a moment. Turn the light the way you want,
wait a second, and quit normally. The log's "Last light mode" line on the next start shows what was saved.

**The wrong mode comes back.** Delete `BepInEx/config/WhoLeftTheLightOn.preferences` with the game closed to start fresh, then set your mode once.

**Running it with LunarLink and nothing happens.** A new hot plugin starts **off**: F9 → HOT PLUGINS → **Enable**. If F9
says it was refused, a second copy is still in `BepInEx/plugins/`: remove one.

Still stuck? [Open an issue](https://github.com/handofhate/WhoLeftTheLightOn/issues/new/choose) with the log lines and the preferences file.
