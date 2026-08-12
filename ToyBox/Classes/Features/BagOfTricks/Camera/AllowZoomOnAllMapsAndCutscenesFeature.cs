using Kingmaker.View;

namespace ToyBox.Features.BagOfTricks.Camera;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Camera.AllowZoomOnAllMapsAndCutscenesFeature")]
public partial class AllowZoomOnAllMapsAndCutscenesFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableAllowZoomOnAllMapsAndCutscenes;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_AllowZoomOnAllMapsAndCutscenesFeature_Name", "Ignore camera zoom lock")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_AllowZoomOnAllMapsAndCutscenesFeature_Description", "Allows zoom in gameplay and free-camera cutscenes. Scripted camera shots are unaffected.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Camera.AllowZoomOnAllMapsAndCutscenesFeature";
        }
    }
    [HarmonyPatch(typeof(CameraZoom), nameof(CameraZoom.TickZoom)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CameraZoom_TickZoom_Patch(IEnumerable<CodeInstruction> instructions) {
        var field = AccessTools.Field(typeof(CameraZoom), nameof(CameraZoom.ZoomLock));
        var foundField = false;
        foreach (var instruction in instructions) {
            if (instruction.LoadsField(field)) {
                yield return CodeInstruction.Call((object obj) => ReplacementFalse(obj)).WithLabels(instruction.labels);
                foundField = true;
            } else {
                yield return instruction;
            }
        }
        ThrowIfTrue(!foundField);
    }
    private static bool ReplacementFalse(object _) {
        return false;
    }
}
