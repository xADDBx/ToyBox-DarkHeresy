using Kingmaker;
using Kingmaker.AreaLogic.Etudes;

namespace ToyBox.Features.Etudes;

public class EtudesSnapshot {
    public readonly Dictionary<string, EtudeRecord> EtudesById;
    public readonly Dictionary<string, ConflictingGroupInfo> ConflictingGroups;

    public readonly Dictionary<string, HashSet<string>> AreaClosureByAreaId = [];
    public readonly HashSet<string> FlagLikeClosure = [];
    public readonly HashSet<string> SearchClosure = [];

    public EtudesSnapshot(Dictionary<string, EtudeRecord> etudesById, Dictionary<string, ConflictingGroupInfo>? conflictingGroups = null) {
        EtudesById = etudesById;
        ConflictingGroups = conflictingGroups ?? [];
    }

    public void BuildDerivedSets() {
        BuildAreaClosure();
        BuildFlagLikeClosure();
        SearchClosure.Clear(); // computed per query via UpdateSearchClosure(...)
    }

    public void UpdateSearchClosure(string? query) {
        SearchClosure.Clear();

        if (string.IsNullOrWhiteSpace(query)) {
            // legacy: when search is empty, no extra expansion is needed
            return;
        }

        foreach (var pair in EtudesById) {
            if (MatchString(pair.Value.Name ?? "", query!) || MatchString(pair.Key, query!)) {
                AddWithAncestors(pair.Key, SearchClosure);
            }
        }
    }

    private void BuildAreaClosure() {
        AreaClosureByAreaId.Clear();

        foreach (var pair in EtudesById) {
            if (string.IsNullOrEmpty(pair.Value.LinkedAreaId)) continue;

            if (!AreaClosureByAreaId.TryGetValue(pair.Value.LinkedAreaId, out var set)) {
                set = [];
                AreaClosureByAreaId[pair.Value.LinkedAreaId] = set;
            }

            AddWithAncestorsAndDescendants(pair.Key, set);
        }
    }

    private void BuildFlagLikeClosure() {
        FlagLikeClosure.Clear();

        foreach (var pair in EtudesById) {
            var isFlagLike =
                string.IsNullOrEmpty(pair.Value.ChainedTo) &&
                string.IsNullOrEmpty(pair.Value.LinkedTo) &&
                string.IsNullOrEmpty(pair.Value.LinkedAreaId) &&
                !ParentHasArea(pair.Value);

            if (isFlagLike) {
                AddWithAncestors(pair.Key, FlagLikeClosure);
            }
        }
    }

    private bool ParentHasArea(EtudeRecord e) {
        var cur = e;
        while (!string.IsNullOrEmpty(cur.ParentId) && EtudesById.TryGetValue(cur.ParentId, out var parent)) {
            if (!string.IsNullOrEmpty(parent.LinkedAreaId)) return true;
            cur = parent;
        }
        return false;
    }

    private void AddWithAncestorsAndDescendants(string id, HashSet<string> set) {
        AddWithAncestors(id, set);
        AddWithDescendants(id, set);
    }

    private void AddWithAncestors(string id, HashSet<string> set) {
        var curId = id;
        while (!string.IsNullOrEmpty(curId) && EtudesById.TryGetValue(curId, out var e)) {
            _ = set.Add(curId);
            curId = e.ParentId;
        }
    }

    private void AddWithDescendants(string id, HashSet<string> set) {
        if (!EtudesById.ContainsKey(id)) return;

        var stack = new Stack<string>();
        stack.Push(id);

        while (stack.Count > 0) {
            var cur = stack.Pop();
            if (!set.Add(cur)) continue;

            if (EtudesById.TryGetValue(cur, out var e)) {
                foreach (var c in e.Children) stack.Push(c);
            }
        }
    }

    public void UpdateRuntimeStates() {
        foreach (var e in EtudesById.Values) {
            e.State = GetState(e.Blueprint);
            e.AllowActionStart = GetAllowActionStart(e.Blueprint);
        }
    }

    private static EtudeState GetState(BlueprintEtude blueprintEtude) {
        try {
            var tree = Game.Instance.EtudesSystem?.Etudes;
            if (tree == null) return EtudeState.NotStarted;

            var fact = tree.Get(blueprintEtude);
            if (fact != null) {
                if (fact.IsCompleted) return EtudeState.Completed;
                if (fact.CompletionInProgress) return EtudeState.CompletionBlocked;
                return fact.IsPlaying ? EtudeState.Active : EtudeState.Started;
            }

            if (Game.Instance.EtudesSystem!.EtudeIsPreCompleted(blueprintEtude)) return EtudeState.CompleteBeforeActive;
            if (Game.Instance.EtudesSystem.EtudeIsCompleted(blueprintEtude)) return EtudeState.Completed;
            return EtudeState.NotStarted;
        } catch {
            return EtudeState.NotStarted;
        }
    }

    private static bool GetAllowActionStart(BlueprintEtude blueprintEtude) {
        try {
            var tree = Game.Instance.EtudesSystem?.Etudes;
            var fact = tree?.Get(blueprintEtude);
            return fact == null || (tree?.EtudeCanPlay(fact) ?? true);
        } catch {
            return true;
        }
    }

    public List<string> GetConflictingEtudes(string etudeId) {
        if (!EtudesById.TryGetValue(etudeId, out var e)) return [];

        var result = new HashSet<string>();
        foreach (var groupId in e.ConflictingGroups) {
            if (ConflictingGroups.TryGetValue(groupId, out var g)) {
                foreach (var id in g.Etudes) _ = result.Add(id);
            }
        }

        return [.. result.OrderBy(id => {
            var rec = EtudesById[id];
            var activeBoost = rec.State == EtudeState.Active ? 100500 : 0;
            return -(rec.Priority + activeBoost);
        })];
    }

    public string? GetValidationProblem(string etudeId) {
        if (!EtudesById.TryGetValue(etudeId, out var e)) return null;

        if (string.IsNullOrEmpty(e.ChainedTo) && string.IsNullOrEmpty(e.LinkedTo))
            return "Chained/Linked to Nothing";

        foreach (var chained in e.ChainedIds) {
            if (!EtudesById.TryGetValue(chained, out var chainedEtude)) continue;
            if (chainedEtude.ParentId != e.ParentId) {
                return $"Chained etude {chainedEtude.Name} ({chainedEtude.Blueprint.AssetGuid}) has different parent: {chainedEtude.ParentId} than {e.Name} parent: {e.ParentId}";
            }
        }

        foreach (var linked in e.LinkedIds) {
            if (!EtudesById.TryGetValue(linked, out var linkedEtude)) continue;
            if (linkedEtude.ParentId != e.ParentId && linkedEtude.ParentId != etudeId) {
                return $"Linked to child {linkedEtude.Name} ({linkedEtude.Blueprint.AssetGuid}) with different parent: {linkedEtude.ParentId} than {e.Name} parent {e.ParentId}";
            }
        }

        return null;
    }
}
