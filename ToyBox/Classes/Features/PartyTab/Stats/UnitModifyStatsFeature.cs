using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Features.PartyTab.Stats;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.PartyTab.Stats.UnitModifyStatsFeature")]
public partial class UnitModifyStatsFeature : FeatureWithPatch, INeedContextFeature<BaseUnitEntity> {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableUnitModifyStats;
        }
    }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyStatsFeature_Name", "Modify Unit Stats")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyStatsFeature_Description", "Allows modifying the various stats of a unit.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.PartyTab.Stats.UnitModifyStatsFeature";
        }
    }
    public bool GetContext(out BaseUnitEntity? context) {
        return ContextProvider.BaseUnitEntity(out context);
    }

    public override void OnGui() {
        if (GetContext(out var unit)) {
            using (HorizontalScope()) {
                OnGui(unit!);
            }
        }
    }
    private readonly TimedCache<float> m_LabelWidth = new(() => {
        List<string> names = [];
        foreach (StatType stat in Enum.GetValues(typeof(StatType))) {
            if (Constants.WeirdStats.Contains(stat)) {
                continue;
            }
            var name = LocalizedTexts.Instance.Stats.GetText(stat);
            if (string.IsNullOrWhiteSpace(name)) {
                name = stat.ToString();
            }
            names.Add(name + " ");
        }
        return CalculateLargestLabelWidth(names, GUI.skin.label);
    });
    private static string GetText(StatType stat) {
        var name = LocalizedTexts.Instance.Stats.GetText(stat);
        if (string.IsNullOrWhiteSpace(name)) {
            name = stat.ToString();
        }
        return name;
    }
    private static readonly StatType[] m_Divider = [StatType.SkillAthletics, StatType.HitPoints];
    public void OnGui(BaseUnitEntity unit) {
        base.OnGui();
        if (IsEnabled) {
            using (HorizontalScope()) {
                Space(25);
                using (VerticalScope()) {
                    UI.Label("When this is turned off, the changed stats will be \"forgotten\" after reloading the save because base stats are recalculated on save load.".Orange(), Width(0.5f * EffectiveWindowWidth()));
                    foreach (StatType stat in Enum.GetValues(typeof(StatType))) {
                        if (Constants.WeirdStats.Contains(stat)) {
                            continue;
                        }
                        if (m_Divider.Contains(stat)) {
                            Div.DrawDiv();
                        }
                        var modifiableValue = unit.Stats.GetStatOptional(stat);
                        var baseValue = 0;
                        var modifiedValue = 0;
                        if (modifiableValue != null) {
                            baseValue = modifiableValue.BaseValue;
                            modifiedValue = modifiableValue.ModifiedValue;
                        } else {
                            // Note: We *could* support this by just not skipping the iteration here.
                            // And adding a nullable check to modifiableValue.m_Override != null
                            continue;
                        }
                        var change = 0;
                        using (HorizontalScope()) {
                            Space(10);
                            var name = GetText(stat);
                            if (modifiableValue.m_Override != null) {
                                name += $" ({m_OverridenByLocalizedText}: {GetText(modifiableValue.m_Override.m_Type)})";
                            }
                            UI.Label(name, Width(m_LabelWidth));
                            _ = UI.Button("<", () => {
                                if (modifiableValue == null) {
                                    modifiableValue = AddStat(stat, unit);
                                    baseValue = modifiableValue.BaseValue;
                                    modifiedValue = modifiableValue.ModifiedValue;
                                }
                                change -= 1;
                            });
                            UI.Label($" {modifiedValue} ".Bold().Orange(), Width(50 * Main.UIScale));
                            _ = UI.Button(">", () => {
                                if (modifiableValue == null) {
                                    modifiableValue = AddStat(stat, unit);
                                    baseValue = modifiableValue.BaseValue;
                                    modifiedValue = modifiableValue.ModifiedValue;
                                }
                                change += 1;
                            });
                            Space(10);
                            var val = modifiedValue;
                            UI.TextField(ref val, pair => {
                                if (modifiableValue == null) {
                                    modifiableValue = AddStat(stat, unit);
                                    baseValue = modifiableValue.BaseValue;
                                    modifiedValue = modifiableValue.ModifiedValue;
                                }
                                change += pair.newContent - modifiableValue.ModifiedValue;
                            }, Width(75 * Main.UIScale));
                        }
                        if (change != 0) {
                            if (InSaveSettings != null) {
                                InSaveSettings.AppliedUnitStatChanges.TryGetValue(unit.UniqueId, out var dict);
                                dict ??= [];
                                if (dict.TryGetValue(stat, out var current)) {
                                    current += change;
                                    if (current == 0) {
                                        dict.Remove(stat);
                                    } else {
                                        dict[stat] = current;
                                    }
                                } else {
                                    dict[stat] = change;
                                }
                                modifiableValue.m_BaseValue = baseValue + change;
                                modifiableValue.UpdateValue();
                                InSaveSettings.AppliedUnitStatChanges[unit.UniqueId] = dict;
                                InSaveSettings.Save();
                            }
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(BaseUnitEntity), nameof(BaseUnitEntity.GetStatBaseValue)), HarmonyPostfix]
    private static void UnitHelper_CreatePreview_Patch(BaseUnitEntity __instance, StatType type, ref StatBaseValue __result) {
        if (InSaveSettings?.AppliedUnitStatChanges.TryGetValue(__instance.UniqueId, out var changes) ?? false) {
            if (changes.TryGetValue(type, out var change)) {
                try {
                    __result = new(__result.Value + change, __result.Enabled, __result.Forced);
                } catch (Exception ex) {
                    Error(ex);
                }
            }
        }
    }
    [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.CreatePreview)), HarmonyPostfix]
    private static void UnitHelper_CreatePreview_Patch(BaseUnitEntity _this, ref BaseUnitEntity __result) {
        if (InSaveSettings?.AppliedUnitStatChanges.TryGetValue(_this.UniqueId, out var changes) ?? false) {
            foreach (var change in changes) {
                try {
                    var mV = __result.Stats.GetStatOptional(change.Key) ?? AddStat(change.Key, __result);
                    mV.m_BaseValue += change.Value;
                    mV.UpdateValue();
                } catch (Exception ex) {
                    Error(ex);
                }
            }
        }
    }
    private static ModifiableValue AddStat(StatType stat, BaseUnitEntity unit) {
        ModifiableValue? ret;
        if (StatTypeHelper.IsSkill(stat)) {
            ret = unit.Stats.Container.RegisterSkill(stat);
        } else if (StatTypeHelper.IsAttribute(stat)) {
            ret = unit.Stats.Container.RegisterAttribute(stat);
        } else {
            ret = unit.Stats.Container.Register(stat);
        }
        return ret;
    }

    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyStatsFeature_m_TryToKeepThisFeatureActivatedAftLocalizedText", "Try to keep this feature activated after using it (Click for Explanation)")]
    private static partial string m_TryToKeepThisFeatureActivatedAftLocalizedText { get; }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyStatsFeature_m_OverridenByLocalizedText", "Overriden by")]
    private static partial string m_OverridenByLocalizedText { get; }
}
