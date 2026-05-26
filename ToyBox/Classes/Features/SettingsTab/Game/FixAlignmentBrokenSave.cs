using Kingmaker.Blueprints;
using Kingmaker.Networking;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using OwlPack.Runtime;
using System.Reflection;

namespace ToyBox.SettingsTab.Game;
#warning BETA FEATURE
[HarmonyPatch, ToyBoxPatchCategory("ToyBox.SettingsTab.Game.FixAlignmentBrokenSave")]
public partial class FixAlignmentBrokenSave : FeatureWithPatch {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableFixAlignmentBrokenSave;
        }
    }
    [LocalizedString("ToyBox_SettingsTab_Game_FixAlignmentBrokenSave_Name", "Fix save that broke because of alignment")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_SettingsTab_Game_FixAlignmentBrokenSave_Description", "Changing alignment via ToyBox in a save with a version < 0.1.11 made that save unable to be loaded. This feature allows loading that save. This feature automatically disables itself once save loading finished.")]
    public override partial string Description { get; }
    [HarmonyPatch]
    private static class TargetPatch {

        [HarmonyTargetMethod]
        private static MethodInfo GetMethod() {
            return AccessTools.Method(typeof(AlignmentShiftHistoryEntry), nameof(AlignmentShiftHistoryEntry.Deserialize)).MakeGenericMethod(typeof(BinaryInputFormatter));
        }
        [HarmonyPrefix]
        private static bool AlignmentShiftHistoryEntry_Deserialize_Patch(AlignmentShiftHistoryEntry __instance, BinaryInputFormatter formatter, uint objectId, DeserializerState state) {
            state.References.Register(objectId, __instance);
            var typeInfo = state.TypeLibrary.GetTypeInfo<AlignmentShiftHistoryEntry>();
            var mappingForType = state.GetMappingForType(AlignmentShiftHistoryEntry.OwlPackTypeInfo, typeInfo);
            formatter.EnterObject();
            for (var i = 0; i < typeInfo.Fields.Length; i++) {
                formatter.ReadFieldHeader(typeInfo, out var b, out var num);
                var b2 = mappingForType[b];
                switch (b2) {
                    case 0:
                        __instance.Axis = formatter.ReadEnum<AlignmentAxis>(state);
                        break;
                    case 1:
                        try {
                            __instance.Source = formatter.ReadPackable<BlueprintScriptableObject>(state);
                        } catch (Exception ex) {
                            Warn($"Skipped AlignmentShiftHistoryEntry Source while deserializing:\n{ex}");
                            __instance.Source = null;
                        }
                        break;
                    case 2:
                        __instance.Rank = formatter.ReadUnmanaged<int>(state);
                        break;
                    case 3:
                        __instance.AchievedNewMark = formatter.ReadUnmanaged<bool>(state);
                        break;
                    case 4:
                        __instance.NewFacts = formatter.ReadPackable<List<BlueprintMechanicEntityFact>>(state);
                        break;
                    default:
                        if (b2 == 255) {
                            formatter.SkipField(num);
                        }
                        break;
                }
            }
            formatter.LeaveObject();
            return false;
        }
    }
    [HarmonyPatch(typeof(SaveNetManager), nameof(SaveNetManager.PostLoad)), HarmonyPostfix]
    private static void SaveNetManager_PostLoad_Patch() {
        var feature = GetInstance<FixAlignmentBrokenSave>();
        feature.IsEnabled = false;
        feature.Disable();
    }
    protected override string HarmonyName {
        get {
            return "ToyBox.SettingsTab.Game.FixAlignmentBrokenSave";
        }
    }
}
