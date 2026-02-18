using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using ToyBox.Infrastructure.Blueprints.BlueprintActions;
using ToyBox.Infrastructure.Inspector;
using UnityEngine;

namespace ToyBox.Features.Etudes;

public partial class EtudesEditorFeature : Feature {
    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_Name", "Etudes Editor")]
    public override partial string Name { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_Description", "Browse the game's Etudes in a tree view, with state and relationship details.")]
    public override partial string Description { get; }

    private readonly EtudesTreeModel2 m_Model = new();

    private Browser<string>? m_AreaBrowser;
    private Dictionary<string, BlueprintArea?> m_AreaByName = [];
    private BlueprintArea? m_SelectedArea;

    private string m_SearchText = "";
    private bool m_ShowOnlyFlagLike = false;

    private readonly HashSet<string> m_ExpandedEtudes = [];
    private readonly HashSet<string> m_ExpandedElements = [];
    private readonly HashSet<string> m_ExpandedConflicts = [];

    private readonly HashSet<string> m_Enclosing = [];

    public override void Enable() {
        Main.OnLocaleChanged += ClearCaches;
        Main.OnHideGUIAction += ClearCaches;
    }

    public override void Disable() {
        Main.OnLocaleChanged -= ClearCaches;
        Main.OnHideGUIAction -= ClearCaches;
    }

    private void ClearCaches() {
        m_Model.Invalidate();
        m_AreaBrowser = null;
        m_AreaByName.Clear();
        m_SelectedArea = null;
        m_ExpandedEtudes.Clear();
        m_ExpandedElements.Clear();
        m_ExpandedConflicts.Clear();
        m_Enclosing.Clear();
    }

    private string m_LastSearchText = "";

    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            return;
        }

        if (m_Model.ErrorDuringBuild != null) {
            UI.Label(m_EncounteredErrorWhileBuildingEtuLocalizedText.Red().Bold() + $":\n{m_Model.ErrorDuringBuild}");
            return;
        }

        if (!m_Model.TryEnsureSnapshot(out var snapshot) || snapshot == null) {
            UI.Label(m_LoadingText.Orange().Bold());
            return;
        }

        EnsureAreaBrowser();

