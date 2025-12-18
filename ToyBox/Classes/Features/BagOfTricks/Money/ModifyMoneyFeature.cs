using Kingmaker;

namespace ToyBox.Features.BagOfTricks.Money;

public partial class ModifyMoneyFeature : Feature {
    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_Name", "Modify Money")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_Description", "Allows modifying how much Scints you have.")]
    public override partial string Description { get; }
    private int m_Gain = 100;
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(m_YouCanOnlyModifyYourMoneyAfterLoLocalizedText.Yellow());
            return;
        }
        using (HorizontalScope()) {
            UI.Label($"{m_CurrentMoneyLocalizedText}: {Game.Instance.Player.Money.ToString().Cyan()}   {m_GainLocalizedText}: ".Green());
            using (VerticalScope()) {
                using (HorizontalScope()) {
                    UI.LogSlider(ref m_Gain, 0, 10000000, 100, options: Width(200 * Main.UIScale));
                }
                using (HorizontalScope()) {
                    if (UI.Button(m_AddLocalizedText)) {
                        Game.Instance.Player.GainMoney(m_Gain);
                    }
                    if (UI.Button(m_RemoveLocalizedText)) {
                        Game.Instance.Player.SpendMoney(m_Gain);
                    }
                }
            }
        }
    }

    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_m_AddLocalizedText", "Add")]
    private static partial string m_AddLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_m_GainLocalizedText", "Gain")]
    private static partial string m_GainLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_m_RemoveLocalizedText", "Remove")]
    private static partial string m_RemoveLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_m_CurrentMoneyLocalizedText", "Current Scints")]
    private static partial string m_CurrentMoneyLocalizedText { get; }
    [LocalizedString("ToyBox_Features_BagOfTricks_Money_ModifyMoneyFeature_m_YouCanOnlyModifyYourMoneyAfterLoLocalizedText", "You can only modify your money after loading a save.")]
    private static partial string m_YouCanOnlyModifyYourMoneyAfterLoLocalizedText { get; }
}
