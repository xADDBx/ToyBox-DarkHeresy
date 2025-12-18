using Kingmaker.EntitySystem.Stats.Base;

namespace ToyBox.Infrastructure;

public static class Constants {
    public const string LinkToIncompatibilitiesFile = "https://raw.githubusercontent.com/xADDBx/ToyBox-DarkHeresy/refs/heads/main/ToyBox/Incompatibilities.json?token=GHSAT0AAAAAACJKW6P26FBVHRMEXZPULTOK2KDMC2Q";
    public const string SaveFileKey = "ToyBox.SaveSpecificSettings";
    public static readonly HashSet<StatType> WeirdStats = [StatType.Unknown];
}
