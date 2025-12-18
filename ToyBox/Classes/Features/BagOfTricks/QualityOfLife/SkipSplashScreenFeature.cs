using Kingmaker;
using Kingmaker.Code.UI.MVVM;
using System.Reflection.Emit;

namespace ToyBox.Features.BagOfTricks.QualityOfLife;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.QualityOfLife.SkipSplashScreenFeature")]
public partial class SkipSplashScreenFeature : FeatureWithPatch, INeedEarlyInitFeature {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableSkipSplashScreen;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_QualityOfLife_SkipSplashScreenFeature_Name", "Skip Splash Screen")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_QualityOfLife_SkipSplashScreenFeature_Description", "This skips the splash screen that appears when the game starts. Helpful if you need to frequently restart the game.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.QualityOfLife.SkipSplashScreenFeature";
        }
    }
    [HarmonyPatch(typeof(SplashScreenController), nameof(SplashScreenController.ShowSplashScreen)), HarmonyPrefix]
    private static bool Prefix(SplashScreenController __instance) {
        __instance.StartCoroutine(__instance.SkipWaitingSplashScreens());
        return false;
    }
    [HarmonyPatch(typeof(MainMenuLoadingScreen), nameof(MainMenuLoadingScreen.OnStart))]
    private static class MainMenuLoadingScreen_OnStart_Patch {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Start(IEnumerable<CodeInstruction> instructions) {
            foreach (var inst in instructions) {
                if (inst.LoadsConstant("Logo Show Requested")) {
                    yield return new(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call((string _, MainMenuLoadingScreen screen) => Helper(_, screen));
                    yield return new(OpCodes.Ret);
                    break;
                } else {
                    yield return inst;
                }
            }
        }
        private static void Helper(string _, MainMenuLoadingScreen screen) {
            screen.gameObject.SetActive(false);
            GameStarter.Instance.StartGame();
        }
    }
}
