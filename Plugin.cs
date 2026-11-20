using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MoonCleaners;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WhoLeftTheLightOn;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "tylerun.moonsweepers.wholeftthelighton";
    public const string PluginName = "Who Left The Light On?";
    public const string PluginVersion = "1.0.0";

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
        Settings.Bind();
        Preferences.Load();
        if (Settings.ConsumeClearRememberedVehicleSettings())
            Preferences.ClearRememberedVehicleSettings();

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
    public void LateUpdate() => Preferences.LateUpdate();
    public string UpdateFeed() => Plugin.UpdateFeed();
}

internal static class Settings
{
    private static ConfigFile config;

    internal static ConfigEntry<bool> RememberHelmetLighting { get; private set; }
    internal static ConfigEntry<bool> RememberVehicleLights { get; private set; }
    internal static ConfigEntry<bool> RememberVehicleSuction { get; private set; }
    internal static ConfigEntry<bool> VehicleBoostLock { get; private set; }
    internal static ConfigEntry<bool> ClearRememberedVehicleSettings { get; private set; }

    internal static void Bind()
    {
        var path = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid + ".cfg");
        var legacyMemory = MigrateLegacyMemorySection(path, out var existingKeys);
        var legacyMaintenance = MigrateLegacySection(path, "Maintenance");
        config = new ConfigFile(path, false);

        RememberHelmetLighting = config.Bind(
            "Lights",
            "Remember Helmet Lighting",
            true,
            "Restore the last helmet lighting mode: off, flashlight, or night vision.");

        RememberVehicleLights = config.Bind(
            "Lights",
            "Remember Vehicle Lights",
            true,
            "Remember each vehicle's light state separately.");

        RememberVehicleSuction = config.Bind(
            "Driving",
            "Remember Vehicle Suction",
            true,
            "Remember each vehicle's adjustable suction setting separately.");

        VehicleBoostLock = config.Bind(
            "Driving",
            "Vehicle Boost Lock",
            true,
            "Left Shift toggles persistent boost mode for the current vehicle. Braking, stopping, and exiting do not clear it.");

        ClearRememberedVehicleSettings = config.Bind(
            "Reset",
            "Clear Remembered Vehicle Settings",
            false,
            "Set to true and Apply once to clear all stored per-vehicle lights, suction, and boost preferences. It resets itself to false.");

        MigrateValue(legacyMemory, existingKeys, "Remember Helmet Lighting", "Lights", RememberHelmetLighting);
        MigrateValue(legacyMemory, existingKeys, "Remember Vehicle Lights", "Lights", RememberVehicleLights);
        MigrateValue(legacyMemory, existingKeys, "Remember Vehicle Suction", "Driving", RememberVehicleSuction);
        MigrateValue(legacyMaintenance, existingKeys, "Clear Remembered Vehicle Settings", "Reset", ClearRememberedVehicleSettings);

        config.Save();
    }

    internal static bool ConsumeClearRememberedVehicleSettings()
    {
        if (!ClearRememberedVehicleSettings.Value) return false;
        ClearRememberedVehicleSettings.Value = false;
        config.Save();
        return true;
    }

    private static void MigrateValue(Dictionary<string, string> legacyMemory, HashSet<string> existingKeys,
        string key, string section, ConfigEntry<bool> entry)
    {
        if (existingKeys.Contains(section + "|" + key) || !legacyMemory.TryGetValue(key, out var value)) return;
        if (bool.TryParse(value, out var parsed)) entry.Value = parsed;
    }

    private static Dictionary<string, string> MigrateLegacyMemorySection(string path, out HashSet<string> existingKeys)
    {
        existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var legacy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!File.Exists(path)) return legacy;

            var lines = File.ReadAllLines(path);
            var output = new List<string>(lines.Length);
            var section = "";
            var inLegacyMemory = false;
            var changed = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    inLegacyMemory = section.Equals("Memory", StringComparison.OrdinalIgnoreCase);
                    if (inLegacyMemory)
                    {
                        changed = true;
                        continue;
                    }
                }

                if (inLegacyMemory) continue;

                if (!trimmed.StartsWith("#", StringComparison.Ordinal) && !trimmed.StartsWith(";", StringComparison.Ordinal))
                {
                    var equals = line.IndexOf('=');
                    if (equals > 0 && (section.Equals("Lights", StringComparison.OrdinalIgnoreCase) ||
                                      section.Equals("Driving", StringComparison.OrdinalIgnoreCase) ||
                                      section.Equals("Reset", StringComparison.OrdinalIgnoreCase)))
                    {
                        var key = line.Substring(0, equals).Trim();
                        existingKeys.Add(section + "|" + key);
                    }
                }

                output.Add(line);
            }

            if (!changed) return legacy;

            section = "";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    continue;
                }

                if (section.Equals("Memory", StringComparison.OrdinalIgnoreCase))
                {
                    var equals = line.IndexOf('=');
                    if (equals > 0)
                        legacy[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
                }
            }

            File.WriteAllLines(path, output);
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not migrate settings categories: " + e.Message);
        }

        return legacy;
    }

    private static Dictionary<string, string> MigrateLegacySection(string path, string sectionName)
    {
        var legacy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!File.Exists(path)) return legacy;

            var lines = File.ReadAllLines(path);
            var output = new List<string>(lines.Length);
            var section = "";
            var inLegacySection = false;
            var changed = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    inLegacySection = section.Equals(sectionName, StringComparison.OrdinalIgnoreCase);
                    if (inLegacySection)
                    {
                        changed = true;
                        continue;
                    }
                }

                if (inLegacySection)
                {
                    var equals = line.IndexOf('=');
                    if (equals > 0)
                        legacy[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
                    continue;
                }

                output.Add(line);
            }

            if (changed) File.WriteAllLines(path, output);
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not migrate the " + sectionName + " settings category: " + e.Message);
        }

        return legacy;
    }
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

