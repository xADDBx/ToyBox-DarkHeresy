using Kingmaker;
using Kingmaker.UnitLogic;
using ToyBox.Classes.Infrastructure.Features;

namespace ToyBox.Features.BagOfTricks.Combat;

public partial class LobotomizeEnemiesFeature : FeatureWithBindableAction {
    [LocalizedString("ToyBox_Features_BagOfTricks_Combat_LobotomizeEnemiesFeature_Name", "Lobotomize Enemies")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Combat_LobotomizeEnemiesFeature_Description", "Makes enemies unable to act, move and perform attack of opportunities.")]
    public override partial string Description { get; }
    public override bool CanExecute(ActionParameter parameter) {
        return IsInGame() && Game.Instance.Player.IsInCombat;
    }
    public override void ExecuteAction(ActionParameter parameter) {
        if (CanExecute(parameter)) {
            var units = Game.Instance.EntityPools?.AllBaseUnits ?? [];
            LogExecution(units);
            foreach (var unit in units) {
                if (unit.IsInCombat && unit.IsPlayerEnemy) {
                    unit.GetMechanicFeature(Kingmaker.UnitLogic.Enums.MechanicsFeatureType.DisableAttacksOfOpportunity).Retain();
                    unit.GetMechanicFeature(Kingmaker.UnitLogic.Enums.MechanicsFeatureType.CantAct).Retain();
                    unit.GetMechanicFeature(Kingmaker.UnitLogic.Enums.MechanicsFeatureType.CantMove).Retain();
                }
            }
        }
    }
}
