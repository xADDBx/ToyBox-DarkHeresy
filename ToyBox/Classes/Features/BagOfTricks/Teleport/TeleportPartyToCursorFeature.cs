using Kingmaker;
using Kingmaker.Cheats;
using Kingmaker.GameModes;
using ToyBox.Classes.Infrastructure.Features;

namespace ToyBox.Features.BagOfTricks.Teleport;

public partial class TeleportPartyToCursorFeature : FeatureWithBindableAction {
    [LocalizedString("ToyBox_Features_BagOfTricks_Teleport_TeleportPartyToCursorFeature_Name", "Teleport Party Characters To Cursor")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Teleport_TeleportPartyToCursorFeature_Description", "Teleports all party units and pets to the position your mouse points at.")]
    public override partial string Description { get; }
    public override bool CanExecute(ActionParameter parameter) {
        return IsInGame() && (Game.Instance.CurrentModeType == GameModeType.Default || Game.Instance.CurrentModeType == GameModeType.Pause);
    }
    public override void ExecuteAction(ActionParameter parameter) {
        if (CanExecute(parameter)) {
            var position = GetCursorPositionInWorld();
            var units = Game.Instance.Player.PartyAndPets ?? [];
            LogExecution(position, units);
            CheatsTransfer.LocalTeleport(position, units);
        }
    }
}
