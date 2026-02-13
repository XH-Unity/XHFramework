using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace XHFramework.Core {

/// <summary>
/// 界面管理器
/// </summary>
public class UIManager : ManagerBase
{
    public override int Priority => 60;
    #region 字段
    private GameObject _gameObject;
    /// <summary>
    /// 界面组的字典
    /// </summary>
    private readonly Dictionary<string, UIGroup> _uiGroups = new();

    /// <summary>
    /// 正在加载的界面的列表
    /// </summary>
    private readonly List<int> _uiFormsBeingLoaded = new();

    /// <summary>
    /// 正在加载的界面资源名称的列表
    /// </summary>
    private readonly List<string> _uiFormAssetNamesBeingLoaded = new();

    /// <summary>
    /// 要释放的界面的哈希集
    /// </summary>
    private readonly HashSet<int> _uiFormsToReleaseOnLoad = new();

    /// <summary>
    /// 等待回收的界面的队列
    /// </summary>
    private readonly LinkedList<UIForm> _recycleQueue = new();

    /// <summary>
    /// 界面实例对象池
    /// </summary>
    private ObjectPool<UIFormInstanceObject> _instancePool;

    /// <summary>
    /// 序列编号
    /// </summary>
    private int _serial;
    #endregion

    #region 生命周期

    public override void Init()
    {
        _instancePool = FW.ObjectPoolManager.CreateObjectPool<UIFormInstanceObject>(50, 60f);
        _instancePool.AutoReleaseInterval = 60f;
        _serial = 0;

        Log.Info("UIManager初始化");
        _gameObject = new GameObject("UIManager");
        _gameObject.transform.SetParent(FW.Root.transform);
        _gameObject.transform.localScale = Vector3.one;

        // 添加Canvas组件
        Canvas canvas = _gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        canvas.sortingOrder = 0;
        canvas.targetDisplay = 0; // Display 1 对应索引0
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
        canvas.vertexColorAlwaysGammaSpace = false;

        // 添加CanvasScaler组件
        UnityEngine.UI.CanvasScaler canvasScaler = _gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(720, 1280);
        canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0f;
        canvasScaler.referencePixelsPerUnit = 100f;

        // 添加GraphicRaycaster组件
        UnityEngine.UI.GraphicRaycaster graphicRaycaster = _gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    
    }

    /// <summary>
    /// 初始化 UI 组（由热更层调用）
    /// </summary>
    /// <param name="groupConfigs">UI 组配置列表</param>
    public void InitUIGroups(List<UIGroupConfig> groupConfigs)
    {
        if (groupConfigs == null || groupConfigs.Count == 0)
        {
            Log.Warn("UIGroupConfigs is null or empty");
            return;
        }

        for (int i = 0; i < groupConfigs.Count; i++)
        {
            if (!AddUIGroup(groupConfigs[i].Name, groupConfigs[i].Depth))
            {
                Log.Warn("Add UI group '{0}' failure.", groupConfigs[i].Name);
            }
        }

        Log.Info("UIManager UI组初始化完成，共 {0} 个组", groupConfigs.Count);
    }
    

    /// <summary>
    /// 界面管理器轮询
    /// </summary>
    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        //回收需要回收的界面
        while (_recycleQueue.Count > 0)
        {
            UIForm uiForm = _recycleQueue.First.Value;
            _recycleQueue.RemoveFirst();
            uiForm.Recycle();
            _instancePool.Unspawn(uiForm.gameObject);
        }

        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            uiGroup.Update(elapseSeconds, realElapseSeconds);
        }
    }

    /// <summary>
    /// 关闭并清理界面管理器
    /// </summary>
    public override void Shutdown()
    {
        CloseAllLoadedUIForms();
        _uiGroups.Clear();
        _uiFormsBeingLoaded.Clear();
        _uiFormAssetNamesBeingLoaded.Clear();
        _uiFormsToReleaseOnLoad.Clear();
        _recycleQueue.Clear();
    }

    #endregion


    #region 界面组相关的方法

    /// <summary>
    /// 是否存在界面组
    /// </summary>
    /// <param name="uiGroupName">界面组名称</param>
    /// <returns>是否存在界面组</returns>
    public bool HasUIGroup(string uiGroupName)
    {
        if (string.IsNullOrEmpty(uiGroupName))
        {
            Log.Error("要检查是存在的界面组名称为空");
            return false;
        }

        return _uiGroups.ContainsKey(uiGroupName);
    }

    #region 获取界面组

    /// <summary>
    /// 获取界面组
    /// </summary>
    /// <param name="uiGroupName">界面组名称</param>
    /// <returns>要获取的界面组</returns>
    public UIGroup GetUIGroup(string uiGroupName)
    {
        if (string.IsNullOrEmpty(uiGroupName))
        {
            Log.Error("要获取的界面组名称为空");
            return null;
        }

        UIGroup uiGroup = null;
        if (_uiGroups.TryGetValue(uiGroupName, out uiGroup))
        {
            return uiGroup;
        }

        return null;
    }

    /// <summary>
    /// 获取所有界面组
    /// </summary>
    /// <returns>所有界面组</returns>
    public UIGroup[] GetAllUIGroups()
    {
        int index = 0;
        UIGroup[] uiGroups = new UIGroup[_uiGroups.Count];
        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            uiGroups[index++] = uiGroup;
        }

        return uiGroups;
    }

    #endregion

    /// <summary>
    /// 增加界面组
    /// </summary>
    /// <param name="uiGroupName">界面组名称</param>
    /// <param name="uiGroupDepth">界面组深度</param>
    /// <returns>是否增加界面组成功</returns>
    public bool AddUIGroup(string uiGroupName, int uiGroupDepth = 0)
    {
        if (string.IsNullOrEmpty(uiGroupName))
        {
            Log.Error("要增加的界面组名称为空");
            return false;
        }

        if (HasUIGroup(uiGroupName))
        {
            Log.Error("要增加界面组已存在");
            return false;
        }

        //创建界面组
        GameObject groupGameObject = new GameObject();
        groupGameObject.name = string.Format("UI Group - {0}", uiGroupName);
        groupGameObject.layer = LayerMask.NameToLayer("UI");
        groupGameObject.transform.SetParent(_gameObject.transform);

        //将界面组放入字典
        _uiGroups.Add(uiGroupName, new UIGroup(uiGroupDepth, groupGameObject));
        return true;
    }

    #endregion

    #region 界面相关的方法

    #region 检查界面

    /// <summary>
    /// 是否存在界面
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <returns>是否存在界面</returns>
    public bool HasUIForm(int serialId)
    {
        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            if (uiGroup.HasUIForm(serialId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 是否存在界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <returns>是否存在界面</returns>
    public bool HasUIForm(string uiFormAssetName)
    {
        if (string.IsNullOrEmpty(uiFormAssetName))
        {
            Log.Error("要检查是否存在的界面的资源名称为空");
            return false;
        }

        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            if (uiGroup.HasUIForm(uiFormAssetName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 是否正在加载界面
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <returns>是否正在加载界面</returns>
    public bool IsLoadingUIForm(int serialId)
    {
        return _uiFormsBeingLoaded.Contains(serialId);
    }

    /// <summary>
    /// 是否正在加载界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <returns>是否正在加载界面</returns>
    public bool IsLoadingUIForm(string uiFormAssetName)
    {
        if (string.IsNullOrEmpty(uiFormAssetName))
        {
            Log.Error("要检查是否正在加载的界面的资源名称为空");
            return false;
        }

        return _uiFormAssetNamesBeingLoaded.Contains(uiFormAssetName);
    }

    /// <summary>
    /// 是否是合法的界面
    /// </summary>
    /// <param name="uiForm">界面</param>
    /// <returns>界面是否合法</returns>
    public bool IsValidUIForm(UIForm uiForm)
    {
        if (uiForm == null)
        {
            return false;
        }

        return HasUIForm(uiForm.SerialId);
    }

    #endregion

    #region 获取界面

    /// <summary>
    /// 获取界面
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <returns>要获取的界面</returns>
    public UIForm GetUIForm(int serialId)
    {
        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            UIForm uiForm = uiGroup.GetUIForm(serialId);
            if (uiForm != null)
            {
                return uiForm;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <returns>要获取的界面</returns>
    public UIForm GetUIForm<T>(string uiFormAssetName) where T:UIForm
    {
        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            UIForm uiForm = uiGroup.GetUIForm(uiFormAssetName);
            if (uiForm != null)
            {
                return uiForm;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <returns>要获取的界面</returns>
    public UIForm[] GetUIForms<T>(string uiFormAssetName) where T:UIForm
    {
        List<UIForm> uiForms = new List<UIForm>();
        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            uiForms.AddRange(uiGroup.GetUIForms(uiFormAssetName));
        }

        return uiForms.ToArray();
    }

    /// <summary>
    /// 获取所有已加载的界面。
    /// </summary>
    /// <returns>所有已加载的界面。</returns>
    public UIForm[] GetAllLoadedUIForms()
    {
        List<UIForm> uiForms = new List<UIForm>();
        foreach (UIGroup uiGroup in _uiGroups.Values)
        {
            uiForms.AddRange(uiGroup.GetAllUIForms());
        }

        return uiForms.ToArray();
    }

    /// <summary>
    /// 获取所有正在加载界面的序列编号。
    /// </summary>
    /// <returns>所有正在加载界面的序列编号。</returns>
    public int[] GetAllLoadingUIFormSerialIds()
    {
        return _uiFormsBeingLoaded.ToArray();
    }

    #endregion

    #region 打开界面

    /// <summary>
    /// 打开界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <param name="uiGroupName">界面组名称</param>
    /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面</param>
    /// <param name="userData">用户自定义数据</param>
    /// <returns>界面的序列编号</returns>
    public async UniTask<int?> OpenUIForm<T>(string uiFormAssetName,string uiGroupName,bool pauseCoveredUIForm,bool allowMultiInstance,uint priority,object userData = null) where T:UIForm ,new()
    {
        if (!allowMultiInstance)
        {
            if (IsLoadingUIForm(uiFormAssetName))
            {
                return null;
            }

            if (HasUIForm(uiFormAssetName))
            {
                return null;
            }
        }

        //界面组检查
        UIGroup uiGroup = GetUIGroup(uiGroupName);
        if (uiGroup == null)
        {
            Log.Error(string.Format("要打开的界面的界面组：{0} 不存在", uiGroupName));
        }

        //尝试从对象池获取界面实例
        int serialId = _serial++;
        UIFormInstanceObject uiFormInstanceObject = _instancePool.Spawn(uiFormAssetName);
        if (uiFormInstanceObject == null)
        {
            //没获取到就加载该界面
            _uiFormsBeingLoaded.Add(serialId);
            _uiFormAssetNamesBeingLoaded.Add(uiFormAssetName);

            GameObject instance =await FW.ResourceManager.LoadGameObjectAsync(uiFormAssetName,priority );
            //移除正在加载
            _uiFormsBeingLoaded.Remove(serialId);
            _uiFormAssetNamesBeingLoaded.Remove(uiFormAssetName);
            //加载结果
            if (instance)
            {
                if (_uiFormsToReleaseOnLoad.Contains(serialId))
                {
                    Log.Error(string.Format("需要释放的界面：{0} 加载成功", serialId));
                    _uiFormsToReleaseOnLoad.Remove(serialId);
                    FW.ResourceManager.ReleaseGameObject(instance);
                    return null;
                }

                //实例化界面，并将界面实例对象放入对象池
                uiFormInstanceObject =  UIFormInstanceObject.Create(instance, uiFormAssetName);
                _instancePool.Register(uiFormInstanceObject, true);

                //打开界面
                OpenUIFormInternal<T>(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject, pauseCoveredUIForm, true,
                    userData);
            }
            else
            {
                _uiFormsToReleaseOnLoad.Remove(serialId);
                Log.Error("打开界面：{0} 失败", uiFormAssetName);
                return null;
            }
        }
        else
        {
            OpenUIFormInternal<T>(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject, pauseCoveredUIForm, false,
                userData);
        }
        return serialId;
    }

    /// <summary>
    /// 打开界面内部方法
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <param name="uiGroup">界面组</param>
    /// <param name="uiFormInstanceObject">界面实例</param>
    /// <param name="pauseCoveredUIForm">界面是否暂停或遮挡</param>
    /// <param name="isNewInstance">界面是否是新实例</param>
    /// <param name="userData">用户自定义数据</param>
    private void OpenUIFormInternal<T>(int serialId,string uiFormAssetName,UIGroup uiGroup, UIFormInstanceObject uiFormInstanceObject,
        bool pauseCoveredUIForm, bool isNewInstance, object userData) where T : UIForm, new()
    {
        //获取ui界面Gamebject
        GameObject gameObject = uiFormInstanceObject.Target as GameObject;
        //初始化ui界面Gamebject
        Transform transform = gameObject.transform;
        transform.SetParent(uiGroup.GetGameObject().transform);
        transform.localScale = Vector3.one;
        //获取或创建ui界面脚本（使用Unity原生组件系统）
        UIForm uiForm = gameObject.GetOrAddComponent<T>();
        //初始化ui界面脚本
        uiForm.Init(serialId, uiFormAssetName, uiGroup, pauseCoveredUIForm, isNewInstance, userData);
        uiGroup.AddUIForm(uiForm);
        uiForm.Open(userData);
        uiGroup.Refresh();
    }

    #endregion

    #region 关闭界面
    /// <summary>
    /// 关闭界面。
    /// </summary>
    /// <param name="uiForm">要关闭的界面。</param>
    /// <param name="userData">用户自定义数据。</param>
    public void CloseUIForm(int serialId, object userData = null)
    {
        UIForm uiForm = GetUIForm(serialId);
        if (uiForm == null)
        {
            Log.Error("要关闭的界面为空");
            return;
        }

        UIGroup uiGroup = uiForm.UIGroup;
        if (uiGroup == null)
        {
            Log.Error("要关闭的界面的界面组为空");
            return;
        }
        uiGroup.RemoveUIForm(uiForm);
        uiForm.Close(userData);
        uiGroup.Refresh();
        _recycleQueue.AddLast(uiForm);
    }

    /// <summary>
    /// 关闭所有已加载的界面。
    /// </summary>
    /// <param name="userData">用户自定义数据。</param>
    public void CloseAllLoadedUIForms(object userData = null)
    {
        UIForm[] uiForms = GetAllLoadedUIForms();
        foreach (UIForm uiForm in uiForms)
        {
            CloseUIForm(uiForm.SerialId, userData);
        }
    }

    /// <summary>
    /// 关闭所有正在加载的界面。
    /// </summary>
    public void CloseAllLoadingUIForms()
    {
        foreach (int serialId in _uiFormsBeingLoaded)
        {
            _uiFormsToReleaseOnLoad.Add(serialId);
        }
    }

    #endregion

    /// <summary>
    /// 激活界面
    /// </summary>
    /// <param name="uiForm">要激活的界面</param>
    /// <param name="userData">用户自定义数据</param>
    public void RefocusUIForm(UIForm uiForm, object userData = null)
    {
        if (uiForm == null)
        {
            Log.Error("要激活的界面为空");
            return;
        }

        UIGroup uiGroup = uiForm.UIGroup;
        if (uiGroup == null)
        {
            Log.Error("要激活的界面的界面组为空");
            return;
        }

        uiGroup.RefocusUIForm(uiForm, userData);
        uiGroup.Refresh();
        uiForm.Refocus(userData);
    }

    #endregion
}

}