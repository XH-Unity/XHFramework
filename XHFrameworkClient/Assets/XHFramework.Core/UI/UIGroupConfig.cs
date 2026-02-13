namespace XHFramework.Core {

/// <summary>
/// UI 组配置（AOT 层定义，热更层填充数据）
/// </summary>
public class UIGroupConfig
{
    /// <summary>
    /// UI 组名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// UI 组深度
    /// </summary>
    public int Depth { get; private set; }

    public UIGroupConfig(string name, int depth)
    {
        Name = name;
        Depth = depth;
    }
}

}
