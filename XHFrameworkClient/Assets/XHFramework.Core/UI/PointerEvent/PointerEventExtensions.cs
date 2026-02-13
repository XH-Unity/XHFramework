using System;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 指针事件扩展方法
/// 提供简洁的链式调用API，适用于UI和3D物体
/// </summary>
public static class PointerEventExtensions
{
    #region 基础事件

    /// <summary>
    /// 添加点击事件
    /// </summary>
    public static PointerEventListener OnClick(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onClick += callback;
        return listener;
    }

    /// <summary>
    /// 添加双击事件
    /// </summary>
    public static PointerEventListener OnDoubleClick(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onDoubleClick += callback;
        return listener;
    }

    /// <summary>
    /// 添加指针按下事件
    /// </summary>
    public static PointerEventListener OnPointerDown(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onPointerDown += callback;
        return listener;
    }

    /// <summary>
    /// 添加指针抬起事件
    /// </summary>
    public static PointerEventListener OnPointerUp(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onPointerUp += callback;
        return listener;
    }

    #endregion

    #region 进入/离开事件

    /// <summary>
    /// 添加指针进入事件
    /// </summary>
    public static PointerEventListener OnPointerEnter(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onPointerEnter += callback;
        return listener;
    }

    /// <summary>
    /// 添加指针离开事件
    /// </summary>
    public static PointerEventListener OnPointerExit(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onPointerExit += callback;
        return listener;
    }

    #endregion

    #region 长按事件

    /// <summary>
    /// 添加长按开始事件
    /// </summary>
    public static PointerEventListener OnLongPressStart(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onLongPressStart += callback;
        return listener;
    }

    /// <summary>
    /// 添加长按持续事件
    /// </summary>
    public static PointerEventListener OnLongPressing(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onLongPressing += callback;
        return listener;
    }

    /// <summary>
    /// 添加长按结束事件
    /// </summary>
    public static PointerEventListener OnLongPressEnd(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onLongPressEnd += callback;
        return listener;
    }

    #endregion

    #region 拖拽事件

    /// <summary>
    /// 添加开始拖拽事件
    /// </summary>
    public static PointerEventListener OnBeginDrag(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onBeginDrag += callback;
        return listener;
    }

    /// <summary>
    /// 添加拖拽中事件
    /// </summary>
    public static PointerEventListener OnDrag(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onDrag += callback;
        return listener;
    }

    /// <summary>
    /// 添加结束拖拽事件
    /// </summary>
    public static PointerEventListener OnEndDrag(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onEndDrag += callback;
        return listener;
    }

    /// <summary>
    /// 添加放下事件
    /// </summary>
    public static PointerEventListener OnDrop(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onDrop += callback;
        return listener;
    }

    #endregion

    #region 滚动事件

    /// <summary>
    /// 添加滚动事件
    /// </summary>
    public static PointerEventListener OnScroll(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onScroll += callback;
        return listener;
    }

    #endregion

    #region 选择事件

    /// <summary>
    /// 添加选中事件
    /// </summary>
    public static PointerEventListener OnSelect(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onSelect += callback;
        return listener;
    }

    /// <summary>
    /// 添加取消选中事件
    /// </summary>
    public static PointerEventListener OnDeselect(this GameObject go, Action<PointerEventListenerData> callback)
    {
        var listener = PointerEventListener.Get(go);
        listener.onDeselect += callback;
        return listener;
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 清除所有事件监听
    /// </summary>
    public static void ClearPointerEvents(this GameObject go)
    {
        var listener = go.GetComponent<PointerEventListener>();
        if (listener != null)
        {
            listener.ClearAllListeners();
        }
    }

    /// <summary>
    /// 移除事件监听器组件
    /// </summary>
    public static void RemovePointerEventListener(this GameObject go)
    {
        var listener = go.GetComponent<PointerEventListener>();
        if (listener != null)
        {
            UnityEngine.Object.Destroy(listener);
        }
    }

    #endregion
}

}
