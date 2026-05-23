using Kingmaker.EntitySystem.Persistence;
using ToyBox.Features.SettingsFeatures.Blueprints;

namespace ToyBox.Features.SettingsTab.Game;

#warning Beta
[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.SettingsTab.Game.FixFlagEnumSavingInBeta")]
public partial class FixFlagEnumSavingInBeta : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableFixFlagEnumSavingInBeta;
        }
    }
    [LocalizedString("ToyBox_Features_SettingsTab_Game_FixFlagEnumSavingInBeta_Name", "Fix saving of Flag Enums")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsTab_Game_FixFlagEnumSavingInBeta_Description", "The Beta switched the save system. The new system can't properly handle flag enums which can cause saving to fail. This feature is a hotfix for that.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.SettingsTab.Game.FixFlagEnumSavingInBeta";
        }
    }
    private bool m_IsSaving = false;
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SerializeSceneState)), HarmonyPrefix]
    private static void StartSaving() {
        GetInstance<FixFlagEnumSavingInBeta>().m_IsSaving = true;
    }
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SerializeSceneState)), HarmonyPostfix]
    private static void StopSaving(ref Task __result) {
        __result = __result.ContinueWith(t => {
            GetInstance<FixFlagEnumSavingInBeta>().m_IsSaving = false;
        });
    }
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetName)), HarmonyPostfix]
    private static void Override(ref string __result, Type enumType, object value) {
        if (__result == null && PerformanceEnhancementFeatures.HasAttribute(enumType, typeof(FlagsAttribute))) {
            __result = value.ToString();
        }
    }
}
