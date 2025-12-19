using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using PropHunt.Settings;
using Reactor;
using Reactor.Utilities;

namespace PropHunt;

[BepInPlugin("com.SuperIdol.amongus.prophunt", "Prop Hunt", "2025.11.18")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class PropHuntPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new("com.SuperIdol.amongus.prophunt");
    public ConfigEntry<bool> IsPropHunt { get; private set; }
    public ConfigEntry<float> MissTimePenalty { get; private set; }
    public ConfigEntry<bool> Infection { get; private set; }

    public static bool isPropHunt = true;
    public static float missTimePenalty = 10f;
    public static bool infection = false;

    public const float propMoveSpeed = 0.5f;
    public const float maxPropDistance = 0.6f;

    public static PropHuntPlugin Instance;

    public override void Load()
    {        
        Instance = PluginSingleton<PropHuntPlugin>.Instance;

        IsPropHunt = Config.Bind("Prop Hunt", "Prop Hunt", false);
        MissTimePenalty = Config.Bind("Prop Hunt", "Miss Penalty", 10f);
        Infection = Config.Bind("Prop Hunt", "Infection", false);

        PropHuntPreset.SetupPreset();
        PropHuntSettings.SetupCustomSettings();

        Harmony.PatchAll(typeof(Patches));
        Harmony.PatchAll(typeof(PropHuntPreset));
        Harmony.PatchAll(typeof(PropHuntSettings));
    }
}
