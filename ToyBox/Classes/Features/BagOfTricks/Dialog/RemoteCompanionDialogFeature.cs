using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using static Owlcat.Runtime.Visual.GPUDrivenBRG.Batching.GPUDrivenNativeDataUpdate;

namespace ToyBox.Features.BagOfTricks.Dialog;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.BagOfTricks.Dialog.RemoteCompanionDialogFeature")]
public partial class RemoteCompanionDialogFeature : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableRemoteCompanionDialog;
        }
    }
    [LocalizedString("ToyBox_Features_BagOfTricks_Dialog_RemoteCompanionDialogFeature_Name", "Expand Dialog To Include Remote Companions")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Dialog_RemoteCompanionDialogFeature_Description", "Allow remote companions to make comments on dialog you are having.")]
    public override partial string Description { get; }

    protected override string HarmonyName {
        get {
            return "ToyBox.Features.BagOfTricks.Dialog.RemoteCompanionDialogFeature";
        }
    }
    [HarmonyPatch(typeof(IsCompanionInParty), nameof(IsCompanionInParty.CheckCondition)), HarmonyPostfix]
    private static void CompanionInParty_CheckCondition_Patch(IsCompanionInParty __instance, ref bool __result) {
        // No need to override if the result is already true
        if (__result) {
            return;
        }
        // We only want this patch to run for conditions requiring the character to be in the party so if it is for the inverse we bail.
        // Example of this comes up with Lann and Wenduag in the final scene of the Prologue Labyrinth
        if (__instance.Not) {
            return;
        }

        // Condition has no Owner. Currently only possibly if manually triggered in DetectiveSystem_IsCompanionAvailableForStudy_Patch
        if (__instance.Owner == null) {
            return;
        }

        // We don't want to match when the game only checks for Ex companions since this is basically a check for companions which left the party then
        // Example is 6aeb6812dcc1464a9b087786556c9b18 which checks whether Pascal left as a companion. Really weird design from Owlcat right there.
        if (__instance.MatchWhenEx && !__instance.MatchWhenActive && !__instance.MatchWhenDetached && !__instance.MatchWhenRemote) {
            return;
        }
        try {
            var maybeCompanion = Game.Instance.Player.AllCrossSceneUnits.FirstOrDefault(u => u.Blueprint == __instance.Companion && !u.IsDisposed && !u.IsDisposingNow)?.GetOptional<UnitPartCompanion>();
            if (maybeCompanion != null) {
                if (maybeCompanion.State != CompanionState.None) {
                    if (maybeCompanion.State != CompanionState.ExCompanion || GetInstance<ExCompanionDialogFeature>().IsEnabled) {
                        if (__instance.Owner is BlueprintCue cueBp) {
                            Debug($"Overiding {cueBp.name} Companion {__instance.Companion.name} ({__instance.Companion.AssetGuid}) In Party to true");
                            __result = true;
                        } else if (__instance.Owner is BlueprintAnswer answeBp) {
                            Debug($"Overiding {answeBp.name} Companion {__instance.Companion.name} ({__instance.Companion.AssetGuid}) In Party to true");
                            __result = true;
                        } else if (__instance.Owner is BlueprintClueStudy clueBp) {
                            Debug($"Overiding {clueBp.name} Companion {__instance.Companion.name} ({__instance.Companion.AssetGuid}) In Party to true");
                            __result = true;
                        } else {
                            Log($"Encountered IsCompanionInParty with unhandled owner type: {__instance.Owner.AssetGuid}");
                        }
                    }
                }
            } else {
                Log($"Could not override check {__instance.name} on {__instance.Owner?.AssetGuid ?? "Null BP Owner?"} because no unit with blueprint {__instance.Companion?.AssetGuid ?? "Null Companion BP?"} was found.");
            }
        } catch (Exception ex) {
            Error(ex);
        }
    }
    [ThreadStatic]
    private static bool m_OriginallyIncludedEx;
    [ThreadStatic]
    private static bool m_OriginallyIncludedRemote;
    [HarmonyPatch(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty), nameof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty.GetAbstractUnitEntityInternal)), HarmonyPrefix]
    private static void CompanionInParty_GetAbstractUnitEntityInternal_Pre_Patch(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty __instance) {
        if (__instance.Owner is BlueprintCue or BlueprintAnswer) {
            m_OriginallyIncludedEx = __instance.IncludeExCompanions;
            m_OriginallyIncludedRemote = __instance.IncludeRemote;
            __instance.IncludeExCompanions = GetInstance<ExCompanionDialogFeature>().IsEnabled;
            __instance.IncludeRemote = true;
            Debug($"Evalutors checking {__instance} Guid:{__instance.AssetGuid} Owner:{__instance.Owner.name} OwnerGuid: {__instance.Owner.AssetGuid}); Allowed ex: {m_OriginallyIncludedEx}, now: {__instance.IncludeExCompanions}; Allowed remote: {m_OriginallyIncludedRemote}, now: true");
        }
    }
    [HarmonyPatch(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty), nameof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty.GetAbstractUnitEntityInternal)), HarmonyPostfix]
    private static void CompanionInParty_GetAbstractUnitEntityInternal_Post_Patch(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty __instance) {
        if (__instance.Owner is BlueprintCue or BlueprintAnswer) {
            __instance.IncludeExCompanions = m_OriginallyIncludedEx;
            __instance.IncludeRemote = m_OriginallyIncludedRemote;
        }
    }
    [HarmonyPatch(typeof(DialogSpeaker), nameof(DialogSpeaker.TryGetSpeakerEntity)), HarmonyPostfix]
    private static void DialogSpeaker_GetEntity_Patch(DialogSpeaker __instance, ref BaseUnitEntity speaker, ref bool __result) {
        if (!__result && __instance.Blueprint != null) {
            var units = Game.Instance.Controllers.EntitySpawner.CreationQueue.Select((EntitySpawnController.SpawnEntry ce) => ce.Entity).OfType<BaseUnitEntity>();
            var maybeUnit = Game.Instance.Player.AllCrossSceneUnits.Where(u => GetInstance<ExCompanionDialogFeature>().IsEnabled || u.GetCompanionOptional()?.State != CompanionState.ExCompanion)
                .Concat(units).Select(__instance.SelectMatchingUnit).NotNull().Distinct().Nearest(Game.Instance.Controllers.DialogController.DialogPosition);
            if (maybeUnit != null) {
                __instance.ReplacedSpeakerWithErrorSpeaker = false;
                speaker = maybeUnit;
                __result = true;
                return;
            }
        }
    }
    [HarmonyPatch(typeof(DetectiveSystem), nameof(DetectiveSystem.IsCompanionAvailableForStudy)), HarmonyPostfix]
    private static void DetectiveSystem_IsCompanionAvailableForStudy_Patch(ref bool __result, BlueprintUnit? blueprint) {
        // if blueprint == null => result == true; but I'll check anyways for future proofing
        if (!__result && blueprint != null) {
            var n = new IsCompanionInParty() {
                Companion = blueprint,
                MatchWhenActive = true,
                MatchWhenRemote = true,
                MatchWhenEx = GetInstance<ExCompanionDialogFeature>().IsEnabled
            }.CheckCondition();
            if (n) {
                Debug($"Overiding DetectiveSystem.IsCompanionAvailableForStudy call for {blueprint.name} ({blueprint.AssetGuid}) In Party to true");
                __result = true;
            }
        }
    }
}
