using Kingmaker.View;

namespace ToyBox.Features.BagOfTricks.Camera;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Camera.AllowRotateOnAllMapsAndCutscenesFeature")]
public partial class AllowRotateOnAllMapsAndCutscenesFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableAllowRotateOnAllMapsAndCutscenes;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_AllowRotateOnAllMapsAndCutscenesFeature_Name", "Ignore manual camera rotation lock")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_AllowRotateOnAllMapsAndCutscenesFeature_Description", "Allows rotation in gameplay and free-camera cutscenes. Scripted camera shots are unaffected.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Camera.AllowRotateOnAllMapsAndCutscenesFeature";
        }
    }
    [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.TickRotate)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CameraRig_TickRotate_Patch(IEnumerable<CodeInstruction> instructions) {
        var field = AccessTools.Field(typeof(CameraRig), nameof(CameraRig.m_HandRotationLock));
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