[HarmonyPatch(typeof(MoonPrototypeVehicle), nameof(MoonPrototypeVehicle.DriverControls))]
internal static class VehicleDriverControlsPatch
{
    private static void Postfix(MoonPrototypeVehicle __instance) => Preferences.ReapplyVehicleBoost(__instance);
}

internal sealed class PreferenceHost : MonoBehaviour
{
    public PreferenceHost(IntPtr ptr) : base(ptr) { }

    private void Update() => Preferences.Update();
    private void LateUpdate() => Preferences.LateUpdate();
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
    private static readonly Dictionary<string, VehicleState> savedVehicleStates = new(StringComparer.Ordinal);
    private static MoonWalker walker;
    private static MoonPrototypeVehicle vehicle;
    private static bool appliedToCurrentWalker;
    private static bool appliedToCurrentVehicle;
    private static bool applying;
    private static float ignoreUntil;
    private static float vehicleIgnoreUntil;
    private static int currentMode = -1;
    private static int candidateMode = -1;
    private static float candidateSince;
    private static VehicleState currentVehicleState;
    private static bool hasCurrentVehicleState;
    private static VehicleState candidateVehicleState;
    private static bool hasCandidateVehicleState;
    private static float vehicleCandidateSince;
    private static float nextLookup;
    private static int lastShiftFrame = -1;

    private sealed class VehicleState
    {
        internal bool LightsOn;
        internal int Suction;
        internal bool BoostLocked;

        internal VehicleState Copy() => new()
        {
            LightsOn = LightsOn,
            Suction = Suction,
            BoostLocked = BoostLocked,
        };

        internal bool EqualsState(VehicleState other) =>
            other != null && LightsOn == other.LightsOn && Suction == other.Suction && BoostLocked == other.BoostLocked;
    }

    internal static string ModeName => savedMode switch
    {
        Flashlight => "flashlight",
        NightVision => "night vision",
        _ => "off",
    };

