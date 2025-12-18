using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using ToyBox.Infrastructure.Inspector;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.RTSpecific;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.RTSpecific.CustomizePsychicPhenomenaFeature")]
// Early Init to prevent threading from causing issues with mods adding new phenomenas
public partial class CustomizePsychicPhenomenaFeature : FeatureWithPatch, INeedEarlyInitFeature {
    private IEnumerable<BlueprintPsykerRoot.PhenomenaData>? m_BackupPsychicPhenomena;
    private IEnumerable<BlueprintPsykerRoot.PhenomenaData>? m_BackupPerilsOfTheWarp;
    private static string GetPsychicKey(BlueprintPsykerRoot.PhenomenaData p) {
        return p.Ability.guid ?? p.Bark?.Entries?[0]?.Text.GetName() ?? p.OptionalMinorFX?.AssetId ?? "<Null>";
    }
    private static string GetPsychicDisplayText(BlueprintPsykerRoot.PhenomenaData p) {
        if (p.Ability.GetBlueprint() is SimpleBlueprint bp) {
            return BPHelper.GetTitle(bp);
        } else {
            return p.Bark?.Entries?[0]?.Text.GetName() ?? p.OptionalMinorFX?.AssetId ?? "<Null>";
        }
    }
    private readonly Browser<BlueprintPsykerRoot.PhenomenaData> m_PsychicPhenomenaBrowser = new(GetPsychicKey, GetPsychicKey, overridePageWidth: (int)(0.8f * EffectiveWindowWidth()));
    private readonly Browser<BlueprintPsykerRoot.PhenomenaData> m_PerilsOfTheWarpBrowser = new(GetPsychicKey, GetPsychicKey, overridePageWidth: (int)(0.8f * EffectiveWindowWidth()));
    private readonly TimedCache<float> m_ButtonWidth = new(() => CalculateLargestLabelWidth([m_StopExcludingLocalizedText, m_ExcludeLocalizedText], GUI.skin.button));
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableCustomizePsychicPhenomena;
        }
    }
    public override void Destroy() {
        base.Destroy();
        RestorePhenomena();
    }
    private void BackupPhenomena() {
        var root = ConfigRoot.Instance.PsykerRoot;
        m_BackupPsychicPhenomena = root.PsychicPhenomena.Select(p => p);
        m_PsychicPhenomenaBrowser.UpdateItems(m_BackupPsychicPhenomena);
        m_BackupPerilsOfTheWarp = root.PerilsOfTheWarp;
        m_PerilsOfTheWarpBrowser.UpdateItems(m_BackupPerilsOfTheWarp);
    }
    private void RestorePhenomena() {
        if (m_BackupPsychicPhenomena != null) {
            var root = ConfigRoot.Instance.PsykerRoot;
            root.PsychicPhenomena = [.. root.PsychicPhenomena.Union(m_BackupPsychicPhenomena)];
            root.PerilsOfTheWarp = [.. root.PerilsOfTheWarp.Union(m_BackupPerilsOfTheWarp)];
            m_BackupPsychicPhenomena = root.PsychicPhenomena;
            m_BackupPerilsOfTheWarp = root.PerilsOfTheWarp;
        }
    }
    private static T[] RemoveAll<T>(T[] collection, Func<T, bool> pred) {
        return [.. collection.Where(x => !pred(x))];
    }
    private void RemovePhenomena() {
        if (m_BackupPsychicPhenomena == null) {
            BackupPhenomena();
        }
        var root = ConfigRoot.Instance.PsykerRoot;
        root.PsychicPhenomena = RemoveAll(root.PsychicPhenomena, phenomena => Settings.ExcludedRandomPhenomena.Contains(GetPsychicKey(phenomena)));
        root.PerilsOfTheWarp = RemoveAll(root.PerilsOfTheWarp, peril => Settings.ExcludedPerils.Contains(GetPsychicKey(peril)));
    }
    private void PhenomenaGui(BlueprintPsykerRoot.PhenomenaData item, ref HashSet<string> collection) {
        using (HorizontalScope()) {
            var key = GetPsychicKey(item);
            var isExcluded = collection.Contains(key);
            var name = GetPsychicDisplayText(item);
            InspectorUI.InspectToggle(item);
            if (isExcluded) {
                if (UI.Button(m_StopExcludingLocalizedText.Cyan(), null, null, Width(m_ButtonWidth))) {
                    collection.Remove(key);
                    RestorePhenomena();
                    RemovePhenomena();
                }
                Space(10);
                UI.Label(name.Cyan());
            } else {
                if (UI.Button(m_ExcludeLocalizedText, null, null, Width(m_ButtonWidth))) {
                    collection.Add(key);
                    RemovePhenomena();
                }
                Space(10);
                UI.Label(name);
            }
        }
        InspectorUI.InspectIfExpanded(item);
    }
    private bool m_IsCustomizing = false;
    public override void OnGui() {
        _ = UI.Toggle(Name, Description, ref IsEnabled, Initialize, Destroy);
        if (Settings.EnableCustomizePsychicPhenomena) {
            using (HorizontalScope()) {
                Space(40);
                using (VerticalScope()) {
                    UI.Toggle(m_CustomizeLocalizedText, null, ref m_IsCustomizing);
                    if (m_IsCustomizing) {
                        if (UI.Button(m_RefreshAvailablePhenomenas_PerilLocalizedText) || m_BackupPsychicPhenomena == null) {
                            RestorePhenomena();
                            BackupPhenomena();
                            RemovePhenomena();
                        }

                        UI.Label(m_PsychicPhenomenaLocalizedText);
                        m_PsychicPhenomenaBrowser.OnGUI(item => {
                            PhenomenaGui(item, ref Settings.ExcludedRandomPhenomena);
                        });
                        UI.Label(m_PerilsLocalizedText);
                        m_PerilsOfTheWarpBrowser.OnGUI(item => {
                            PhenomenaGui(item, ref Settings.ExcludedPerils);
                        });
                    }
                }
            }
        }
    }

    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_m_CustomizeLocalizedText", "Open Customize UI")]
    private static partial string m_CustomizeLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_m_PsychicPhenomenaLocalizedText", "Psychic Phenomena")]
    private static partial string m_PsychicPhenomenaLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_m_PerilsLocalizedText", "Perils")]
    private static partial string m_PerilsLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_Name", "Customize Psychic Phenomena / Perils of the Warp")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_Description", "Allows disabling specific psychic phenomenas or perils of the warp.")]
    public override partial string Description { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_m_RefreshAvailablePhenomenas_PerilLocalizedText", "Refresh Available Phenomenas/Perils (for mod compat)")]
    private static partial string m_RefreshAvailablePhenomenas_PerilLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_m_StopExcludingLocalizedText", "Stop Excluding")]
    private static partial string m_StopExcludingLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_CustomizePsychicPhenomenaFeature_m_ExcludeLocalizedText", "Exclude")]
    private static partial string m_ExcludeLocalizedText { get; }
    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
    private static void InitializePatch() {
        var feature = GetInstance<CustomizePsychicPhenomenaFeature>();
        if (feature.IsEnabled) {
            feature.BackupPhenomena();
            feature.RemovePhenomena();
        }
    }
    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.RTSpecific.CustomizePsychicPhenomenaFeature";
        }
    }
}
