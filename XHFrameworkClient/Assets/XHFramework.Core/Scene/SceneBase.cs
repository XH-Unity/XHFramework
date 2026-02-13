using Cysharp.Threading.Tasks;

namespace XHFramework.Core {

public abstract class SceneBase : IReference
{
    public SceneBase()
    {
    }

    public abstract void OnInit(int sceneID);

    /// <summary>
    /// 打开加载界面
    /// </summary>
    public abstract UniTask OpenLoading();

    /// <summary>
    /// 关闭加载界面
    /// </summary>
    public abstract void CloseLoading();

    /// <summary>
    /// 预加载资源
    /// </summary>
    public virtual UniTask OnPreload()
    {
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 进入场景
    /// </summary>
    public abstract void OnEnter();

    /// <summary>
    /// 场景更新
    /// </summary>
    public abstract void OnUpdate(float elapseSeconds, float realElapseSeconds);

    /// <summary>
    /// 离开场景
    /// </summary>
    public abstract void OnLeave();

    public void Clear()
    {
    }
}

}
