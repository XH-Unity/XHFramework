namespace XHFramework.Core {

/// <summary>
/// UI 窗口配置（AOT 层定义，热更层填充数据）
/// </summary>
public class UIFormConfig
{
    /// <summary>
    /// 资源名称
    /// </summary>
    public string AssetName { get; private set; }

    /// <summary>
    /// 所属 UI 组名称
    /// </summary>
    public string UIGroupName { get; private set; }

    /// <summary>
    /// 是否允许多实例
    /// </summary>
    public bool AllowMultiInstance { get; private set; }

    /// <summary>
    /// 是否暂停被覆盖的界面
    /// </summary>
    public bool PauseCoveredUIForm { get; private set; }

    public UIFormConfig(string assetName, string uiGroupName, bool allowMultiInstance, bool pauseCoveredUIForm)
    {
        AssetName = assetName;
        UIGroupName = uiGroupName;
        AllowMultiInstance = allowMultiInstance;
        PauseCoveredUIForm = pauseCoveredUIForm;
    }
}

}
