using Kingmaker.Achievements;
using Owlcat.Runtime.Platforms.Core.Common;
using Owlcat.Runtime.Platforms.Core.Common.Achievements;
using Owlcat.Runtime.Platforms.Core.Common.Achievements.Settings;

namespace ToyBox.Infrastructure.Utilities;

internal static class AchievementsHelper {
    public static bool IsAvailable(AchievementData achievement) {
        if (!PlatformCore.IsCreated || !PlatformCore.Features.HasAchievementsSupport) {
            return false;
        }
        var data = ((IAchievementData)achievement).GetEffective(PlatformCore.Platform);
        return !data.Ignore && !string.IsNullOrWhiteSpace(data.Id);
    }

    public static bool CanUnlock(AchievementData achievement) {
        if (!TryGetServiceAndData(achievement, out var service, out var data)) {
            return false;
        }
        if (service.GetPendingSnapshot().ContainsKey(data)) {
            return false;
        }
        return service.GetSyncedSnapshot().Any(entity => entity.Data.Equals(data) && !entity.State.IsUnlocked);
    }

    public static Dictionary<AchievementData, AchievementEntity> GetEntities(IEnumerable<AchievementData> achievements) {
        var result = new Dictionary<AchievementData, AchievementEntity>();
        if (!PlatformCore.IsCreated || PlatformCore.AchievementService is not AchievementsServiceBase service) {
            return result;
        }
        var entitiesByData = service.GetSyncedSnapshot().ToDictionary(entity => entity.Data);
        foreach (var pending in service.GetPendingSnapshot()) {
            entitiesByData[pending.Key] = new() { Data = pending.Key, State = pending.Value };
        }
        foreach (var achievement in achievements) {
            var data = ((IAchievementData)achievement).GetEffective(PlatformCore.Platform);
            if (entitiesByData.TryGetValue(data, out var entity)) {
                result[achievement] = entity;
            }
        }
        return result;
    }

    private static bool TryGetServiceAndData(AchievementData achievement, out AchievementsServiceBase service, out EffectiveAchievementData data) {
        data = default;
        service = null!;
        if (!PlatformCore.IsCreated || PlatformCore.AchievementService is not AchievementsServiceBase achievementService) {
            return false;
        }
        service = achievementService;
        data = ((IAchievementData)achievement).GetEffective(PlatformCore.Platform);
        return !data.Ignore && !string.IsNullOrWhiteSpace(data.Id);
    }
}
