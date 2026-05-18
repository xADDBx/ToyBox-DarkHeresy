using Kingmaker.EntitySystem.Stats.Base;

namespace ToyBox.Infrastructure;

public static class Constants {
    public const string LinkToIncompatibilitiesFile = "https://raw.githubusercontent.com/xADDBx/ToyBox-DarkHeresy/refs/heads/main/ToyBox/Incompatibilities.json";
    public const string SaveFileKey = "ToyBox.SaveSpecificSettings";
    public static readonly HashSet<StatType> WeirdStats = [StatType.Unknown];
    public const string RootEtudeId = "0d8d0f32e4f1486e8e34061bc7309fcc";
}
