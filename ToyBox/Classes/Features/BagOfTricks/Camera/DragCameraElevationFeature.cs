using Kingmaker.View;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.Camera;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Camera.DragCameraElevationFeature")]
public partial class DragCameraElevationFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableDragCameraElevation;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_DragCameraElevationFeature_Name", "Press CTRL while rotating camera to change height")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_DragCameraElevationFeature_Description", "CTRL + Mouse3 changes camera height in gameplay and free-camera cutscenes.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Camera.DragCameraElevationFeature";
        }
    }
    [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.CameraDragToRotate)), HarmonyPostfix]
    private static void CameraRig_CameraDragToRotate_Patch(CameraRig __instance, Vector2 __result) {
        if (Input.GetKey(KeyCode.LeftControl) && !__instance.MainCameraCinemachineBrain.isActiveAndEnabled) {
            __instance.m_TargetPosition.y += __result.y / 10f;
        }
    }
    [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.TryConsumeRotationInput)), HarmonyPostfix]
    private static void CameraRig_TryConsumeRotationInput_PostPatch(CameraRig __instance, ref bool __result, ref float yawDelta) {
        if (__result && Input.GetKey(KeyCode.LeftControl) && !__instance.MainCameraCinemachineBrain.isActiveAndEnabled) {
            yawDelta = 0;
            __result = false;
        }
    }
}
