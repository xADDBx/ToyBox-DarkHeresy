using Kingmaker;
using Kingmaker.Blueprints.Items;
using ToyBox.Classes.Infrastructure.Features;

namespace ToyBox.Features.BagOfTricks.Common;

public partial class GiveAllItemsFeature : FeatureWithBindableAction {
    [LocalizedString("ToyBox_Features_BagOfTricks_Common_GiveAllItemsFeature_Name", "Give All Items")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Common_GiveAllItemsFeature_Description", "Adds 1 of each BlueprintItem to the player inventory.")]
    public override partial string Description { get; }
    public override bool CanExecute(ActionParameter parameter) {
        return IsInGame();
    }
    public override void ExecuteAction(ActionParameter parameter) {
        _ = BPLoader.GetBlueprintsOfType<BlueprintItem>(bps => {
            if (CanExecute(parameter)) {
                LogExecution(parameter);
                var inv = GetMainInventory()!;
                foreach (var bp in bps) {
                    inv.Add(bp, 1, null, false);
                }
            }
        });
    }
}
