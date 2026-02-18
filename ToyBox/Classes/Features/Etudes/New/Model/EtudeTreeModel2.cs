using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Utility.DotNetExtensions;

namespace ToyBox.Features.Etudes;

public class EtudesTreeModel2 {
    private bool m_IsBuildingSnapshot = false;
    public Exception? ErrorDuringBuild;
    private EtudesSnapshot? m_Snapshot;

    public void Invalidate() {
        lock (this) {
            m_IsBuildingSnapshot = false;
            m_Snapshot = null;
            ErrorDuringBuild = null;
        }
    }

    public bool TryEnsureSnapshot(out EtudesSnapshot? snapshot) {
        snapshot = null;
        if (m_Snapshot != null) {
            snapshot = m_Snapshot;
            return true;
        }

        if (ErrorDuringBuild == null) {
            var bps = BPLoader.GetBlueprintsOfType<BlueprintEtude>();
            if (bps != null) {
                lock (this) {
                    if (!m_IsBuildingSnapshot) {
                        m_IsBuildingSnapshot = true;
                        _ = Task.Run(() => BuildSnapshotThreaded(bps));
                    }
                }
            }
        }
        return false;
    }
    private void BuildSnapshotThreaded(IEnumerable<BlueprintEtude> bps) {
        try {
            var records = new Dictionary<string, EtudeRecord>();
            var childrenByParent = new Dictionary<string, List<string>>();
            var conflictingGroups = new Dictionary<string, ConflictingGroupInfo>();

            foreach (var bp in bps) {
                var id = bp.AssetGuid;
                var rec = new EtudeRecord(bp);
                records[id] = rec;

                var parentId = bp.Parent?.Get()?.AssetGuid ?? string.Empty;
                rec.ParentId = parentId;

                if (!string.IsNullOrEmpty(parentId)) {
                    if (!childrenByParent.TryGetValue(parentId, out var list)) {
                        list = [];
                        childrenByParent[parentId] = list;
                    }
                    list.Add(id);
                }

                rec.CompletesParent = bp.CompletesParent;
                rec.Comment = bp.Comment ?? "";
                rec.Priority = bp.Priority;

                if (bp.LinkedAreaPart != null) {
                    rec.LinkedAreaId = bp.LinkedAreaPart.AssetGuid;
                }

                // Chained / Linked edges
                foreach (var chained in bp.StartsOnComplete) {
                    var c = chained.Get();
                    if (c != null) {
                        rec.ChainedIds.Add(c.AssetGuid);
                    }
                }

                foreach (var linked in bp.StartsWith) {
                    var l = linked.Get();
                    if (l != null) {
                        rec.LinkedIds.Add(l.AssetGuid);
                    }
                }

                // Elements list (cheap reference; no heavy processing)
                if (bp.m_AllElements != null) {
                    rec.Elements.AddRange(bp.m_AllElements);
                }

                // Conflicting groups
                foreach (var g in bp.ConflictingGroups) {
                    var gBp = g.GetBlueprint();
                    if (gBp == null) {
                        continue;
                    }
                    rec.ConflictingGroups.Add(gBp.AssetGuid);
                    if (!conflictingGroups.TryGetValue(gBp.AssetGuid, out var info)) {
                        info = new ConflictingGroupInfo(gBp.AssetGuid, gBp.name);
                        conflictingGroups[gBp.AssetGuid] = info;
                    }
                    _ = info.Etudes.Add(id);
                }
            }

            // wire children + reverse edges (linkedTo/chainedTo)
            foreach (var (id, rec) in records) {
                if (childrenByParent.TryGetValue(id, out var children)) {
                    rec.Children.AddRange(children);
                }
                foreach (var child in rec.LinkedIds) {
                    if (records.TryGetValue(child, out var childRec)) {
                        childRec.LinkedTo = id;
                    }
                }
                foreach (var child in rec.ChainedIds) {
                    if (records.TryGetValue(child, out var childRec)) {
                        childRec.ChainedTo = id;
                    }
                }
            }

            var snapshot = new EtudesSnapshot(records, conflictingGroups);
            snapshot.BuildDerivedSets();

            Main.ScheduleForMainThread(() => {
                lock (this) {
                    if (m_IsBuildingSnapshot) {
                        m_IsBuildingSnapshot = false;
                        m_Snapshot = snapshot;
                    }
                }
            });
        } catch (Exception ex) {
            Critical($"Failed to build Etudes snapshot:\n{ex}", false);
            Main.ScheduleForMainThread(() => {
                lock (this) {
                    m_IsBuildingSnapshot = false;
                    ErrorDuringBuild = ex;
                }
            });
        }
    }
}

