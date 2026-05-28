using Code.View.UI.UIUtils;
using Kingmaker;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Gameplay.Features.Reputation;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.RTSpecific;

public partial class ModifyFactionReputationFeature : Feature {
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_ModifyFactionReputationFeature_Name", "Modify Faction Reputation")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_ModifyFactionReputationFeature_Description", "Allows you to modify the reputation at the various in-game factions.")]
    public override partial string Description { get; }
    private List<FactionType>? m_ValidFactions = null;
    private FactionType m_SelectedFaction = FactionType.None;
    private int m_Adjustment = 100;
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            return;
        }
        using (HorizontalScope()) {
            UI.Label(Name);
            Space(10);
            UI.Label(Description.Green());
        }
        if (m_ValidFactions == null) {
            m_ValidFactions = [FactionType.None];
            foreach (var e in Enum.GetValues(typeof(FactionType))) {
                var e2 = (FactionType)e;
                if (Game.Instance.Reputation._reputation.ContainsKey(e2)) {
                    m_ValidFactions.Add(e2);
                }
            }
        }
        _ = UI.SelectionGrid(ref m_SelectedFaction, m_ValidFactions, 6, @enum => UIStrings.Instance.CharacterSheet.GetFactionLabel(@enum));
        if (m_SelectedFaction != FactionType.None) {
            using (HorizontalScope()) {
                UI.Label(m_CurrentReputationLocalizedText.Bold() + ": ", Width(250 * Main.UIScale));
                using (VerticalScope()) {
                    using (HorizontalScope()) {
                        UI.Label(UIUtilityFaction.GetEncyclopediaName(ReputationType.Fear) + ": ", Width(100 * Main.UIScale));
                        UI.Label(Game.Instance.Reputation.Get(m_SelectedFaction, ReputationType.Fear).ToString());
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_AdjustReputationByTheFollowingAmLocalizedText + ":");
                        if (UI.TextField(ref m_Adjustment, null, GUILayout.MinWidth(200), AutoWidth())) {
                            m_Adjustment = m_Adjustment < 0 ? 1 : m_Adjustment;
                        }
                        Space(10);
                        _ = UI.Button(m_AddLocalizedText, () => Game.Instance.Reputation.AddFear(m_SelectedFaction, m_Adjustment));
                        Space(10);
                        _ = UI.Button(m_RemoveLocalizedText, () => Game.Instance.Reputation.AddFear(m_SelectedFaction, -m_Adjustment));
                    }
                    using (HorizontalScope()) {
                        UI.Label(UIUtilityFaction.GetEncyclopediaName(ReputationType.Respect) + ": ", Width(100 * Main.UIScale));
                        UI.Label(Game.Instance.Reputation.Get(m_SelectedFaction, ReputationType.Respect).ToString());
                    }
                    using (HorizontalScope()) {
                        UI.Label(m_AdjustReputationByTheFollowingAmLocalizedText + ":");
                        if (UI.TextField(ref m_Adjustment, null, GUILayout.MinWidth(200), AutoWidth())) {
                            m_Adjustment = m_Adjustment < 0 ? 1 : m_Adjustment;
                        }
                        Space(10);
                        _ = UI.Button(m_AddLocalizedText, () => Game.Instance.Reputation.AddRespect(m_SelectedFaction, m_Adjustment));
                        Space(10);
                        _ = UI.Button(m_RemoveLocalizedText, () => Game.Instance.Reputation.AddRespect(m_SelectedFaction, -m_Adjustment));
                    }
                }
            }
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_ModifyFactionReputationFeature_m_CurrentReputationLocalizedText", "Current Reputation")]
    private static partial string m_CurrentReputationLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_ModifyFactionReputationFeature_m_AdjustReputationByTheFollowingAmLocalizedText", "Adjust Reputation by the following amount")]
    private static partial string m_AdjustReputationByTheFollowingAmLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_ModifyFactionReputationFeature_m_AddLocalizedText", "Add")]
    private static partial string m_AddLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_ModifyFactionReputationFeature_m_RemoveLocalizedText", "Remove")]
    private static partial string m_RemoveLocalizedText { get; }
}
