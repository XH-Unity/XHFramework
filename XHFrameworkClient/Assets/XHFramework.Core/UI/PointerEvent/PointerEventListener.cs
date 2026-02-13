using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XHFramework.Core {

/// <summary>
/// 指针事件监听器
/// 支持UI元素和3D物体的事件监听
/// 功能：点击、双击、长按、拖拽、进入/离开、滚动、事件穿透等
/// </summary>
public class PointerEventListener : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IScrollHandler,
    ISelectHandler,
    IDeselectHandler
{
    #region 事件回调委托

    public Action<PointerEventListenerData> onPointerEnter;
    public Action<PointerEventListenerData> onPointerExit;
    public Action<PointerEventListenerData> onPointerDown;
    public Action<PointerEventListenerData> onPointerUp;
    public Action<PointerEventListenerData> onClick;
    public Action<PointerEventListenerData> onDoubleClick;
    public Action<PointerEventListenerData> onLongPressStart;
    public Action<PointerEventListenerData> onLongPressing;
    public Action<PointerEventListenerData> onLongPressEnd;
    public Action<PointerEventListenerData> onBeginDrag;
    public Action<PointerEventListenerData> onDrag;
    public Action<PointerEventListenerData> onEndDrag;
    public Action<PointerEventListenerData> onDrop;
    public Action<PointerEventListenerData> onScroll;
    public Action<PointerEventListenerData> onSelect;
    public Action<PointerEventListenerData> onDeselect;

    #endregion

    #region 配置参数
    /// <summary>
    /// 穿透的事件类型
    /// </summary>
    [Header("应用事件类型")]
    public PointerEventType useEvents = PointerEventType.None;
    /// <summary>
    /// 双击间隔时间（秒）
    /// </summary>
    [Header("双击设置")]
    [Tooltip("双击的最大间隔时间")]
    public float doubleClickInterval = 0.3f;

    /// <summary>
    /// 长按触发时间（秒）
    /// </summary>
    [Header("长按设置")]
    [Tooltip("触发长按的最小按住时间")]
    public float longPressTime = 0.5f;

    /// <summary>
    /// 长按持续回调间隔（秒）
    /// </summary>
    [Tooltip("长按持续中的回调间隔")]
    public float longPressInterval = 0.1f;

    /// <summary>
    /// 拖拽阈值（像素）
    /// </summary>
    [Header("拖拽设置")]
    [Tooltip("开始拖拽的最小移动距离")]
    public float dragThreshold = 10f;

    /// <summary>
    /// 是否允许事件穿透
    /// </summary>
    [Header("事件穿透")]
    [Tooltip("是否允许事件穿透到下层UI或3D物体")]
    public bool passThrough = false;

    /// <summary>
    /// 穿透的事件类型
    /// </summary>
    [Tooltip("允许穿透的事件类型")]
    public PointerEventType passThroughEvents = PointerEventType.None;

    /// <summary>
    /// 是否穿透到3D物体
    /// </summary>
    [Tooltip("是否允许穿透到3D物体（需要Camera上有PhysicsRaycaster）")]
    public bool passThroughTo3D = false;

    /// <summary>
    /// 是否忽略所有事件
    /// </summary>
    [Header("事件控制")]
    [Tooltip("是否忽略所有事件")]
    public bool ignoreAllEvents = false;

    /// <summary>
    /// 忽略的事件类型
    /// </summary>
    [Tooltip("忽略的事件类型")]
    public PointerEventType ignoredEvents = PointerEventType.None;

    #endregion

    #region 私有字段

    private PointerEventListenerData _eventData;
    private float _lastClickTime;
    private int _clickCount;
    private float _pointerDownTime;
    private float _lastLongPressTime;
    private bool _isPointerDown;
    private bool _isLongPressing;
    private bool _isDragging;
    private Vector2 _pointerDownPosition;

    #endregion

    #region 静态方法

    /// <summary>
    /// 获取或添加事件监听器
    /// </summary>
    public static PointerEventListener Get(GameObject go)
    {
        if (go == null)
        {
            Log.Error("PointerEventListener.Get: GameObject is null");
            return null;
        }

        PointerEventListener listener = go.GetComponent<PointerEventListener>();
        if (listener == null)
        {
            listener = go.AddComponent<PointerEventListener>();
        }
        return listener;
    }
    #endregion

    #region Unity生命周期

    private void Awake()
    {
        _eventData = new PointerEventListenerData();
    }

    private void Update()
    {
        if (!_isPointerDown || ignoreAllEvents)
            return;

        float holdTime = Time.unscaledTime - _pointerDownTime;

        // 长按检测
        if (!_isDragging && holdTime >= longPressTime)
        {
            if (!_isLongPressing)
            {
                // 长按开始
                _isLongPressing = true;
                _lastLongPressTime = Time.unscaledTime;
                InvokeLongPressStart();
            }
            else if (Time.unscaledTime - _lastLongPressTime >= longPressInterval)
            {
                // 长按持续中
                _lastLongPressTime = Time.unscaledTime;
                _eventData.LongPressDuration = holdTime;
                InvokeLongPressing();
            }
        }
    }

    private void OnDisable()
    {
        // 重置状态
        ResetState();
    }

    #endregion

    #region 接口实现

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.PointerEnter))
            return;

        FillEventData(eventData, PointerEventType.PointerEnter);
        onPointerEnter?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.PointerEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.PointerExit))
            return;

        FillEventData(eventData, PointerEventType.PointerExit);
        onPointerExit?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.PointerExit);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.PointerDown))
            return;

        _isPointerDown = true;
        _pointerDownTime = Time.unscaledTime;
        _pointerDownPosition = eventData.position;

        FillEventData(eventData, PointerEventType.PointerDown);
        _eventData.PressPosition = _pointerDownPosition;
        onPointerDown?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.PointerDown);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.PointerUp))
            return;

        // 长按结束
        if (_isLongPressing)
        {
            _eventData.LongPressDuration = Time.unscaledTime - _pointerDownTime;
            InvokeLongPressEnd();
        }

        FillEventData(eventData, PointerEventType.PointerUp);
        _eventData.PressPosition = _pointerDownPosition;
        onPointerUp?.Invoke(_eventData);

        ResetState();
        TryPassThrough(eventData, PointerEventType.PointerUp);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果正在长按或拖拽，不触发点击
        if (_isLongPressing || _isDragging)
            return;

        if (ShouldIgnoreEvent(PointerEventType.Click))
            return;

        // 双击检测
        float currentTime = Time.unscaledTime;
        if (currentTime - _lastClickTime <= doubleClickInterval)
        {
            _clickCount++;
        }
        else
        {
            _clickCount = 1;
        }
        _lastClickTime = currentTime;

        FillEventData(eventData, PointerEventType.Click);
        _eventData.ClickCount = _clickCount;

        if (_clickCount >= 2 && !ShouldIgnoreEvent(PointerEventType.DoubleClick))
        {
            // 双击
            _eventData.EventType = PointerEventType.DoubleClick;
            onDoubleClick?.Invoke(_eventData);
            _clickCount = 0;
            TryPassThrough(eventData, PointerEventType.DoubleClick);
        }
        else
        {
            // 单击
            onClick?.Invoke(_eventData);
            TryPassThrough(eventData, PointerEventType.Click);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.BeginDrag))
            return;

        _isDragging = true;
        FillEventData(eventData, PointerEventType.BeginDrag);
        _eventData.IsDragging = true;
        _eventData.PressPosition = _pointerDownPosition;
        onBeginDrag?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.BeginDrag);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.Drag))
            return;

        FillEventData(eventData, PointerEventType.Drag);
        _eventData.IsDragging = true;
        _eventData.PressPosition = _pointerDownPosition;
        onDrag?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.Drag);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.EndDrag))
            return;

        FillEventData(eventData, PointerEventType.EndDrag);
        _eventData.IsDragging = false;
        _eventData.PressPosition = _pointerDownPosition;
        onEndDrag?.Invoke(_eventData);

        _isDragging = false;
        TryPassThrough(eventData, PointerEventType.EndDrag);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.Drop))
            return;

        FillEventData(eventData, PointerEventType.Drop);
        onDrop?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.Drop);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.Scroll))
            return;

        FillEventData(eventData, PointerEventType.Scroll);
        _eventData.ScrollDelta = eventData.scrollDelta;
        onScroll?.Invoke(_eventData);
        TryPassThrough(eventData, PointerEventType.Scroll);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.Select))
            return;

        _eventData.EventType = PointerEventType.Select;
        _eventData.GameObject = gameObject;
        onSelect?.Invoke(_eventData);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (ShouldIgnoreEvent(PointerEventType.Deselect))
            return;

        _eventData.EventType = PointerEventType.Deselect;
        _eventData.GameObject = gameObject;
        onDeselect?.Invoke(_eventData);
    }

    #endregion

    #region 私有方法

    private void FillEventData(PointerEventData eventData, PointerEventType eventType)
    {
        _eventData.EventType = eventType;
        _eventData.GameObject = gameObject;
        _eventData.PointerEventData = eventData;
        _eventData.Position = eventData.position;
        _eventData.Delta = eventData.delta;
    }

    private void InvokeLongPressStart()
    {
        if (ShouldIgnoreEvent(PointerEventType.LongPressStart))
            return;

        _eventData.EventType = PointerEventType.LongPressStart;
        _eventData.LongPressDuration = Time.unscaledTime - _pointerDownTime;
        onLongPressStart?.Invoke(_eventData);
    }

    private void InvokeLongPressing()
    {
        if (ShouldIgnoreEvent(PointerEventType.LongPressing))
            return;

        _eventData.EventType = PointerEventType.LongPressing;
        onLongPressing?.Invoke(_eventData);
    }

    private void InvokeLongPressEnd()
    {
        if (ShouldIgnoreEvent(PointerEventType.LongPressEnd))
            return;

        _eventData.EventType = PointerEventType.LongPressEnd;
        onLongPressEnd?.Invoke(_eventData);
    }

    private bool ShouldIgnoreEvent(PointerEventType eventType)
    {
        if (ignoreAllEvents)
            return true;

        return (ignoredEvents & eventType) != 0;
    }

    private void TryPassThrough(PointerEventData eventData, PointerEventType eventType)
    {
        if (!passThrough)
            return;

        if ((passThroughEvents & eventType) == 0)
            return;

        // 穿透到下层
        PassEventToNext(eventData, eventType);
    }

    private void PassEventToNext(PointerEventData eventData, PointerEventType eventType)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool foundTarget = false;

        // 先尝试穿透到下层UI或带PointerEventListener的3D物体
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject == gameObject)
                continue;

            var listener = results[i].gameObject.GetComponent<PointerEventListener>();
            if (listener != null)
            {
                // 根据事件类型调用对应的处理
                ExecuteEventOnTarget(listener, eventData, eventType);
                foundTarget = true;
                break;
            }
        }

        // 如果没找到PointerEventListener且开启了3D穿透，尝试射线检测3D物体
        if (!foundTarget && passThroughTo3D)
        {
            PassEventTo3D(eventData, eventType);
        }
    }

    /// <summary>
    /// 穿透事件到3D物体
    /// </summary>
    private void PassEventTo3D(PointerEventData eventData, PointerEventType eventType)
    {
        Camera camera = eventData.pressEventCamera ?? Camera.main;
        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(eventData.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var listener = hit.collider.GetComponent<PointerEventListener>();
            if (listener != null && listener != this)
            {
                ExecuteEventOnTarget(listener, eventData, eventType);
            }
        }
    }

    private void ExecuteEventOnTarget(PointerEventListener listener, PointerEventData eventData, PointerEventType eventType)
    {
        switch (eventType)
        {
            case PointerEventType.PointerEnter:
                listener.OnPointerEnter(eventData);
                break;
            case PointerEventType.PointerExit:
                listener.OnPointerExit(eventData);
                break;
            case PointerEventType.PointerDown:
                listener.OnPointerDown(eventData);
                break;
            case PointerEventType.PointerUp:
                listener.OnPointerUp(eventData);
                break;
            case PointerEventType.Click:
            case PointerEventType.DoubleClick:
                listener.OnPointerClick(eventData);
                break;
            case PointerEventType.BeginDrag:
                listener.OnBeginDrag(eventData);
                break;
            case PointerEventType.Drag:
                listener.OnDrag(eventData);
                break;
            case PointerEventType.EndDrag:
                listener.OnEndDrag(eventData);
                break;
            case PointerEventType.Drop:
                listener.OnDrop(eventData);
                break;
            case PointerEventType.Scroll:
                listener.OnScroll(eventData);
                break;
        }
    }

    private void ResetState()
    {
        _isPointerDown = false;
        _isLongPressing = false;
        // 注意：_isDragging 在 OnEndDrag 中重置
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 清除所有事件监听
    /// </summary>
    public void ClearAllListeners()
    {
        onPointerEnter = null;
        onPointerExit = null;
        onPointerDown = null;
        onPointerUp = null;
        onClick = null;
        onDoubleClick = null;
        onLongPressStart = null;
        onLongPressing = null;
        onLongPressEnd = null;
        onBeginDrag = null;
        onDrag = null;
        onEndDrag = null;
        onDrop = null;
        onScroll = null;
        onSelect = null;
        onDeselect = null;
    }

    /// <summary>
    /// 设置自定义用户数据
    /// </summary>
    public void SetUserData(object userData)
    {
        _eventData.UserData = userData;
    }

    /// <summary>
    /// 启用/禁用所有事件
    /// </summary>
    public void SetEventsEnabled(bool enabled)
    {
        ignoreAllEvents = !enabled;
    }

    /// <summary>
    /// 设置忽略的事件类型
    /// </summary>
    public void SetIgnoredEvents(PointerEventType events)
    {
        ignoredEvents = events;
    }

    /// <summary>
    /// 添加忽略的事件类型
    /// </summary>
    public void AddIgnoredEvent(PointerEventType eventType)
    {
        ignoredEvents |= eventType;
    }

    /// <summary>
    /// 移除忽略的事件类型
    /// </summary>
    public void RemoveIgnoredEvent(PointerEventType eventType)
    {
        ignoredEvents &= ~eventType;
    }

    /// <summary>
    /// 设置事件穿透
    /// </summary>
    /// <param name="enabled">是否启用穿透</param>
    /// <param name="events">穿透的事件类型</param>
    /// <param name="includeTo3D">是否穿透到3D物体</param>
    public void SetPassThrough(bool enabled, PointerEventType events = PointerEventType.All, bool includeTo3D = false)
    {
        passThrough = enabled;
        passThroughEvents = events;
        passThroughTo3D = includeTo3D;
    }

    #endregion
}

}
