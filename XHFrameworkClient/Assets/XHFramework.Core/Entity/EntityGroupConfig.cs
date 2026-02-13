namespace XHFramework.Core {

/// <summary>
/// 实体组配置（AOT 层定义，热更层填充数据）
/// </summary>
public class EntityGroupConfig
{
    /// <summary>
    /// 实体组名称
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 实例自动释放间隔（秒）
    /// </summary>
    public readonly float InstanceAutoReleaseInterval;

    /// <summary>
    /// 实例容量
    /// </summary>
    public readonly int InstanceCapacity;

    /// <summary>
    /// 实例过期时间（秒）
    /// </summary>
    public readonly float InstanceExpireTime;

    public EntityGroupConfig(string name, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime)
    {
        Name = name;
        InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
        InstanceCapacity = instanceCapacity;
        InstanceExpireTime = instanceExpireTime;
    }
}

}
