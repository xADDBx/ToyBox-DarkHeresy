namespace ToyBox.Features.Etudes;
public class ConflictingGroupInfo {
    public string Id { get; }
    public string Name { get; }
    public HashSet<string> Etudes { get; } = [];

    public ConflictingGroupInfo(string id, string name) {
        Id = id;
        Name = name;
    }
}
