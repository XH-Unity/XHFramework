namespace XHFramework.Core {

/// <summary>
/// 场景加载进度事件参数
/// </summary>
public class SceneLoadProgressEventArgs : GlobalEventArgs
{
    /// <summary>
    /// 场景加载进度事件编号
    /// </summary>
    public static readonly int EventId = typeof(SceneLoadProgressEventArgs).GetHashCode();

    /// <summary>
    /// 获取场景加载进度事件编号
    /// </summary>
    public override int Id => EventId;

    /// <summary>
    /// 场景资源名称
    /// </summary>
    public string SceneAssetName { get; private set; }

    
    /// <summary>
    /// 加载进度 (0-1)
    /// </summary>
    public float Progress { get; private set; }

    /// <summary>
    /// 用户自定义数据
    /// </summary>
    public object UserData { get; private set; }

    public SceneLoadProgressEventArgs()
    {
    }

    /// <summary>
    /// 创建场景加载进度事件
    /// </summary>
    /// <param name="sceneAssetName">场景资源名称</param>
    /// <param name="progress">加载进度 (0-1)</param>
    /// <param name="userData">用户自定义数据</param>
    /// <returns>场景加载进度事件</returns>
    public static SceneLoadProgressEventArgs Create(string sceneAssetName, float progress, object userData = null)
    {
        SceneLoadProgressEventArgs eventArgs = ReferencePool.Acquire<SceneLoadProgressEventArgs>();
        eventArgs.SceneAssetName = sceneAssetName;
        eventArgs.Progress = progress;
        eventArgs.UserData = userData;
        return eventArgs;
    }

    /// <summary>
    /// 清理场景加载进度事件
    /// </summary>
    public override void Clear()
    {
        SceneAssetName = null;
        Progress = 0f;
        UserData = null;
    }
}

}
