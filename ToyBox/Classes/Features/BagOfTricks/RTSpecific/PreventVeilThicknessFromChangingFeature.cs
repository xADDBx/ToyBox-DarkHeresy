using Kingmaker;
using Kingmaker.Code.AreaLogic;

namespace ToyBox.Features.BagOfTricks.RTSpecific;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.RTSpecific.PreventVeilThicknessFromChangingFeature")]
public partial class PreventVeilThicknessFromChangingFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnablePreventVeilThicknessFromChanging;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_PreventVeilThicknessFromChangingFeature_Name", "Prevent Veil Thickness from Changing")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_RTSpecific_PreventVeilThicknessFromChangingFeature_Description", "Forces the current veil thickness to 0.")]
    public override partial string Description { get; }
    public override void Initialize() {
        base.Initialize();
        if (IsInGame()) {
            Game.Instance.LoadedArea?.Veil?.Damage = 0;
        }
    }
    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.RTSpecific.PreventVeilThicknessFromChangingFeature";
        }
    }
    [HarmonyPatch(typeof(PartVeil), nameof(PartVeil.Damage), MethodType.Setter), HarmonyPrefix]
    private static void VeilThicknessCounter_setValue_Patch(ref int value) {
        value = 0;
    }
}
