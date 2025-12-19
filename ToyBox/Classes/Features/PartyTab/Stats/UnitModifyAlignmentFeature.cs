using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Features.PartyTab.Stats;

public partial class UnitModifyAlignmentFeature : Feature, INeedContextFeature<BaseUnitEntity> {
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyAlignmentsFeature_Name", "Modify Alignments")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyAlignmentsFeature_Description", "Allows modifying the alignment of the character.")]
    public override partial string Description { get; }
    public bool GetContext(out BaseUnitEntity? context) {
        return ContextProvider.BaseUnitEntity(out context);
    }

    public override void OnGui() {
        if (GetContext(out var unit)) {
            using (HorizontalScope()) {
                OnGui(unit!);
            }
        }
    }
    public void OnGui(BaseUnitEntity unit) {
        if (unit.IsMainCharacter) {
            using (HorizontalScope()) {
                Space(100);
                UI.Label(m_AlignmentsLocalizedText.Cyan());
                using (VerticalScope()) {
                    foreach (AlignmentAxis direction in Enum.GetValues(typeof(AlignmentAxis))) {
                        if (direction == AlignmentAxis.None) {
                            continue;
                        }
                        try {
                            var mark = AlignmentShiftExtension.GetMainCharacterAlignmentMark(direction);
                            var rank = AlignmentShiftExtension.GetMainCharacterAlignmentRank(direction) + 1;
                            using (HorizontalScope()) {
                                Space(10);
                                UI.Label(UIUtilityText.GetSoulMarkDirectionText(direction), Width(80 * Main.UIScale));
                                if (rank > 0) {
                                    _ = UI.Button("<", () => {
                                        ModifyAlignment(direction, rank, rank - 1);
                                    });
                                }
                                UI.Label($" {rank - 1} ".Bold().Orange());
                                _ = UI.Button(">", () => {
                                    ModifyAlignment(direction, rank, rank + 1);
                                });
                                Space(10);
                                var val = rank - 1;
                                UI.TextField(ref val, pair => {
                                    if (pair.newContent >= 0) {
                                        ModifyAlignment(direction, rank, pair.newContent + 1);
                                    }
                                }, Width(75 * Main.UIScale));
                            }
                        } catch (Exception ex) {
                            Error(ex);
                        }
                    }
                }
            }
        } else {
            UI.Label(m_UnitsOtherThanTheMainCharacterDoLocalizedText);
        }
    }
    private static void ModifyAlignment(AlignmentAxis dir, int oldRank, int newRank) {
        var change = newRank - oldRank;
        var shift = new AlignmentShift() { Axis = dir, Value = change };
        AlignmentShiftExtension.ApplyShiftToMainCharacter(shift, new());
    }

    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyAlignmentsFeature_m_UnitsOtherThanTheMainCharacterDoLocalizedText", "Units other than the main character don't really have an alignment. Assign the milestone features like AlignmentCorruption2_Feature, though they might not work on other units.")]
    private static partial string m_UnitsOtherThanTheMainCharacterDoLocalizedText { get; }
    [LocalizedString("ToyBox_Features_PartyTab_Stats_UnitModifyAlignmentsFeature_m_AlignmentsLocalizedText", "Alignments")]
    private static partial string m_AlignmentsLocalizedText { get; }
}
