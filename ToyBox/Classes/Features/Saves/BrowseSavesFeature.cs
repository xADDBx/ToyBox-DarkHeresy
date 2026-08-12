using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence;

namespace ToyBox.Features.Saves;

public partial class BrowseSavesFeature : Feature {
    [LocalizedString("ToyBox_Features_Saves_BrowseSavesFeature_Name", "Browse Saves")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_Saves_BrowseSavesFeature_Description", "Allows looking through all available save games and viewing their char name, party level, area, save name and game id")]
    public override partial string Description { get; }
    public Browser<SaveInfo>? SaveBrowser;
    private DateTime m_NextSaveListUpdate;
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            return;
        }
        var saveList = Game.Instance.SaveManager.SaveList;
        if (DateTime.UtcNow >= m_NextSaveListUpdate) {
            m_NextSaveListUpdate = DateTime.UtcNow.AddMinutes(2);
            if (!saveList.UpdateInProgress) {
                _ = saveList.UpdateAsync();
            }
            SaveBrowser = null;
        }
        if (saveList.UpdateInProgress) {
            return;
        }
        if (SaveBrowser == null) {
            var saves = saveList.GetList().OfType<SaveInfo>().ToList();
            SaveBrowser = new(info => $"{info.Name}{info.Area?.AreaName?.Text}{info.Description}{info.ArchiveName}{info.GameId}",
                info => $"{info.PlayerCharacterName}, {info.GameSaveTime}",
                saves.Where(save => save.GameId == Game.Instance.Player.GameId),
                func => func(saves), overridePageWidth: (int)(EffectiveWindowWidth() * 0.9f));
            SaveBrowser.SetComparer(BlueprintFilter<SimpleBlueprint>.Sorter);
        }
        SaveBrowser.OnGUI(info => {
            using (HorizontalScope()) {
                UI.Label(info.GameId == Game.Instance.Player.GameId ? info.PlayerCharacterName.Orange() : info.PlayerCharacterName.Cyan(), Width(150 * Main.UIScale));
                Space(25);
                UI.Label($"{m_LevelLocalizedText}: {info.PlayerCharacterRank}", Width(75 * Main.UIScale));
                Space(25);
                UI.Label(info.Area?.AreaName?.Text ?? info.AreaNameOverride ?? "", Width(300 * Main.UIScale));
                Space(25);
                var tmp = info.GameId;
                UI.TextField(ref tmp, null, Width(250 * Main.UIScale));
                Space(25);
                UI.Label(info.Name.Green());
            }
        });
    }

    [LocalizedString("ToyBox_Features_Saves_BrowseSavesFeature_m_LevelLocalizedText", "Level")]
    private static partial string m_LevelLocalizedText { get; }
}
