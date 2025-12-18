using Kingmaker.EntitySystem.Stats.Base;

namespace ToyBox.Infrastructure;

public static class Constants {
    public const string LinkToIncompatibilitiesFile = "https://raw.githubusercontent.com/xADDBx/ToyBox-DarkHeresy/main/ToyBox/Incompatibilities.json";
    public const string SaveFileKey = "ToyBox.SaveSpecificSettings";
    public static readonly HashSet<StatType> WeirdStats = [StatType.Unknown];
}
