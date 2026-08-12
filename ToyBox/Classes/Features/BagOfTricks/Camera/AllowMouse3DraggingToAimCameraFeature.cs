using Kingmaker.View;
using ToyBox.Classes.Infrastructure.Features;
using ToyBox.Infrastructure.Keybinds;
using ToyBox.Infrastructure.Utilities;
using UnityEngine;

namespace ToyBox.Features.BagOfTricks.Camera;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Camera.AllowMouse3DraggingToAimCameraFeature")]
public partial class AllowMouse3DraggingToAimCameraFeature : FeatureWithPatch, IToggledWithBinding {
    private static float m_PitchDelta;
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableMouse3DraggingToAimCamera;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_AllowMouse3DraggingToAimCameraFeature_Name", "Enable Mouse3 Dragging To Aim The Camera")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Camera_AllowMouse3DraggingToAimCameraFeature_Description", "Allows vertical camera rotation with Mouse3 in gameplay and free-camera cutscenes.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Camera.AllowMouse3DraggingToAimCameraFeature";
        }
    }
    public override void Enable() {
        base.Enable();
        Keybind = Hotkeys.MaybeGetHotkey(GetType());
    }
    public Hotkey? Keybind {
        get;
        set;
    }
    public void ExecuteAction(ActionParameter parameter) {
        LogExecution(parameter);
        IsEnabled = !IsEnabled;
        if (IsEnabled) {
            Enable();
        } else {
            Disable();
        }
    }
    public void LogExecution(ActionParameter parameter) {
        Helpers.LogExecution(this, parameter);
    }
    [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.TickRotate)), HarmonyPrefix]
    private static void CameraRig_TickRotate_PrePatch() {
        m_PitchDelta = 0;
    }
    [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.CameraDragToRotate)), HarmonyPostfix]
    private static void CameraRig_CameraDragToRotate_Patch(Vector2 __result) {
        if (!Input.GetKey(KeyCode.LeftControl) || !Settings.EnableDragCameraElevation) {
            m_PitchDelta = __result.y;
        }
    }
    [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.TickRotate)), HarmonyPostfix]
    private static void CameraRig_TickRotate_PostPatch(CameraRig __instance) {
        if (m_PitchDelta == 0 || __instance.MainCameraCinemachineBrain.isActiveAndEnabled) {
            return;
        }
        var target = __instance.m_TargetRotate;
        target.x += m_PitchDelta * __instance.RotationSpeed;
        target.x = Mathf.Clamp(target.x <= 180 ? target.x : -(360 - target.x), -63, 100);
        target.z = 0;
        __instance.m_TargetRotate = target;
    }
}
