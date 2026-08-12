namespace ToyBox.Features.Achievements;

public partial class AchievementsFeatureTab : FeatureTab {
    [LocalizedString("ToyBox_Features_Achievements_AchievementsFeatureTab_Name", "Achievements")]
    public override partial string Name { get; }
    public AchievementsFeatureTab() {
        AddFeature(new BrowseAchievementsFeature());
    }
}
