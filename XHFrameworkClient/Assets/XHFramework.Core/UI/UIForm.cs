using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XHFramework.Core {

/// <summary>
/// UGUI界面基类
/// 继承 MonoBehaviour，支持热更新（HybridCLR + YooAsset）
/// </summary>
public abstract class UIForm : MonoBehaviour
{

    private const int DepthFactor = 100;

    /// <summary>
    /// 缓存的 Canvas
    /// </summary>
    private Canvas _cachedCanvas = null;

    /// <summary>
    /// Canvas组
    /// </summary>
    private CanvasGroup _canvasGroup = null;

    #region 界面基础属性

    /// <summary>
    /// 界面序列编号
    /// </summary>
    public int SerialId { get; private set; }

    /// <summary>
    /// 界面资源名称
    /// </summary>
    public string UIFormAssetName { get; private set; }


    /// <summary>
    /// 界面所属的界面组
    /// </summary>
    public UIGroup UIGroup { get; private set; }

    /// <summary>
    /// 获取界面在界面组中的深度
    /// </summary>
    public int DepthInUIGroup { get; private set; }

    /// <summary>
    /// 是否暂停被覆盖的界面
    /// </summary>
    public bool PauseCoveredUIForm { get; private set; }

    /// <summary>
    /// 界面是否暂停
    /// </summary>
    public bool Paused { get; set; }

    /// <summary>
    /// 界面是否遮挡
    /// </summary>
    public bool Covered { get; set; }


    /// <summary>
    /// 原始深度
    /// </summary>
    private int _originalDepth;

    /// <summary>
    /// 界面深度
    /// </summary>
    private int Depth => _cachedCanvas.sortingOrder;

    #endregion
    
    #region 界面生命周期管理

    /// <summary>
    /// 初始化界面
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <param name="uiGroup">界面所处的界面组</param>
    /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面</param>
    /// <param name="isNewInstance">是否是新实例</param>
    /// <param name="userData">用户自定义数据</param>
    public void Init(int serialId, string uiFormAssetName, UIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance,
        object userData)
    {
        Paused = true;
        Covered = true;
        //界面每次打开时都刷新一次序列编号
        SerialId = serialId;
        UIFormAssetName = uiFormAssetName;

        if (isNewInstance)
        {
            UIGroup = uiGroup;
        }
        else if (UIGroup != uiGroup)
        {
            Log.Error("非新实例对象的界面初始化时界面组不一致");
            return;
        }

        DepthInUIGroup = 0;
        PauseCoveredUIForm = pauseCoveredUIForm;

        if (!isNewInstance)
        {
            return;
        }

        _cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
        _cachedCanvas.overrideSorting = true;
        _originalDepth = _cachedCanvas.sortingOrder;

        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        gameObject.GetOrAddComponent<GraphicRaycaster>();

        //自动绑定
        BindInit();
        // 调用子类的初始化逻辑
        OnInit(userData);
    }

    protected virtual void BindInit()
    {
    }

    /// <summary>
    /// 界面回收
    /// </summary>
    public void Recycle()
    {
        SerialId = 0;
        DepthInUIGroup = 0;
        PauseCoveredUIForm = true;
    }

    /// <summary>
    /// 界面打开
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    public void Open(object userData)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        OnOpen(userData);
    }

    /// <summary>
    /// 界面关闭
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    public void Close(object userData)
    {
        gameObject.SetActive(false);
        OnClose(userData);
    }

    /// <summary>
    /// 界面暂停
    /// </summary>
    public void Pause()
    {
        gameObject.SetActive(false);
        OnPause();
    }

    /// <summary>
    /// 界面暂停恢复
    /// </summary>
    public void Resume()
    {
        gameObject.SetActive(true);
        OnResume();
    }

    /// <summary>
    /// 界面遮挡
    /// </summary>
    public void Cover()
    {
        OnCover();
    }

    /// <summary>
    /// 界面遮挡恢复
    /// </summary>
    public void Reveal()
    {
        OnReveal();
    }

    /// <summary>
    /// 界面激活
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    public void Refocus(object userData)
    {
        OnRefocus(userData);
    }

    /// <summary>
    /// 界面轮询
    /// </summary>
    public void UpdateForm(float elapseSeconds, float realElapseSeconds)
    {
        OnUpdate(elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    public void Close()
    {
        FW.UIManager.CloseUIForm(SerialId);
    }

    /// <summary>
    /// 清理界面数据（界面回收时调用）
    /// </summary>
    public void Clear()
    {
        _cachedCanvas = null;
        _canvasGroup = null;
        UIFormAssetName = null;
        UIGroup = null;
    }

    /// <summary>
    /// 界面深度改变
    /// </summary>
    /// <param name="uiGroupDepth">界面组深度</param>
    /// <param name="depthInUIGroup">界面在界面组中的深度</param>
    public void DepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
        DepthInUIGroup = depthInUIGroup;

        int oldDepth = Depth;
        int deltaDepth = UIGroup.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth + _originalDepth;

        Canvas[] canvases = gameObject.GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].sortingOrder += deltaDepth;
        }

        OnDepthChanged(uiGroupDepth, depthInUIGroup);
    }

    #endregion

    #region 子类可重写的逻辑方法

    /// <summary>
    /// 界面初始化逻辑(子类可重写)
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnInit(object userData)
    {
    }

    /// <summary>
    /// 界面打开逻辑(子类可重写)
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnOpen(object userData)
    {
    }

    /// <summary>
    /// 界面关闭逻辑(子类可重写)
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnClose(object userData)
    {
    }

    /// <summary>
    /// 界面暂停逻辑(子类可重写)
    /// </summary>
    protected virtual void OnPause()
    {
    }

    /// <summary>
    /// 界面暂停恢复逻辑(子类可重写)
    /// </summary>
    protected virtual void OnResume()
    {
    }

    /// <summary>
    /// 界面遮挡逻辑(子类可重写)
    /// </summary>
    protected virtual void OnCover()
    {
    }

    /// <summary>
    /// 界面遮挡恢复逻辑(子类可重写)
    /// </summary>
    protected virtual void OnReveal()
    {
    }

    /// <summary>
    /// 界面激活逻辑(子类可重写)
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnRefocus(object userData)
    {
    }

    /// <summary>
    /// 界面轮询逻辑(子类可重写)
    /// </summary>
    protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    /// 界面深度改变逻辑(子类可重写)
    /// </summary>
    /// <param name="uiGroupDepth">界面组深度</param>
    /// <param name="depthInUIGroup">界面在界面组中的深度</param>
    protected virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    #endregion

    #region PointerEventListener 绑定

    /// <summary>
    /// 绑定 PointerEventListener 事件
    /// 根据 listener.useEvents 自动绑定对应的事件回调
    /// </summary>
    protected void PointerEventListenerBind(PointerEventListener listener)
    {
        if (listener == null) return;

        if ((listener.useEvents & PointerEventType.PointerEnter) != 0)
            listener.onPointerEnter += OnPointerEnter;

        if ((listener.useEvents & PointerEventType.PointerExit) != 0)
            listener.onPointerExit += OnPointerExit;

        if ((listener.useEvents & PointerEventType.PointerDown) != 0)
            listener.onPointerDown += OnPointerDown;

        if ((listener.useEvents & PointerEventType.PointerUp) != 0)
            listener.onPointerUp += OnPointerUp;

        if ((listener.useEvents & PointerEventType.Click) != 0)
            listener.onClick += OnClick;

        if ((listener.useEvents & PointerEventType.DoubleClick) != 0)
            listener.onDoubleClick += OnDoubleClick;

        if ((listener.useEvents & PointerEventType.LongPressStart) != 0)
            listener.onLongPressStart += OnLongPressStart;

        if ((listener.useEvents & PointerEventType.LongPressing) != 0)
            listener.onLongPressing += OnLongPressing;

        if ((listener.useEvents & PointerEventType.LongPressEnd) != 0)
            listener.onLongPressEnd += OnLongPressEnd;

        if ((listener.useEvents & PointerEventType.BeginDrag) != 0)
            listener.onBeginDrag += OnBeginDrag;

        if ((listener.useEvents & PointerEventType.Drag) != 0)
            listener.onDrag += OnDrag;

        if ((listener.useEvents & PointerEventType.EndDrag) != 0)
            listener.onEndDrag += OnEndDrag;

        if ((listener.useEvents & PointerEventType.Drop) != 0)
            listener.onDrop += OnDrop;

        if ((listener.useEvents & PointerEventType.Scroll) != 0)
            listener.onScroll += OnScroll;

        if ((listener.useEvents & PointerEventType.Select) != 0)
            listener.onSelect += OnSelect;

        if ((listener.useEvents & PointerEventType.Deselect) != 0)
            listener.onDeselect += OnDeselect;
    }

    #endregion

    #region PointerEventListener 事件回调 (子类可重写)

    /// <summary>
    /// 指针进入事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnPointerEnter(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 指针离开事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnPointerExit(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 指针按下事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnPointerDown(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 指针抬起事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnPointerUp(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 点击事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnClick(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 双击事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnDoubleClick(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 长按开始事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnLongPressStart(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 长按持续事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnLongPressing(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 长按结束事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnLongPressEnd(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 开始拖拽事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnBeginDrag(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 拖拽中事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnDrag(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 结束拖拽事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnEndDrag(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 拖放事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnDrop(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 滚动事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnScroll(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 选中事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnSelect(PointerEventListenerData data)
    {
    }

    /// <summary>
    /// 取消选中事件 (子类可重写)
    /// </summary>
    /// <param name="data">事件数据，通过 data.GameObject.name 获取触发对象</param>
    protected virtual void OnDeselect(PointerEventListenerData data)
    {
    }

    #endregion
}

}
