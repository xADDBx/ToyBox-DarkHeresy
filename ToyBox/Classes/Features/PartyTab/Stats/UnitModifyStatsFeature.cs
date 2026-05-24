using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using System.Runtime.CompilerServices;
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
    private static readonly StatType[] m_Divider = [StatType.BallisticSkill, StatType.SkillAthletics, StatType.MaxHitPoints];
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
                        var statResult = unit.Actor.GetStat(stat);
                        var baseValue = statResult.BaseValue;
                        var modifiedValue = statResult.ModifiedValue;
                        var change = 0;
                        using (HorizontalScope()) {
                            Space(10);
                            var name = GetText(stat);
                            if (statResult.FullOverrideStat != null) {
                                name += $" ({m_OverridenByLocalizedText}: {GetText(statResult.FullOverrideStat.Value)})";
                            }
                            UI.Label(name, Width(m_LabelWidth));
                            _ = UI.Button("<", () => {
                                change -= 1;
                            });
                            GUILayout.FlexibleSpace();
                            UI.Label($" {modifiedValue} ({baseValue}) ".Bold().Orange(), GUILayout.MinWidth(60 * Main.UIScale));
                            _ = UI.Button(">", () => {
                                change += 1;
                            });
                            GUILayout.FlexibleSpace();
                            Space(10);
                            var val = modifiedValue;
                            _ = UI.TextField(ref val, pair => {
                                change += pair.newContent - statResult.ModifiedValue;
                            }, Width(75 * Main.UIScale));
                        }
                        if (change != 0) {
                            if (InSaveSettings != null) {
                                _ = InSaveSettings.AppliedUnitStatChanges.TryGetValue(unit.UniqueId, out var dict);
                                dict ??= [];
                                if (dict.TryGetValue(stat, out var current)) {
                                    current += change;
                                    if (current == 0) {
                                        _ = dict.Remove(stat);
                                    } else {
                                        dict[stat] = current;
                                    }
                                } else {
                                    dict[stat] = change;
                                }
                                InSaveSettings.AppliedUnitStatChanges[unit.UniqueId] = dict;
                                InSaveSettings.Save();
                                unit.Actor.InvalidateStatBaseValueCache();
                                unit.Actor.NotifyStatChanged(stat);
                            }
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(BaseUnitEntity), nameof(BaseUnitEntity.GetStatBaseValue)), HarmonyPostfix]
    private static void UnitHelper_CreatePreview_Patch(BaseUnitEntity __instance, StatType type, ref StatBaseValue __result) {
        Dictionary<StatType, int>? changes = null;
        _ = InSaveSettings?.AppliedUnitStatChanges.TryGetValue(__instance.UniqueId, out changes);
        if (changes == null) {
            _ = m_PreviewUnitStatChanges.TryGetValue(__instance, out changes);
        }
        if (changes != null) {
            if (changes.TryGetValue(type, out var change)) {
                try {
                    __result = new(__result.Value + change, __result.Enabled, __result.Forced);
                } catch (Exception ex) {
                    Error(ex);
                }
            }
        }
    }
    private static readonly ConditionalWeakTable<BaseUnitEntity, Dictionary<StatType, int>> m_PreviewUnitStatChanges = new();
    [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.CreatePreview)), HarmonyPostfix]
    private static void UnitHelper_CreatePreview_Patch(BaseUnitEntity _this, ref BaseUnitEntity __result) {
        if (InSaveSettings?.AppliedUnitStatChanges.TryGetValue(_this.UniqueId, out var changes) ?? false) {
            m_PreviewUnitStatChanges.Add(__result, changes);

            __result.Actor.InvalidateStatBaseValueCache();
            foreach (var stat in changes.Keys) {
                __result.Actor.NotifyStatChanged(stat);
            }
        }
    }

    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyStatsFeature_m_TryToKeepThisFeatureActivatedAftLocalizedText", "Try to keep this feature activated after using it (Click for Explanation)")]
    private static partial string m_TryToKeepThisFeatureActivatedAftLocalizedText { get; }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyStatsFeature_m_OverridenByLocalizedText", "Overriden by")]
    private static partial string m_OverridenByLocalizedText { get; }
}
