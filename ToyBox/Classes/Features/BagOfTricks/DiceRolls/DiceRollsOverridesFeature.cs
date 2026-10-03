using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.View.Covers;
using System.Reflection.Emit;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.DiceRolls;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.DiceRolls.DiceRollsOverridesFeature")]
public partial class DiceRollsOverridesFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableDiceRollsOverrides;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_Name", "Dice Roll Overrides")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_Description", "Allows changing the results of various dice rolls across the game.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.DiceRolls.DiceRollsOverridesFeature";
        }
    }
    private readonly TimedCache<float> m_LabelWidth = new(() => CalculateLargestLabelWidth([
        m_AllAttacksHitLocalizedText, m_RollWithAdvantageLocalizedText, m_RollWithDisadvantageLocalizedText,
        m_Initiative_AlwaysRoll1LocalizedText, m_Initiative_AlwaysRoll50LocalizedText, m_Initiative_AlwaysRoll100LocalizedText, m_NeverRoll100LocalizedText,
        m_NeverRoll1LocalizedText, m_DamageRolls_Take1LocalizedText, m_DamageRolls_Take25LocalizedText, m_DamageRolls_Take50LocalizedText,
        m_OutOfCombat_Take1LocalizedText, m_OutOfCombat_Take25LocalizedText, m_OutOfCombat_Take50LocalizedText, m_SkillChecks_Take1LocalizedText,
        m_SkillChecks_Take25LocalizedText, m_SkillChecks_Take50LocalizedText, m_Take100LocalizedText, m_Take1LocalizedText,
        m_Take50LocalizedText, m_CoverNeverInterceptsLocalizedText, m_CoverAlwaysInterceptsLocalizedText], GUI.skin.label));
    public override void OnGui() {
        base.OnGui();
        if (IsEnabled) {
            using (HorizontalScope()) {
                Space(25);
                using (VerticalScope()) {
                    var labelWidth = m_LabelWidth.Value + 50 * Main.UIScale;
                    using (HorizontalScope()) {
                        UI.Label(m_AllAttacksHitLocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsAllAttacksHit, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    UI.Label(m_AdvantageMeansDoingTwoRollsAndTaLocalizedText.Green());
                    using (HorizontalScope()) {
                        UI.Label(m_RollWithAdvantageLocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsRollWithAdvantage, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_RollWithDisadvantageLocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsRollWithDisadvantage, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_Take100LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsAlwaysRoll100, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_Take50LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsAlwaysRoll50, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_Take1LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsAlwaysRoll1, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_NeverRoll100LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsNeverRoll100, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_NeverRoll1LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsNeverRoll1, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    UI.Label(m_CoverInterception_UsesASeparateRoLocalizedText.Green());
                    using (HorizontalScope()) {
                        UI.Label(m_CoverNeverInterceptsLocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsCoverNeverIntercept, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_CoverAlwaysInterceptsLocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsCoverAlwaysIntercept, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_OutOfCombat_Take50LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsOutOfCombatTake50, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_OutOfCombat_Take25LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsOutOfCombatTake25, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_OutOfCombat_Take1LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsOutOfCombatTake1, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    UI.Label(m_Initiative__HigherIsBetterLocalizedText.Green());
                    using (HorizontalScope()) {
                        UI.Label(m_Initiative_AlwaysRoll1LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsInitiativeAlwaysRoll1, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_Initiative_AlwaysRoll50LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsInitiativeAlwaysRoll50, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_Initiative_AlwaysRoll100LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsInitiativeAlwaysRoll100, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    UI.Label(m_SkillChecks__LowerIsBetterLocalizedText.Green());
                    using (HorizontalScope()) {
                        UI.Label(m_SkillChecks_Take50LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsSkillChecksTake50, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_SkillChecks_Take25LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsSkillChecksTake25, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_SkillChecks_Take1LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsSkillChecksTake1, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    UI.Label(m_Damage__HigherIsBetterLocalizedText.Green());
                    using (HorizontalScope()) {
                        UI.Label(m_DamageRolls_Take50LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsDamageTake50, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_DamageRolls_Take25LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsDamageTake25, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_DamageRolls_Take1LocalizedText, Width(labelWidth));
                        UI.SelectionGrid(ref Settings.DiceRollsDamageTake1, 8, e => e.GetLocalized(), Width(0.7f * EffectiveWindowWidth()));
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(RulePerformAttackRoll), nameof(RulePerformAttackRoll.OnTrigger)), HarmonyPostfix]
    private static void RulePerformAttackRoll_OnTrigger_Patch(RulePerformAttackRoll __instance) {
        if (ToyBoxUnitHelper.IsOfSelectedType(__instance.InitiatorUnit, Settings.DiceRollsAllAttacksHit) && __instance.Result != AttackResult.Hit && __instance.Result != AttackResult.CoverHit) {
            __instance.Result = AttackResult.Hit;
        }
    }
    [HarmonyPatch(typeof(RulePerformAttackRoll), nameof(RulePerformAttackRoll.OnTrigger)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> RulePerformAttackRoll_Cover_Patch(IEnumerable<CodeInstruction> instructions) {
        var getForceCoverHit = AccessTools.PropertyGetter(typeof(RuleCalculateHitChances), nameof(RuleCalculateHitChances.ResultForceCoverHit));
        var awaitingStore = false;
        var replaced = 0;
        foreach (var instruction in instructions) {
            if (instruction.Calls(getForceCoverHit)) {
                awaitingStore = true;
            }
            if (awaitingStore && instruction.IsStloc()) {
                yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                yield return CodeInstruction.Call((bool isCoverHit, RulePerformAttackRoll rule) => OverrideCoverHit(isCoverHit, rule));
                awaitingStore = false;
                replaced++;
            }
            yield return instruction;
        }
        ThrowIfTrue(replaced != 1);
    }
    private static bool OverrideCoverHit(bool isCoverHit, RulePerformAttackRoll rule) {
        if (rule.HitChanceRule.ResultLos != LosCalculations.CoverType.Cover) {
            return isCoverHit;
        }
        if (ToyBoxUnitHelper.IsOfSelectedType(rule.InitiatorUnit, Settings.DiceRollsCoverNeverIntercept)) {
            return false;
        }
        return ToyBoxUnitHelper.IsOfSelectedType(rule.InitiatorUnit, Settings.DiceRollsCoverAlwaysIntercept) || isCoverHit;
    }
    private static void ReRoll(RuleRollDice dice, bool? advantage = null) {
        m_IsRecursiveInitiativeRoll = true;
        var tmp = dice._result;
        dice.Roll();
        if (advantage.HasValue) {
            if (advantage.Value) {
                dice._result = Math.Max(dice._result, tmp);
            } else {
                dice._result = Math.Min(dice._result, tmp);
            }
        }
        m_IsRecursiveInitiativeRoll = false;
    }
    [ThreadStatic]
    private static bool m_IsRecursiveInitiativeRoll;
    [HarmonyPatch(typeof(RuleRollInitiative), nameof(RuleRollInitiative.ResultD100), MethodType.Getter), HarmonyPostfix]
    private static void RuleRollInitiative_getResultD10_Patch(ref RuleRollD100 __result, RuleRollInitiative __instance) {
        if (ToyBoxUnitHelper.IsOfSelectedType(__instance.InitiatorUnit, Settings.DiceRollsInitiativeAlwaysRoll1)) {
            __result._result = 1;
        } else if (ToyBoxUnitHelper.IsOfSelectedType(__instance.InitiatorUnit, Settings.DiceRollsInitiativeAlwaysRoll50)) {
            __result._result = 50;
        } else if (ToyBoxUnitHelper.IsOfSelectedType(__instance.InitiatorUnit, Settings.DiceRollsInitiativeAlwaysRoll100)) {
            __result._result = 100;
        } else if (ToyBoxUnitHelper.IsOfSelectedType(__instance.InitiatorUnit, Settings.DiceRollsRollWithAdvantage)) {
            ReRoll(__result, true);
        } else if (ToyBoxUnitHelper.IsOfSelectedType(__instance.InitiatorUnit, Settings.DiceRollsRollWithDisadvantage)) {
            ReRoll(__result, false);
        }
    }
    [HarmonyPatch(typeof(RuleRollDice), nameof(RuleRollDice.Roll)), HarmonyPostfix]
    private static void RuleRollDice_Roll_Patch(RuleRollDice __instance) {
        if (m_IsRecursiveInitiativeRoll || __instance.DiceFormula.Dice != DiceType.D100 || __instance.DiceFormula.Rolls > 1) {
            return;
        }
        var isDamageRule = false;
        foreach (var evt in Rulebook.CurrentContext?.m_EventStack ?? []) {
            if (evt is RulePerformPartySkillCheck partySkillCheck) {
                // Initiator of party skill check is always main character unit.
                if (ToyBoxUnitHelper.IsOfSelectedType(partySkillCheck.InitiatorUnit, Settings.DiceRollsSkillChecksTake1)) {
                    __instance._result = 1;
                    return;
                } else if (ToyBoxUnitHelper.IsOfSelectedType(partySkillCheck.InitiatorUnit, Settings.DiceRollsSkillChecksTake25)) {
                    __instance._result = 25;
                    return;
                } else if (ToyBoxUnitHelper.IsOfSelectedType(partySkillCheck.InitiatorUnit, Settings.DiceRollsSkillChecksTake50)) {
                    __instance._result = 50;
                    return;
                }
            } else if (evt is RulePerformSkillCheck skillCheck) {
                if (ToyBoxUnitHelper.IsOfSelectedType(skillCheck.InitiatorUnit, Settings.DiceRollsSkillChecksTake1)) {
                    __instance._result = 1;
                } else if (ToyBoxUnitHelper.IsOfSelectedType(skillCheck.InitiatorUnit, Settings.DiceRollsSkillChecksTake25)) {
                    __instance._result = 25;
                } else if (ToyBoxUnitHelper.IsOfSelectedType(skillCheck.InitiatorUnit, Settings.DiceRollsSkillChecksTake50)) {
                    __instance._result = 50;
                } else if (ToyBoxUnitHelper.IsOfSelectedType(skillCheck.InitiatorUnit, Settings.DiceRollsRollWithAdvantage)) {
                    ReRoll(__instance, false);
                } else if (ToyBoxUnitHelper.IsOfSelectedType(skillCheck.InitiatorUnit, Settings.DiceRollsRollWithDisadvantage)) {
                    ReRoll(__instance, true);
                } else {
                    isDamageRule = false;
                    break;
                }
                return;
            } else if (evt is RuleDealDamage) {
                isDamageRule = true;
            } else if (evt is RulePerformBodyPartHitRoll) {
                return;
            } else if (evt is RuleRollInitiative) {
                return;
            }
        }
        var initiator = __instance.InitiatorUnit;
        if (initiator == null) {
            return;
        }
        if (!initiator.IsInCombat) {
            if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsOutOfCombatTake1)) {
                __instance._result = 1;
                return;
            } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsOutOfCombatTake25)) {
                __instance._result = 25;
                return;
            } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsOutOfCombatTake50)) {
                __instance._result = 50;
                return;
            }
        } else if (isDamageRule) {
            if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsDamageTake1)) {
                __instance._result = 1;
                return;
            } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsDamageTake25)) {
                __instance._result = 25;
                return;
            } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsDamageTake50)) {
                __instance._result = 50;
                return;
            }
        }
        if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsAlwaysRoll1)) {
            __instance._result = 1;
            return;
        } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsAlwaysRoll50)) {
            __instance._result = 50;
            return;
        } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsAlwaysRoll100)) {
            __instance._result = 100;
            return;
        }

        var minInclusive = 1;
        var maxExclusive = 101;
        if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsNeverRoll1)) {
            minInclusive = 2;
        }
        if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsNeverRoll100)) {
            maxExclusive = 100;
        }
        var currentAttempt = 0;
        do {
            currentAttempt++;
            if (__instance._result < minInclusive || __instance._result >= maxExclusive) {
                ReRoll(__instance);
            }
            if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsRollWithAdvantage)) {
                ReRoll(__instance, isDamageRule);
            } else if (ToyBoxUnitHelper.IsOfSelectedType(initiator, Settings.DiceRollsRollWithDisadvantage)) {
                ReRoll(__instance, !isDamageRule);
            }
        } while (currentAttempt < MaxAttempts && (__instance._result < minInclusive || __instance._result >= maxExclusive));
    }
    public const int MaxAttempts = 5;

    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_AdvantageMeansDoingTwoRollsAndTaLocalizedText", "Advantage means doing two rolls and taking the better result; which for Initiative and Damage rolls means the higher number and for Skill Checks the lower number.")]
    private static partial string m_AdvantageMeansDoingTwoRollsAndTaLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_AllAttacksHitLocalizedText", "All Attacks Hit")]
    private static partial string m_AllAttacksHitLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_RollWithAdvantageLocalizedText", "Roll with Advantage")]
    private static partial string m_RollWithAdvantageLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_RollWithDisadvantageLocalizedText", "Roll with Disadvantage")]
    private static partial string m_RollWithDisadvantageLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Take100LocalizedText", "Take 100")]
    private static partial string m_Take100LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Take50LocalizedText", "Take 50")]
    private static partial string m_Take50LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Take1LocalizedText", "Take 1")]
    private static partial string m_Take1LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_NeverRoll100LocalizedText", "Never Roll 100")]
    private static partial string m_NeverRoll100LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_NeverRoll1LocalizedText", "Never Roll 1")]
    private static partial string m_NeverRoll1LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_OutOfCombat_Take50LocalizedText", "Out of Combat: Take 50")]
    private static partial string m_OutOfCombat_Take50LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_OutOfCombat_Take25LocalizedText", "Out of Combat: Take 25")]
    private static partial string m_OutOfCombat_Take25LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_OutOfCombat_Take1LocalizedText", "Out of Combat: Take 1")]
    private static partial string m_OutOfCombat_Take1LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Initiative__HigherIsBetterLocalizedText", "Initiative -> Higher is better")]
    private static partial string m_Initiative__HigherIsBetterLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Initiative_AlwaysRoll1LocalizedText", "Initiative: Always Roll 1")]
    private static partial string m_Initiative_AlwaysRoll1LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Initiative_AlwaysRoll50LocalizedText", "Initiative: Always Roll 50")]
    private static partial string m_Initiative_AlwaysRoll50LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Initiative_AlwaysRoll100LocalizedText", "Initiative: Always Roll 100")]
    private static partial string m_Initiative_AlwaysRoll100LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_SkillChecks__LowerIsBetterLocalizedText", "Skill Checks -> Lower is better")]
    private static partial string m_SkillChecks__LowerIsBetterLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_SkillChecks_Take50LocalizedText", "Skill Checks: Take 50")]
    private static partial string m_SkillChecks_Take50LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_SkillChecks_Take25LocalizedText", "Skill Checks: Take 25")]
    private static partial string m_SkillChecks_Take25LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_SkillChecks_Take1LocalizedText", "Skill Checks: Take 1")]
    private static partial string m_SkillChecks_Take1LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_Damage__HigherIsBetterLocalizedText", "Damage -> Higher is better")]
    private static partial string m_Damage__HigherIsBetterLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_DamageRolls_Take50LocalizedText", "Damage Rolls: Take 50")]
    private static partial string m_DamageRolls_Take50LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_DamageRolls_Take25LocalizedText", "Damage Rolls: Take 25")]
    private static partial string m_DamageRolls_Take25LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_DamageRolls_Take1LocalizedText", "Damage Rolls: Take 1")]
    private static partial string m_DamageRolls_Take1LocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_CoverInterception_UsesASeparateRoLocalizedText", "Controls whether attacks made by the selected units are redirected to cover instead of their target.")]
    private static partial string m_CoverInterception_UsesASeparateRoLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_CoverNeverInterceptsLocalizedText", "Cover: Never Redirect")]
    private static partial string m_CoverNeverInterceptsLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_DiceRolls_DiceRollsOverridesFeature_m_CoverAlwaysInterceptsLocalizedText", "Cover: Always Redirect")]
    private static partial string m_CoverAlwaysInterceptsLocalizedText { get; }
}
