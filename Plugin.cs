using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MoonCleaners;
using UnityEngine;

namespace WhoLeftTheLightOn;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "tylerun.moonsweepers.wholeftthelighton";
    public const string PluginName = "Who Left The Light On?";
    public const string PluginVersion = "0.1.1";

    internal static ManualLogSource Logger;
    internal static Harmony Harmony;

    public override void Load()
    {
        StartRuntime(base.Log, registerNormalHost: true);
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded normally. Last light mode: {Preferences.ModeName}.");
    }

    internal static void StartHot()
    {
        StartRuntime(BepInEx.Logging.Logger.CreateLogSource(PluginName), registerNormalHost: false);
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded through Lunar Link. Last light mode: {Preferences.ModeName}.");
    }

    private static void StartRuntime(ManualLogSource source, bool registerNormalHost)
    {
        Logger = source;
        Preferences.Load();

        Harmony = new Harmony(PluginGuid);
        Harmony.PatchAll(typeof(Plugin).Assembly);

        if (registerNormalHost)
        {
            ClassInjector.RegisterTypeInIl2Cpp<PreferenceHost>();
            var host = new GameObject("Who Left The Light On?");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<PreferenceHost>();
        }
    }

    internal static string StopHot()
    {
        Harmony?.UnpatchSelf();
        Harmony = null;
        var oldLogger = Logger;
        Logger = null;
        if (oldLogger != null) BepInEx.Logging.Logger.Sources.Remove(oldLogger);
        return "light preference stopped";
    }

    internal static string UpdateFeed() => "github:handofhate/WhoLeftTheLightOn";
}

// Lunar Link discovers this public lifecycle class by method name. Plugin remains a normal BepInEx plugin fallback.
public sealed class WhoLeftTheLightOnHot
{
    public void Start(string state) => Plugin.StartHot();
    public string Stop() => Plugin.StopHot();
    public void Update() => Preferences.Update();
    public string UpdateFeed() => Plugin.UpdateFeed();
}

[HarmonyPatch(typeof(MoonWalker), nameof(MoonWalker.ToggleLamp))]
internal static class ToggleLampPatch
{
    private static void Postfix() => Preferences.RequestCapture();
}

[HarmonyPatch(typeof(MoonWalker), nameof(MoonWalker.SetNightVision))]
internal static class NightVisionPatch
{
    private static void Postfix() => Preferences.RequestCapture();
}

internal sealed class PreferenceHost : MonoBehaviour
{
    public PreferenceHost(IntPtr ptr) : base(ptr) { }

    private void Update() => Preferences.Update();
}

internal static class Preferences
{
    private const int Off = 0;
    private const int Flashlight = 1;
    private const int NightVision = 2;
    private const float StableSeconds = 0.35f;
    private const float ApplyQuietSeconds = 0.9f;

    private static readonly string FilePath = Path.Combine(Paths.ConfigPath, "WhoLeftTheLightOn.preferences");
    private static int savedMode;
    private static MoonWalker walker;
    private static bool appliedToCurrentWalker;
    private static bool applying;
    private static float ignoreUntil;
    private static int currentMode = -1;
    private static int candidateMode = -1;
    private static float candidateSince;
    private static float nextLookup;

    internal static string ModeName => savedMode switch
    {
        Flashlight => "flashlight",
        NightVision => "night vision",
        _ => "off",
    };

    internal static void Load()
    {
        savedMode = Off;
        try
        {
            if (!File.Exists(FilePath)) return;
            foreach (var line in File.ReadAllLines(FilePath))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2 && parts[0].Equals("Mode", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[1], out var mode))
                {
                    savedMode = Math.Clamp(mode, Off, NightVision);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not read saved light mode: " + e.Message);
        }
    }

    internal static void RequestCapture()
    {
        candidateMode = -1;
        nextLookup = 0f;
    }

    internal static void Update()
    {
        var now = Time.unscaledTime;
        if (now < nextLookup) return;
        nextLookup = now + 0.2f;

        var found = FindWalker();
        if (found != walker)
        {
            walker = found;
            appliedToCurrentWalker = false;
            currentMode = -1;
            candidateMode = -1;
        }

        if (walker == null) return;

        var lamp = FindLamp(walker);
        if (lamp == null) return;

        if (!appliedToCurrentWalker)
        {
            if (!ApplySavedMode(walker, lamp)) return;
            appliedToCurrentWalker = true;
            currentMode = savedMode;
            candidateMode = -1;
            ignoreUntil = now + ApplyQuietSeconds;
            return;
        }

        if (now < ignoreUntil || applying) return;

        var mode = ReadMode(walker, lamp);
        if (mode < 0) return;
        if (mode == currentMode)
        {
            candidateMode = -1;
            return;
        }

        if (candidateMode != mode)
        {
            candidateMode = mode;
            candidateSince = now;
            return;
        }

        if (now - candidateSince >= StableSeconds)
        {
            currentMode = mode;
            savedMode = mode;
            Save();
            candidateMode = -1;
            Plugin.Logger?.LogInfo("Saved light mode: " + ModeName + ".");
        }
    }

    private static MoonWalker FindWalker()
    {
        try
        {
            var camera = Camera.main;
            if (camera != null)
            {
                var fromParent = camera.GetComponentInParent<MoonWalker>();
                if (fromParent != null) return fromParent;
                var fromRoot = camera.transform.root.GetComponentInChildren<MoonWalker>(true);
                if (fromRoot != null) return fromRoot;
            }

            var all = UnityEngine.Object.FindObjectsOfType<MoonWalker>(true);
            return all != null && all.Length > 0 ? all[0] : null;
        }
        catch
        {
            return null;
        }
    }

    private static MoonHelmetLamp FindLamp(MoonWalker value)
    {
        try
        {
            if (value == null) return null;
            var child = value.GetComponentInChildren<MoonHelmetLamp>(true);
            if (child != null) return child;

            var all = UnityEngine.Object.FindObjectsOfType<MoonHelmetLamp>(true);
            if (all != null)
                foreach (var lamp in all)
                    if (lamp != null && lamp.walker == value) return lamp;
        }
        catch { }
        return null;
    }

    private static int ReadMode(MoonWalker value, MoonHelmetLamp lamp)
    {
        try
        {
            if (value.NightVisionOn) return NightVision;
            return lamp.On ? Flashlight : Off;
        }
        catch
        {
            return -1;
        }
    }

    private static bool ApplySavedMode(MoonWalker value, MoonHelmetLamp lamp)
    {
        try
        {
            applying = true;
            if (value.NightVisionOn) value.SetNightVision(false);
            if (lamp.On) value.ToggleLamp();

            if (savedMode == Flashlight && !lamp.On)
                value.ToggleLamp();
            else if (savedMode == NightVision)
                value.SetNightVision(true);

            return true;
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not restore light mode yet: " + e.Message);
            return false;
        }
        finally
        {
            applying = false;
        }
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllLines(FilePath, new[]
            {
                "# Who Left The Light On? persistent preference",
                $"Mode={savedMode}",
            });
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not save light mode: " + e.Message);
        }
    }
}
