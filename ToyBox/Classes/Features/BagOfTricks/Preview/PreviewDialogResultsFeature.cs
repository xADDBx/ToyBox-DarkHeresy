using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.DialogSystem.Blueprints;

namespace ToyBox.Features.BagOfTricks.Preview;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Preview.PreviewDialogResultsFeature")]
public partial class PreviewDialogResultsFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnablePreviewDialogResults;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Preview_PreviewDialogResultsFeature_Name", "Dialog Results")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Preview_PreviewDialogResultsFeature_Description", "Shows results of cues and answers in dialog.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Preview.PreviewDialogResultsFeature";
        }
    }
    [HarmonyPatch(typeof(CueVM), nameof(CueVM.GetMechanicText)), HarmonyPostfix]
    private static void GetCueText_Patch(ref string __result) {
        try {
            var cue = Game.Instance.Controllers.DialogController.CurrentCue;
            if (cue != null) {
                __result += DialogPreviewUtilities.GetCueResultText(cue);
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
    [HarmonyPatch(typeof(BookEventAnswerView), nameof(BookEventAnswerView.SetupAnswerText)), HarmonyPriority(Priority.HigherThanNormal), HarmonyPostfix]
    private static void GetAnswerFormattedString_Patch(BookEventAnswerView __instance) {
        try {
            if (!string.IsNullOrWhiteSpace(__instance.m_AnswerText.text)) {
                __instance.m_AnswerText.text += DialogPreviewUtilities.GetAnswerResultText(__instance.ViewModel.Answer);
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
    [HarmonyPatch(typeof(DialogAnswerBaseView), nameof(DialogAnswerBaseView.SetupAnswerText)), HarmonyPriority(Priority.HigherThanNormal), HarmonyPostfix]
    private static void GetAnswerFormattedString_Patch(DialogAnswerBaseView __instance) {
        try {
            if (!string.IsNullOrWhiteSpace(__instance.m_AnswerText.text)) {
                __instance.m_AnswerText.text += DialogPreviewUtilities.GetAnswerResultText(__instance.ViewModel.Answer);
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
}
