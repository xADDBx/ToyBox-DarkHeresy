using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Entities;
using ToyBox.Classes.Infrastructure.Features;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Features.PartyTab.Stats;

public partial class ChangeGenderFeature : FeatureWithAction, INeedContextFeature<BaseUnitEntity> {
    [LocalizedString("ToyBox_Features_PartyTab_Stats_ChangeGenderFeature_Name", "Change Gender")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_ChangeGenderFeature_Description", "Toggles the characters gender between male and female.")]
    public override partial string Description { get; }

    public override bool CanExecute(ActionParameter parameter) {
        return parameter.UnitParam != null;
    }
    public override void ExecuteAction(ActionParameter parameter) {
        var unit = parameter.UnitParam!;
        switch (unit.Gender) {
            case Gender.Male: unit.Description?.SetGender(Gender.Female); break;
            case Gender.Female: unit.Description?.SetGender(Gender.Male); break;
            case Gender.Unknown:
                throw new NotImplementedException("Unknown Gender not supported by design (currently)");
            default:
                throw new NotSupportedException("Missing Gender Case?");
        }
    }
    public bool GetContext(out BaseUnitEntity? context) {
        return ContextProvider.BaseUnitEntity(out context, false);
    }

    public override void OnGui() {
        if (GetContext(out var unit)) {
            OnGui(unit!);
        }
    }
    public void OnGui(BaseUnitEntity unit) {
        using (HorizontalScope()) {
            UI.Label(Name + ": ");
            Space(5);
            var isUnknown = unit.Gender == Gender.Unknown;
            if (isUnknown) {
                UI.Label(m_UnitGenderIs_Unknown_AndDoesNotCLocalizedText);
            } else {
                var isFemale = unit.Gender == Gender.Female;
                if (UI.Button(isFemale ? "♀".Magenta() : "♂".Aqua(), null, null, Width(Main.UIScale * 40))) {
                    ExecuteAction(new(unit));
                }
            }
        }
    }

    [LocalizedString("ToyBox_Features_PartyTab_Stats_ChangeGenderFeature_m_UnitGenderIs_Unknown_AndDoesNotCLocalizedText", "Unit Gender is \"Unknown\" and does not currently support switching.")]
    private static partial string m_UnitGenderIs_Unknown_AndDoesNotCLocalizedText { get; }
}
