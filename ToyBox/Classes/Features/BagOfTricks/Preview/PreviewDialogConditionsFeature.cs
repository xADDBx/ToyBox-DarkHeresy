using Kingmaker.Code.UI.MVVM;

namespace ToyBox.Features.BagOfTricks.Preview;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Preview.PreviewDialogConditionsFeature")]
public partial class PreviewDialogConditionsFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnablePreviewDialogConditions;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Preview_PreviewDialogConditionsFeature_Name", "Dialog Conditions")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Preview_PreviewDialogConditionsFeature_Description", "Shows conditions of answers in dialog.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Preview.PreviewDialogConditionsFeature";
        }
    }
    [HarmonyPatch(typeof(BookEventAnswerView), nameof(BookEventAnswerView.SetupAnswerText)), HarmonyPriority(Priority.HigherThanNormal), HarmonyPostfix]
    private static void GetAnswerFormattedString_Patch(BookEventAnswerView __instance) {
        try {
            if (!string.IsNullOrWhiteSpace(__instance.m_AnswerText.text)) {
                var conditions = DialogPreviewUtilities.FormatConditionsAsList(__instance.ViewModel.Answer) ?? [];
                var conditionsText = string.Join("", conditions.Select(s => "\n" + DialogPreviewUtilities.Indent + s));
                if (!string.IsNullOrWhiteSpace(conditionsText)) {
                    __instance.m_AnswerText.text += $"<size=65%>{conditionsText}</size>";
                }
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
    [HarmonyPatch(typeof(DialogAnswerBaseView), nameof(DialogAnswerBaseView.SetupAnswerText)), HarmonyPriority(Priority.HigherThanNormal), HarmonyPostfix]
    private static void GetAnswerFormattedString_Patch(DialogAnswerBaseView __instance) {
        try {
            if (!string.IsNullOrWhiteSpace(__instance.m_AnswerText.text)) {
                var conditions = DialogPreviewUtilities.FormatConditionsAsList(__instance.ViewModel.Answer) ?? [];
                var conditionsText = string.Join("", conditions.Select(s => "\n" + DialogPreviewUtilities.Indent + s));
                if (!string.IsNullOrWhiteSpace(conditionsText)) {
                    __instance.m_AnswerText.text += $"<size=65%>{conditionsText}</size>";
                }
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
}
