using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;
using RevertSynchronize.Patches;

namespace RevertSynchronize;

/// <summary>
/// 替换功能总开关：
/// 1) 形状守卫通过（类型与补丁目标成员存在、ModelDb 中确有「同步」）；
/// 2) 当前游戏版本不是 v0.110.x —— 该版本卡牌本就是目标效果，不干预。
/// min_game_version=0.107.0 由加载器原生保证（低于 0.107.0 不加载）。
/// </summary>
public static class ModConfig
{
    public static bool IsReplaceEnabled =>
        SynchronizeShapeGuard.IsSupported() && IsActiveVersion();

    private static bool? _versionOk;

    /// <summary>
    /// 版本判断（懒求值 + 缓存）：SemVer 未就绪时暂按"非 110"放行，下次访问再评估；
    /// 一旦拿到确定的 SemVer 即缓存结论。110 判断口径：Major=0 且 Minor=110（含 0.110.x 全部补丁号）。
    /// </summary>
    public static bool IsActiveVersion()
    {
        if (_versionOk.HasValue)
        {
            return _versionOk.Value;
        }

        try
        {
            SemanticVersion? semVer = ReleaseInfoManager.Instance.SemVer;
            if (semVer != null)
            {
                _versionOk = !(semVer.Major == 0 && semVer.Minor == 110);
                if (_versionOk.Value)
                {
                    Log.Info($"{ModEntry.ModId}: current version {semVer} is supported; patches enabled.");
                }
                else
                {
                    Log.Info($"{ModEntry.ModId}: current version {semVer} (v0.110.x) already has the target card effect; patches disabled.");
                }
            }
        }
        catch (Exception e)
        {
            Log.Warn($"{ModEntry.ModId}: failed to read release version ({e.Message}); treating as supported, will retry.");
        }

        return _versionOk ?? true;
    }
}
