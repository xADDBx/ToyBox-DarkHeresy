using Kingmaker.AreaLogic.Etudes;
using Kingmaker.ElementsSystem;

namespace ToyBox.Features.Etudes;
public class EtudeRecord {
    public BlueprintEtude Blueprint { get; }

    public string Name => BPHelper.GetTitle(Blueprint);

    public string ParentId = "";
    public readonly List<string> Children = [];

    public readonly List<string> LinkedIds = [];
    public readonly List<string> ChainedIds = [];
    public string LinkedTo = "";
    public string ChainedTo = "";

    public readonly List<string> ConflictingGroups = [];

    public string LinkedAreaId = "";
    public string Comment = "";
    public int Priority;

    public bool CompletesParent;
    public bool AllowActionStart;
    public EtudeState State;

    public readonly List<Element> Elements = [];

    public EtudeRecord(BlueprintEtude blueprint) {
        Blueprint = blueprint;
    }
}
