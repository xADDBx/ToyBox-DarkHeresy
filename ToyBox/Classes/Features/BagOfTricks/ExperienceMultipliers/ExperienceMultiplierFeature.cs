using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Gameplay.Features.Experience;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.ExperienceMultipliers;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.ExperienceMultipliers.ExperienceMultiplierFeature")]
public partial class ExperienceMultiplierFeature : FeatureWithPatch {
    private static bool m_IsEnabled = false;
    public override ref bool IsEnabled {
        get {
            m_IsEnabled = Settings.AllExperienceMultiplier != 1f || Settings.UseCombatExperienceMultiplier || Settings.UseQuestExperienceMultiplier || Settings.UseSkillCheckMultiplier || Settings.UseInvestigationMultiplier;
            return ref m_IsEnabled;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_Name", "Experience Multipliers")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_Description", "Provides a general experience multiplier and possible overrides for specific kinds of experience sources.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.ExperienceMultipliers.ExperienceMultiplierFeature";
        }
    }
    private void MaybeReset() {
        if (m_IsEnabled != IsEnabled) {
            Destroy();
            Initialize();
        }
    }
    public override void OnGui() {
        using (HorizontalScope()) {
            UI.Label(m_AllExperienceLocalizedText.Cyan(), Width(250 * Main.UIScale));
            if (UI.LogSlider(ref Settings.AllExperienceMultiplier, 0f, 100f, 1f, 1)) {
                MaybeReset();
            }
        }
        using (HorizontalScope()) {
            if (UI.Toggle(m_OverrideForCombatLocalizedText, null, ref Settings.UseCombatExperienceMultiplier, null, null, 250 * Main.UIScale)) {
                MaybeReset();
            }
            if (Settings.UseCombatExperienceMultiplier) {
                UI.LogSlider(ref Settings.CombatExperienceMultiplier, 0f, 100f, 1f, 1);
            }
        }
        using (HorizontalScope()) {
            if (UI.Toggle(m_OverrideForQuestsLocalizedText, null, ref Settings.UseQuestExperienceMultiplier, null, null, 250 * Main.UIScale)) {
                MaybeReset();
            }
            if (Settings.UseQuestExperienceMultiplier) {
                UI.LogSlider(ref Settings.QuestExperienceMultiplier, 0f, 100f, 1f, 1);
            }
        }
        using (HorizontalScope()) {
            if (UI.Toggle(m_OverrideForSkillChecksLocalizedText, null, ref Settings.UseSkillCheckMultiplier, null, null, 250 * Main.UIScale)) {
                MaybeReset();
            }
            if (Settings.UseSkillCheckMultiplier) {
                UI.LogSlider(ref Settings.SkillCheckMultiplier, 0f, 100f, 1f, 1);
            }
        }
        using (HorizontalScope()) {
            if (UI.Toggle(m_OverrideForInvestigationsLocalizedText, null, ref Settings.UseInvestigationMultiplier, null, null, 250 * Main.UIScale)) {
                MaybeReset();
            }
            if (Settings.UseInvestigationMultiplier) {
                UI.LogSlider(ref Settings.InvestigationMultiplier, 0f, 100f, 1f, 1);
            }
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_m_AllExperienceLocalizedText", "All Experience")]
    private static partial string m_AllExperienceLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_m_OverrideForCombatLocalizedText", "Override for Combat")]
    private static partial string m_OverrideForCombatLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_m_OverrideForQuestsLocalizedText", "Override for Quests")]
    private static partial string m_OverrideForQuestsLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_m_OverrideForSkillChecksLocalizedText", "Override for Skill Checks")]
    private static partial string m_OverrideForSkillChecksLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_ExperienceMultipliers_ExperienceMultiplierFeature_m_OverrideForInvestigationsLocalizedText", "Override for Investigations")]
    private static partial string m_OverrideForInvestigationsLocalizedText { get; }
    #region Patches
    private static ExperienceType? m_Context;
    [HarmonyPatch(typeof(Experience), nameof(Experience.Gain), [typeof(int), typeof(MechanicEntity), typeof(bool)]), HarmonyPrefix]
    private static void Experience_Gain_Patch1(ref int experience) {
        if (m_Context.HasValue) {
            Experience_Calculate_Patch(m_Context.Value, ref experience);
        }
    }
    [HarmonyPatch(typeof(Experience), nameof(Experience.Gain), [typeof(IExperienceSettings), typeof(BaseUnitEntity)]), HarmonyPrefix]
    private static void Experience_Gain_Patch2(IExperienceSettings experience) {
        if (experience.OverrideValue != null) {
            m_Context = experience.Type;
        }
    }
    [HarmonyPatch(typeof(Experience), nameof(Experience.GainForEncounter)), HarmonyPrefix]
    private static void Experience_GainForEncounter_Patch(Kingmaker.Gameplay.Features.Encounter.BlueprintEncounter blueprint) {
        if (blueprint.OverrideExperience != null) {
            m_Context = ExperienceType.Encounter;
        }
    }
    [HarmonyPatch(typeof(Experience), nameof(Experience.TryGain), [typeof(BlueprintQuest), typeof(MechanicEntity)]), HarmonyPrefix]
    private static void Experience_GainForEncounter_Patch() {
        m_Context = ExperienceType.Quest;
    }
    [HarmonyPatch(typeof(Experience), nameof(Experience.Calculate), [typeof(ExperienceType), typeof(int?), typeof(int?), typeof(UnitDifficultyType?)]), HarmonyPostfix]
    private static void Experience_Calculate_Patch(ExperienceType type, ref int __result) {
        var mult = Settings.AllExperienceMultiplier;
        switch (type) {
            case ExperienceType.Quest:
            case ExperienceType.MainQuest: {
                    if (Settings.UseQuestExperienceMultiplier) {
                        mult = Settings.QuestExperienceMultiplier;
                    }
                }
                break;
            case ExperienceType.Encounter: {
                    if (Settings.UseCombatExperienceMultiplier) {
                        mult = Settings.CombatExperienceMultiplier;
                    }
                }
                break;
            case ExperienceType.Investigation: {
                    if (Settings.UseInvestigationMultiplier) {
                        mult = Settings.InvestigationMultiplier;
                    }
                }
                break;
            case ExperienceType.SkillCheck: {
                    if (Settings.UseSkillCheckMultiplier) {
                        mult = Settings.SkillCheckMultiplier;
                    }
                }
                break;
        }
        if (mult != 1) {
            __result = Mathf.RoundToInt(__result * mult);
        }
    }
    #endregion
}
