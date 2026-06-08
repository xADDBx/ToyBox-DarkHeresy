namespace ToyBox.Features.Etudes;

public partial class EtudesFeatureTab : FeatureTab {
    [LocalizedString("ToyBox_Features_Etudes_EtudesFeatureTab_Name", "Etudes")]
    public override partial string Name { get; }
    public EtudesFeatureTab() {
#warning Implement Completely?
        // AddFeature(new LegacyEtudesEditorFeature());
        AddFeature(new EtudesEditorFeature());
    }
}
