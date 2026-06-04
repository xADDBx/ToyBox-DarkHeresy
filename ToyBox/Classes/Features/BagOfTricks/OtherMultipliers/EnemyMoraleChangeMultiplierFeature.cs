using Kingmaker.RuleSystem.Rules;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.OtherMultipliers;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.OtherMultipliers.EnemyMoraleChangeMultiplierFeature")]
public partial class EnemyMoraleChangeMultiplierFeature : FeatureWithPatch {
    [LocalizedString("ToyBox_Features_BagOfTricks_OtherMultipliers_EnemyMoraleChangeMultiplierFeature_Name", "Enemy Morale Multiplier")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_OtherMultipliers_EnemyMoraleChangeMultiplierFeature_Description", "Multiplies any changes to enemy morale.")]
    public override partial string Description { get; }

    private bool m_IsEnabled = false;
    public override ref bool IsEnabled {
        get {
            m_IsEnabled = Settings.EnemyMoraleChangeMultiplier != null;
            return ref m_IsEnabled;
        }
    }
    public override void OnGui() {
        var tmp = Settings.EnemyMoraleChangeMultiplier ?? 1f;
        using (HorizontalScope()) {
            if (UI.Slider(ref tmp, 0f, 20f, 1f, 2, null, null, AutoWidth(), GUILayout.MinWidth(50), GUILayout.MinWidth(150))) {
                if (tmp == 1f) {
                    Settings.EnemyMoraleChangeMultiplier = null;
                    Disable();
                } else {
                    Settings.EnemyMoraleChangeMultiplier = tmp;
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
            return "ToyBox.Features.BagOfTricks.OtherMultipliers.EnemyMoraleChangeMultiplierFeature";
        }
    }
    [HarmonyPatch(typeof(RuleCalculateMoraleChange), nameof(RuleCalculateMoraleChange.OnTrigger)), HarmonyPostfix]
    private static void RuleCalculateMoraleChange_OnTrigger_Patch(RuleCalculateMoraleChange __instance) {
        try {
            if (ToyBoxUnitHelper.IsOfSelectedType(__instance.TargetUnit, UnitSelectType.Enemies) && Settings.EnemyMoraleChangeMultiplier.HasValue) {
                __instance.ValueModifier.Add(Kingmaker.RuleSystem.Rules.Modifiers.ModifierType.PctMul_Extra, (int)(100 * Settings.EnemyMoraleChangeMultiplier.Value), __instance, Kingmaker.Enums.ModifierDescriptor.Cheat);
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
}