    internal static void Load()
    {
        savedMode = Off;
        savedVehicleStates.Clear();
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
                }
                else if (parts.Length == 2 && parts[0].Equals("Vehicle", StringComparison.OrdinalIgnoreCase))
                {
                    LoadVehicleState(parts[1]);
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

    internal static void ClearRememberedVehicleSettings()
    {
        savedVehicleStates.Clear();
        vehicle = null;
        appliedToCurrentVehicle = false;
        hasCurrentVehicleState = false;
        hasCandidateVehicleState = false;
        Save();
        Plugin.Logger?.LogInfo("Cleared remembered vehicle settings.");
    }

    internal static void Update()
    {
        UpdateVehicleFrame();

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

        UpdateVehicle(now, walker);

        if (!Settings.RememberHelmetLighting.Value)
        {
            appliedToCurrentWalker = true;
            return;
        }

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

    internal static void LateUpdate() => UpdateVehicleFrame();

    private static void UpdateVehicleFrame()
    {
        if (vehicle == null || !appliedToCurrentVehicle || !hasCurrentVehicleState) return;

        try
        {
            if (!vehicle.IsDriven) return;
        }
        catch
        {
            return;
        }

        if (!Settings.VehicleBoostLock.Value) return;

        if (IsLeftShiftPressed() && lastShiftFrame != Time.frameCount)
        {
            lastShiftFrame = Time.frameCount;
            currentVehicleState.BoostLocked = !currentVehicleState.BoostLocked;
            if (TryGetVehicleId(vehicle, out var vehicleId))
                savedVehicleStates[vehicleId] = currentVehicleState.Copy();
            Save();
            Plugin.Logger?.LogInfo($"Vehicle boost lock {(currentVehicleState.BoostLocked ? "enabled" : "disabled")}.");
        }

        ApplyBoostLock(vehicle, currentVehicleState);
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

    private static MoonPrototypeVehicle FindVehicle(MoonWalker value)
    {
        try
        {
            var all = UnityEngine.Object.FindObjectsOfType<MoonPrototypeVehicle>(true);
            MoonPrototypeVehicle drivenFallback = null;
            if (all == null) return null;

            RestoreStoredVehicleLights(all);

            foreach (var candidate in all)
            {
                if (candidate == null || !candidate.IsDriven) continue;
                drivenFallback ??= candidate;

                try
                {
                    if (candidate.Owner == value) return candidate;
                }
                catch { }
            }

            return drivenFallback;
        }
        catch
        {
            return null;
        }
    }

    private static void RestoreStoredVehicleLights(MoonPrototypeVehicle[] all)
    {
        if (!Settings.RememberVehicleLights.Value || all == null) return;

        foreach (var candidate in all)
        {
            if (candidate == null) continue;

            try
            {
                if (!TryGetVehicleId(candidate, out var vehicleId) ||
                    !savedVehicleStates.TryGetValue(vehicleId, out var saved) ||
                    candidate.LightsOn == saved.LightsOn)
                    continue;

                candidate.SetLights(saved.LightsOn);
            }
            catch
            {
                // The vehicle may still be finishing initialization; the next discovery pass retries it.
            }
        }
    }

    private static void UpdateVehicle(float now, MoonWalker value)
    {
        var found = FindVehicle(value);
        if (found != vehicle)
        {
            vehicle = found;
            appliedToCurrentVehicle = false;
            hasCurrentVehicleState = false;
            hasCandidateVehicleState = false;
        }

        if (vehicle == null) return;
        if (!TryGetVehicleId(vehicle, out var vehicleId)) return;
        if (!TryReadVehicleState(vehicle, out var state)) return;

        if (!appliedToCurrentVehicle)
        {
            if (!ApplySavedVehicleState(vehicle, vehicleId, state)) return;
            appliedToCurrentVehicle = true;
            currentVehicleState = ReadVehicleState(vehicle);
            if (savedVehicleStates.TryGetValue(vehicleId, out var savedState))
                currentVehicleState.BoostLocked = savedState.BoostLocked;
            else
                currentVehicleState.BoostLocked = Settings.VehicleBoostLock.Value;
            hasCurrentVehicleState = currentVehicleState != null;
            hasCandidateVehicleState = false;
            vehicleIgnoreUntil = now + ApplyQuietSeconds;

            if (!savedVehicleStates.ContainsKey(vehicleId) && hasCurrentVehicleState &&
                (Settings.RememberVehicleLights.Value || Settings.RememberVehicleSuction.Value || Settings.VehicleBoostLock.Value))
            {
                savedVehicleStates[vehicleId] = currentVehicleState.Copy();
                Save();
            }

            return;
        }

        if (!hasCurrentVehicleState || currentVehicleState == null) return;
        if (!Settings.RememberVehicleLights.Value)
            state.LightsOn = currentVehicleState.LightsOn;
        if (!Settings.RememberVehicleSuction.Value)
            state.Suction = currentVehicleState.Suction;
        state.BoostLocked = currentVehicleState.BoostLocked;

        if (now < vehicleIgnoreUntil || applying || !hasCurrentVehicleState) return;

        if (currentVehicleState.EqualsState(state))
        {
            hasCandidateVehicleState = false;
            return;
        }

        if (!hasCandidateVehicleState || !candidateVehicleState.EqualsState(state))
        {
            candidateVehicleState = state.Copy();
            hasCandidateVehicleState = true;
            vehicleCandidateSince = now;
            return;
        }

        if (now - vehicleCandidateSince >= StableSeconds)
        {
            currentVehicleState = candidateVehicleState.Copy();
            savedVehicleStates[vehicleId] = currentVehicleState.Copy();
            Save();
            hasCandidateVehicleState = false;
            Plugin.Logger?.LogInfo($"Saved vehicle {vehicleId} settings: lights={(currentVehicleState.LightsOn ? "on" : "off")}, suction={currentVehicleState.Suction}.");
        }
    }

    private static bool TryGetVehicleId(MoonPrototypeVehicle value, out string vehicleId)
    {
        vehicleId = null;
        try
        {
            vehicleId = value.StorageId;
            return !string.IsNullOrWhiteSpace(vehicleId);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadVehicleState(MoonPrototypeVehicle value, out VehicleState state)
    {
        state = null;
        try
        {
            state = ReadVehicleState(value);
            return state != null;
        }
        catch
        {
            return false;
        }
    }

    private static VehicleState ReadVehicleState(MoonPrototypeVehicle value)
    {
        if (value == null) return null;

        var suction = value.Suction;
        if (!value.SuctionAdjustable)
            suction = 0;

        return new VehicleState
        {
            LightsOn = value.LightsOn,
            Suction = suction,
        };
    }

    private static bool IsLeftShiftPressed()
    {
        try
        {
            return Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
        }
        catch
        {
            return false;
        }
    }

    private static void ApplyBoostLock(MoonPrototypeVehicle value, VehicleState state)
    {
        if (value == null || state == null || !Settings.VehicleBoostLock.Value || !state.BoostLocked || !value.IsDriven) return;

        try
        {
            var shouldBoost = value.DriveInput > 0.01f;
            if (value.boost != shouldBoost)
                value.boost = shouldBoost;
            if (value.boostLatched != shouldBoost)
                value.boostLatched = shouldBoost;
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not apply vehicle boost lock: " + e.Message);
        }
    }

    internal static void ReapplyVehicleBoost(MoonPrototypeVehicle value)
    {
        if (value == vehicle && hasCurrentVehicleState)
            ApplyBoostLock(value, currentVehicleState);
    }

    private static bool ApplySavedVehicleState(MoonPrototypeVehicle value, string vehicleId, VehicleState current)
    {
        if (!savedVehicleStates.TryGetValue(vehicleId, out var saved)) return true;

        try
        {
            applying = true;
            if (Settings.RememberVehicleLights.Value && value.LightsOn != saved.LightsOn)
                value.SetLights(saved.LightsOn);

            if (Settings.RememberVehicleSuction.Value && value.SuctionAdjustable)
            {
                var bottom = MoonVehicleUpgrades.SuctionBottom(value.SuctionLevel);
                var top = MoonVehicleUpgrades.SuctionTop(value.SuctionLevel);
                var target = Math.Clamp(saved.Suction, bottom, top);
                if (value.Suction != target)
                    value.Suction = target;
            }

            if (Settings.VehicleBoostLock.Value && saved.BoostLocked)
            {
                if (!value.boost) value.boost = true;
                if (!value.boostLatched) value.boostLatched = true;
            }

            return true;
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning($"Could not restore vehicle {vehicleId} settings yet: {e.Message}");
            return false;
        }
        finally
        {
            applying = false;
        }
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
            var lines = new List<string>
            {
                "# Who Left The Light On? persistent preference",
                $"Mode={savedMode}",
            };

            foreach (var pair in savedVehicleStates)
            {
                var encodedId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pair.Key));
                lines.Add($"Vehicle={encodedId}|{(pair.Value.LightsOn ? 1 : 0)}|{pair.Value.Suction}|{(pair.Value.BoostLocked ? 1 : 0)}");
            }

            File.WriteAllLines(FilePath, lines);
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogWarning("Could not save light mode: " + e.Message);
        }
    }

    private static void LoadVehicleState(string value)
    {
        try
        {
            var parts = value.Split('|');
            if (parts.Length < 3 || !int.TryParse(parts[1], out var lights) || !int.TryParse(parts[2], out var suction)) return;
            var boostLocked = parts.Length < 4 || (int.TryParse(parts[3], out var boost) && boost != 0);

            var vehicleId = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
            if (!string.IsNullOrWhiteSpace(vehicleId))
            {
                savedVehicleStates[vehicleId] = new VehicleState
                {
                    LightsOn = lights != 0,
                    Suction = suction,
                    BoostLocked = boostLocked,
                };
            }
        }
        catch
        {
            // Ignore malformed vehicle entries so one bad preference cannot block the rest.
        }
    }
}