        using (VerticalScope()) {
            DrawHeader(snapshot);

            if (!string.Equals(m_LastSearchText, m_SearchText, StringComparison.Ordinal)) {
                m_LastSearchText = m_SearchText;
                snapshot.UpdateSearchClosure(m_SearchText);
            }

            using (HorizontalScope()) {
                DrawLeftPane();
                Space(10);
                DrawTreePane(snapshot);
            }
        }
    }

    private void DrawHeader(EtudesSnapshot snapshot) {
        using (HorizontalScope()) {
            UI.Label((m_RootText + ": ").Cyan() + EtudesEditor.rootEtudeId.Orange(), AutoWidth());
            Space(20);

            UI.Label(m_SearchTextLabel.Cyan(), AutoWidth());
            _ = UI.ActionTextField(ref m_SearchText, "EtudesTreeSearch", null, _ => { }, Width(350));

            Space(20);
            if (UI.Toggle(m_FlagsOnlyText.Cyan(), null, ref m_ShowOnlyFlagLike)) {
                // no-op; evaluated during filtering
            }

            Space(20);
            UI.Toggle(m_ShowGuidsText.Cyan(), null, ref Settings.showAssetIDs);

            Space(20);
            UI.Toggle(m_ShowCommentsText.Cyan(), null, ref Settings.showEtudeComments);

            Space(20);
            if (UI.Button(m_RefreshText.Cyan(), () => {
                m_Model.Invalidate();
                m_Model.TryEnsureSnapshot(out _);
            })) { }
        }

        if (!snapshot.EtudesById.ContainsKey(EtudesEditor.rootEtudeId)) {
            UI.Label((m_RootNotFoundText + " ").Red().Bold() + EtudesEditor.rootEtudeId.Orange());
        }
    }

    private void EnsureAreaBrowser() {
        if (m_AreaBrowser != null) {
            return;
        }

        // Create minimal list synchronously; update once areas arrive.
        m_AreaByName["All"] = null;
        m_AreaBrowser = new(
            sortKey: s => s == "All" ? "" : s,
            searchKey: s => s,
            initialItems: m_AreaByName.Keys,
            showDivBetweenItems: false,
            overridePageWidth: (int)(300 * Main.UIScale),
            orderInitialCollection: true);

        // Populate areas async via BPLoader
        BPLoader.GetBlueprintsOfType<BlueprintArea>(bps => {
            Main.ScheduleForMainThread(() => {
                foreach (var bp in bps) {
                    var title = BPHelper.GetTitle(bp);
                    if (!string.IsNullOrWhiteSpace(title)) {
                        m_AreaByName[title] = bp;
                    }
                }
                m_AreaByName["All"] = null;
                m_AreaBrowser.UpdateItems(m_AreaByName.Keys);
            });
        });
    }

    private void DrawLeftPane() {
        using (VerticalScope(GUI.skin.box, Width(300 * Main.UIScale))) {
            UI.Label(m_AreaFilterText.Cyan().Bold());

            m_AreaBrowser!.OnGUI(areaName => {
                var area = m_AreaByName.TryGetValue(areaName, out var a) ? a : null;
                var selected = m_SelectedArea == area;

                if (selected) {
                    GUILayout.Toggle(true, areaName.Orange(), UI.LeftAlignedButtonStyle);
                } else {
                    if (GUILayout.Toggle(false, areaName.Cyan(), UI.LeftAlignedButtonStyle)) {
                        m_SelectedArea = area;
                    }
                }
            });
        }
    }

    private void DrawTreePane(EtudesSnapshot snapshot) {
        using (VerticalScope(GUI.skin.box, Width((EffectiveWindowWidth() - 320 * Main.UIScale)))) {
            if (!snapshot.EtudesById.TryGetValue(EtudesEditor.rootEtudeId, out var root)) {
                UI.Label(m_NoEtudesText.Red().Bold());
                return;
            }

            // Refresh runtime state cheaply when playing
            if (Application.isPlaying) {
                snapshot.UpdateRuntimeStates();
            }

            DrawEtudeRecursive(snapshot, EtudesEditor.rootEtudeId, indent: 0, ignoreFilter: false);
        }
    }

    private bool PassesFilter(EtudesSnapshot snapshot, string etudeId) {
        if (!snapshot.EtudesById.TryGetValue(etudeId, out var e)) {
            return false;
        }

        if (m_SelectedArea != null) {
            // show only etudes linked to that area OR ancestors/descendants as per legacy logic:
            // easiest equivalent: precomputed "area closure" set
            if (!snapshot.AreaClosureByAreaId.TryGetValue(m_SelectedArea.AssetGuid, out var allowed) || !allowed.Contains(etudeId)) {
                return false;
            }
        }

        if (m_ShowOnlyFlagLike) {
            if (!snapshot.FlagLikeClosure.Contains(etudeId)) {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(m_SearchText)) {
            var q = m_SearchText;
            if (!MatchString(e.Name ?? "", q) && !MatchString(etudeId, q)) {
                return false;
            }
        }

        return true;
    }

    private void DrawEtudeRecursive(EtudesSnapshot snapshot, string etudeId, int indent, bool ignoreFilter) {
        if (!snapshot.EtudesById.TryGetValue(etudeId, out var e)) {
            return;
        }

        if (m_Enclosing.Contains(etudeId)) {
            return;
        }

        if (!ignoreFilter && !PassesFilter(snapshot, etudeId)) {
            // If not matched, still recurse into children only if search is active and a descendant matches.
            // Snapshot precomputes this.
            if (!snapshot.SearchClosure.Contains(etudeId)) {
                return;
            }
        }

        m_Enclosing.Add(etudeId);

        try {
            using (HorizontalScope(GUILayout.ExpandWidth(true))) {
                // Actions (same as legacy)
                using (HorizontalScope(Width(310 * Main.UIScale))) {
                    foreach (var action in BlueprintActionFeature.GetActionsForBlueprintType<BlueprintEtude>()) {
                        _ = action.OnGui(e.Blueprint, false);
                    }
                }

                Space(indent * (int)(35 * Main.UIScale));

                var expanded = m_ExpandedEtudes.Contains(etudeId);
                var name = (e.Name ?? "<null>").Orange().Bold();
                if (UI.DisclosureToggle(ref expanded, name)) {
                    SetExpanded(m_ExpandedEtudes, etudeId, expanded);
                }

                Space(15);

                UI.Label(e.State.ToString().Yellow(), Width(120 * Main.UIScale));

                Space(10);

                var validation = snapshot.GetValidationProblem(etudeId);
                if (!string.IsNullOrEmpty(validation)) {
                    UI.Label(validation.Cyan().Yellow(), Width(420 * Main.UIScale));
                } else {
                    UI.Label("", Width(420 * Main.UIScale));
                }

                Space(10);

                if (e.CompletesParent) {
                    UI.Label(m_CompletesParentText.Cyan(), AutoWidth());
                    Space(10);
                }
                if (e.AllowActionStart) {
                    UI.Label(m_CanStartText.Cyan(), AutoWidth());
                    Space(10);
                }

                InspectorUI.InspectToggle(e.Blueprint, m_InspectText, options: Width(100 * Main.UIScale));

                if (Settings.showAssetIDs) {
                    var id = etudeId;
                    UI.TextField(ref id, null, Width(260 * Main.UIScale));
                }

                if (Settings.showEtudeComments && !Settings.showAssetIDs && !string.IsNullOrWhiteSpace(e.Comment)) {
                    UI.Label(e.Comment.Green(), GUILayout.ExpandWidth(true));
                }
            }

            // Expanded details
            if (m_ExpandedEtudes.Contains(etudeId)) {
                DrawEtudeDetails(snapshot, etudeId, indent + 1);
            }

            // Children
            if (m_ExpandedEtudes.Contains(etudeId) || snapshot.SearchClosure.Contains(etudeId)) {
                foreach (var child in e.Children) {
                    DrawEtudeRecursive(snapshot, child, indent + 1, ignoreFilter);
                }
            }
        } finally {
            m_Enclosing.Remove(etudeId);
        }
    }

    private void DrawEtudeDetails(EtudesSnapshot snapshot, string etudeId, int indent) {
        var e = snapshot.EtudesById[etudeId];

        using (HorizontalScope()) {
            Space(indent * (int)(35 * Main.UIScale));
            using (VerticalScope(GUI.skin.box)) {
                // Elements toggle
                var elementsExpanded = m_ExpandedElements.Contains(etudeId);
                var eltCount = e.Elements.Count;
                if (eltCount > 0) {
                    if (UI.DisclosureToggle(ref elementsExpanded, (m_ElementsText + $" ({eltCount})").Cyan())) {
                        SetExpanded(m_ExpandedElements, etudeId, elementsExpanded);
                    }
                }

                // Conflicts toggle
                var conflicts = snapshot.GetConflictingEtudes(etudeId);
                var conflictsExpanded = m_ExpandedConflicts.Contains(etudeId);
                if (conflicts.Count > 1) {
                    if (UI.DisclosureToggle(ref conflictsExpanded, (m_ConflictsText + $" ({conflicts.Count - 1})").Cyan())) {
                        SetExpanded(m_ExpandedConflicts, etudeId, conflictsExpanded);
                    }
                }

                if (elementsExpanded) {
                    Div.DrawDiv();
                    DrawElements(snapshot, e, indent);
                }

                if (conflictsExpanded) {
                    Div.DrawDiv();
                    foreach (var c in conflicts) {
                        DrawEtudeRecursive(snapshot, c, indent + 1, ignoreFilter: true);
                    }
                }
            }
        }
    }

    private void DrawElements(EtudesSnapshot snapshot, EtudeRecord e, int indent) {
        foreach (var element in e.Elements) {
            using (HorizontalScope(GUILayout.ExpandWidth(true))) {
                Space((indent + 1) * (int)(35 * Main.UIScale));

                using (HorizontalScope(420 * Main.UIScale)) {
                    if (element is GameAction ga) {
                        UI.Button((ga.GetCaption() ?? "?").Yellow(), () => {
                            try {
                                ga.RunAction();
                            } catch (Exception ex) {
                                Warn($"Failed to run action {ga.GetCaption()}:\n{ex}");
                            }
                        });
                    } else {
                        UI.Label((element.GetCaption() ?? "?").Yellow());
                    }

                    Space(10);
                    InspectorUI.InspectToggle(element, m_InspectText, options: Width(100 * Main.UIScale));
                }

                Space(10);
                if (element is Condition cond) {
                    UI.Label($"{element.GetType().Name.Cyan()} : {cond.CheckCondition().ToString().Orange()}", Width(420 * Main.UIScale));
                } else if (element is Conditional conditional) {
                    var caption = string.Join(", ", conditional.ConditionsChecker.Conditions.Select(c => c.GetCaption()));
                    UI.Label($"{element.GetType().Name.Cyan()} : {conditional.ConditionsChecker.Check().ToString().Orange()} - {caption.Yellow()}", Width(420 * Main.UIScale));
                } else {
                    UI.Label(element.GetType().Name.Cyan(), Width(420 * Main.UIScale));
                }

                if (Settings.showEtudeComments) {
                    UI.Label(element.GetDescription().Green(), GUILayout.ExpandWidth(true));
                }
            }

            // Cross-links (same behavior as legacy)
            if (element is StartEtude started) {
                DrawEtudeRecursive(snapshot, started.Etude.Guid, indent + 2, ignoreFilter: true);
            } else if (element is EtudeStatus status) {
                DrawEtudeRecursive(snapshot, status.m_Etude.Guid, indent + 2, ignoreFilter: true);
            } else if (element is CompleteEtude completed) {
                DrawEtudeRecursive(snapshot, completed.Etude.Guid, indent + 2, ignoreFilter: true);
            }
        }
    }

    private static void SetExpanded(HashSet<string> set, string key, bool expanded) {
        if (expanded) {
            _ = set.Add(key);
        } else {
            _ = set.Remove(key);
        }
    }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_LoadingText", "Loading Etudes...")]
    private static partial string m_LoadingText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_RootText", "Root")]
    private static partial string m_RootText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_RootNotFoundText", "Root etude not found in current snapshot:")]
    private static partial string m_RootNotFoundText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_SearchTextLabel", "Search")]
    private static partial string m_SearchTextLabel { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_FlagsOnlyText", "Flags Only")]
    private static partial string m_FlagsOnlyText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_ShowGuidsText", "Show GUIDs")]
    private static partial string m_ShowGuidsText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_ShowCommentsText", "Show Comments")]
    private static partial string m_ShowCommentsText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_RefreshText", "Refresh")]
    private static partial string m_RefreshText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_AreaFilterText", "Area Filter")]
    private static partial string m_AreaFilterText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_NoEtudesText", "No Etudes")]
    private static partial string m_NoEtudesText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_CompletesParentText", "Completes Parent")]
    private static partial string m_CompletesParentText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_CanStartText", "Can Start")]
    private static partial string m_CanStartText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_InspectText", "Inspect")]
    private static partial string m_InspectText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_ElementsText", "Elements")]
    private static partial string m_ElementsText { get; }

    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_ConflictsText", "Conflicts")]
    private static partial string m_ConflictsText { get; }
    [LocalizedString("ToyBox_Features_Etudes_EtudesFeature_m_EncounteredErrorWhileBuildingEtuLocalizedText", "Encountered error while building Etudes Tree!")]
    private static partial string m_EncounteredErrorWhileBuildingEtuLocalizedText { get; }
}
