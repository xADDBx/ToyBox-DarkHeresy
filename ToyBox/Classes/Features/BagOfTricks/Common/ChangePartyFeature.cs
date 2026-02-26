using Kingmaker;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.GameModes;
using ToyBox.Classes.Infrastructure.Features;

namespace ToyBox.Features.BagOfTricks.Common;

public partial class ChangePartyFeature : FeatureWithBindableAction {
    [LocalizedString("ToyBox_Features_BagOfTricks_Common_ChangePartyFeature_Name", "Change Party")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Common_ChangePartyFeature_Description", "Opens the party member selection screen.")]
    public override partial string Description { get; }
    public override bool CanExecute(ActionParameter parameter) {
        return IsInGame() && (Game.Instance.CurrentModeType == GameModeType.Default || Game.Instance.CurrentModeType == GameModeType.Pause || Game.Instance.CurrentModeType == GameModeType.GlobalMap);
    }
    public override void ExecuteAction(ActionParameter parameter) {
        if (CanExecute(parameter)) {
            LogExecution(parameter);
            ToggleModWindow();
            new ShowPartySelection() {
                ActionsAfterPartySelection = new(),
                ActionsIfCanceled = new(),
                AllowRemoteCompanionsAnywhere = true
            }.Run();
        }
    }
}
