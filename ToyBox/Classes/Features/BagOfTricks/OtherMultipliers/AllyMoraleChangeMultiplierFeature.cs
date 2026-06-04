using Kingmaker.RuleSystem.Rules;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.OtherMultipliers;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.OtherMultipliers.AllyMoraleChangeMultiplierFeature")]
public partial class AllyMoraleChangeMultiplierFeature : FeatureWithPatch {
    [LocalizedString("ToyBox_Features_BagOfTricks_OtherMultipliers_AllyMoraleChangeMultiplierFeature_Name", "Ally Morale Multiplier")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_OtherMultipliers_AllyMoraleChangeMultiplierFeature_Description", "Multiplies any changes to ally morale.")]
    public override partial string Description { get; }

    private bool m_IsEnabled = false;
    public override ref bool IsEnabled {
        get {
            m_IsEnabled = Settings.AllyMoraleChangeMultiplier != null;
            return ref m_IsEnabled;
        }
    }
    public override void OnGui() {
        var tmp = Settings.AllyMoraleChangeMultiplier ?? 1f;
        using (HorizontalScope()) {
            if (UI.Slider(ref tmp, 0f, 20f, 1f, 2, null, null, AutoWidth(), GUILayout.MinWidth(50), GUILayout.MinWidth(150))) {
                if (tmp == 1f) {
                    Settings.AllyMoraleChangeMultiplier = null;
                    Disable();
                } else {
                    Settings.AllyMoraleChangeMultiplier = tmp;
                    Enable();
                }
            }
            Space(10);
            UI.Label(Name);
            Space(10);
            UI.Label(Description.Green());
        }
    }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.OtherMultipliers.AllyMoraleChangeMultiplierFeature";
        }
    }
    [HarmonyPatch(typeof(RuleCalculateMoraleChange), nameof(RuleCalculateMoraleChange.OnTrigger)), HarmonyPrefix]
    private static void RuleCalculateMoraleChange_OnTrigger_Patch(RuleCalculateMoraleChange __instance) {
        try {
            if (ToyBoxUnitHelper.IsOfSelectedType(__instance.TargetUnit, UnitSelectType.Party) && Settings.AllyMoraleChangeMultiplier.HasValue) {
                __instance.ValueModifier.Add(Kingmaker.RuleSystem.Rules.Modifiers.ModifierType.PctMul_Extra, (int)(100 * Settings.AllyMoraleChangeMultiplier.Value), __instance, Kingmaker.Enums.ModifierDescriptor.Cheat);
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
}
