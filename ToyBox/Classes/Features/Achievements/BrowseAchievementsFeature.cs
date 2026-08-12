using Kingmaker.Achievements;
using Kingmaker.Blueprints.Root;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Features.Achievements;

public partial class BrowseAchievementsFeature : Feature {
    [LocalizedString("ToyBox_Features_Achievements_BrowseAchievementsFeature_Name", "Browse Achievements")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_Achievements_BrowseAchievementsFeature_Description", "Once a save is loaded, this allows looking through a list of achievements, supporting inspecting and unlocking them.")]
    public override partial string Description { get; }
    private Browser<AchievementData>? m_AchievementsBrowser;
    private static List<AchievementData> m_AllAchievements = [];
    private readonly HashSet<string> m_UnlockedAchievements = [];
    public override void OnGui() {
        if (!IsInGame()) {
            UI.Label(SharedStrings.ThisCannotBeUsedFromTheMainMenu.Red().Bold());
            return;
        }
        if (m_AchievementsBrowser == null) {
            m_AllAchievements = [.. ConfigRoot.Instance.Achievements.List.Where(AchievementsHelper.IsAvailable)];
            m_AchievementsBrowser = new(BPHelper.GetSortKey, BPHelper.GetSearchKey, null, func => func(m_AllAchievements), overridePageWidth: (int)(EffectiveWindowWidth() - (40 * Main.UIScale)));
        }
        var achievementEntities = AchievementsHelper.GetEntities(m_AllAchievements);
        var unlockedAchievements = m_AllAchievements.Where(achievement => achievementEntities.TryGetValue(achievement, out var entity) && entity.State.IsUnlocked).ToList();
        if (!m_AchievementsBrowser.ShowAll && !m_UnlockedAchievements.SetEquals(unlockedAchievements.Select(achievement => achievement.AssetGuid.ToString()))) {
            m_UnlockedAchievements.Clear();
            m_UnlockedAchievements.UnionWith(unlockedAchievements.Select(achievement => achievement.AssetGuid.ToString()));
            m_AchievementsBrowser.UpdateItems(unlockedAchievements);
        }
        m_AchievementsBrowser.OnGUI(ach => {
            BlueprintUI.BlueprintRowGUI(m_AchievementsBrowser, ach, null, null, (bp, unit) => achievementEntities.TryGetValue(bp, out var entity) ? entity : null);
        }, BlueprintUI.BlueprintHeaderGUI);
    }
}
