using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using ToyBox.Infrastructure.Keybinds;
using UnityEngine;

namespace ToyBox.Features.SettingsTab.Game;

[HarmonyPatch, ToyBoxPatchCategory("ToyBox.Features.SettingsTab.Game.DisplayGuidsInTooltipsFeature")]
public partial class DisplayGuidsInTooltipsFeature : FeatureWithPatch, IBindableFeature {
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableDisplayGuidsInTooltips;
        }
    }
    [LocalizedString("ToyBox_Features_SettingsTab_Game_DisplayGuidsInTooltipsFeature_Name", "Display GUIDs in most Tooltips")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsTab_Game_DisplayGuidsInTooltipsFeature_Description", "Displays the guids of the items etc. in their tooltips and allows copying them by pressing LMB + Hotkey.")]
    public override partial string Description { get; }
    public override void Initialize() {
        base.Initialize();
        Keybind = Hotkeys.MaybeGetHotkey(GetType());
    }
    public Hotkey? Keybind {
        get;
        set;
    }
    public override void OnGui() {
        using (HorizontalScope()) {
            base.OnGui();
            var current = Keybind;
            if (UI.HotkeyPicker(ref current, this, true)) {
                Keybind = current;
            }
        }
    }
    protected override string HarmonyName {
        get {
            return "ToyBox.Features.SettingsTab.Game.DisplayGuidsInTooltipsFeature";
        }
    }

    public void ExecuteAction(params object[] parameter) {
        throw new NotImplementedException();
    }

    public void LogExecution(params object[] parameter) {
        throw new NotImplementedException();
    }
    private static TooltipBrickText GetTooltip(string text) {
        return new TooltipBrickText(text.Grey().SizePercent(105), TooltipTextType.Simple | TooltipTextType.Italic);
    }
    private static void CopyToClipboard(string guid) {
        GUIUtility.systemCopyBuffer = guid;
        EventBus.RaiseEvent<IWarningNotificationUIHandler>(h => h.HandleWarning("Copied Guid to clipboard: " + guid, false));
    }
    [HarmonyPatch(typeof(TooltipTemplateAbility), nameof(TooltipTemplateAbility.GetBody)), HarmonyPostfix]
    private static void TooltipTemplateAbility_GetBody_Patch(TooltipTemplateAbility __instance, ref IEnumerable<ITooltipBrick> __result) {
        var guid = __instance.BlueprintAbility?.AssetGuid;
        if (guid != null) {
            __result = [GetTooltip($"guid: {guid}"), .. __result];
        }
    }
    [HarmonyPatch(typeof(TooltipTemplateItem), nameof(TooltipTemplateItem.GetBody)), HarmonyPostfix]
    private static void TooltipTemplateItem_GetBody_Patch(TooltipTemplateItem __instance, ref IEnumerable<ITooltipBrick> __result) {
        var guid = __instance.m_BlueprintItem?.AssetGuid ?? __instance.m_Item?.Blueprint?.AssetGuid;
        if (guid != null) {
            __result = [GetTooltip(guid), .. __result];
        }
    }
    [HarmonyPatch(typeof(TooltipTemplateBuff), nameof(TooltipTemplateBuff.GetBody)), HarmonyPostfix]
    public static void TooltipTemplateBuff_GetBody_Patch(TooltipTemplateBuff __instance, ref IEnumerable<ITooltipBrick> __result) {
        var guid = __instance.m_BlueprintBuff?.AssetGuid;
        if (guid != null) {
            __result = [GetTooltip(guid), .. __result];
        }
    }
    [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnMainClick)), HarmonyPrefix]
    private static bool LeftClickToolbar(ActionBarSlotVM __instance) {
        if (GetInstance<DisplayGuidsInTooltipsFeature>().Keybind?.IsActive() ?? false) {
            switch (__instance.MechanicActionBarSlot) {
                case MechanicActionBarSlotAbility ab:
                    CopyToClipboard(ab.Ability.Blueprint.AssetGuid);
                    return false;
                case MechanicActionBarSlotItem item:
                    CopyToClipboard(item.Item.Blueprint.AssetGuid);
                    return false;
                case MechanicActionBarSlotToggleAbility ab:
                    CopyToClipboard(ab.Ability.Blueprint.AssetGuid);
                    return false;
                case MechanicActionBarSlotSpontaneusConvertedSpell cspell:
                    CopyToClipboard(cspell.Spell.Blueprint.AssetGuid);
                    return false;
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(ItemSlotPCView), nameof(ItemSlotPCView.OnClick)), HarmonyPostfix]
    private static void LeftClickItem(ItemSlotPCView __instance) {
        if (GetInstance<DisplayGuidsInTooltipsFeature>().Keybind?.IsActive() ?? false) {
            var guid = __instance.ViewModel?.Item?.CurrentValue?.Blueprint?.AssetGuid;
            if (guid != null) {
                CopyToClipboard(guid);
            }
        }
    }
}
