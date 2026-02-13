using UnityEngine;
using UnityEngine.EventSystems;

namespace XHFramework.Core {

/// <summary>
/// 指针事件数据
/// 封装了所有指针相关的事件信息，适用于UI和3D物体
/// </summary>
public class PointerEventListenerData
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public PointerEventType EventType { get; set; }

    /// <summary>
    /// 触发事件的游戏对象
    /// </summary>
    public GameObject GameObject { get; set; }

    /// <summary>
    /// 原始指针事件数据
    /// </summary>
    public PointerEventData PointerEventData { get; set; }

    /// <summary>
    /// 当前位置（屏幕坐标）
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// 位置增量
    /// </summary>
    public Vector2 Delta { get; set; }

    /// <summary>
    /// 按下时的位置
    /// </summary>
    public Vector2 PressPosition { get; set; }

    /// <summary>
    /// 滚动增量
    /// </summary>
    public Vector2 ScrollDelta { get; set; }

    /// <summary>
    /// 点击次数（用于双击检测）
    /// </summary>
    public int ClickCount { get; set; }

    /// <summary>
    /// 长按持续时间
    /// </summary>
    public float LongPressDuration { get; set; }

    /// <summary>
    /// 是否正在拖拽
    /// </summary>
    public bool IsDragging { get; set; }

    /// <summary>
    /// 拖拽距离
    /// </summary>
    public float DragDistance => Vector2.Distance(Position, PressPosition);

    /// <summary>
    /// 自定义用户数据
    /// </summary>
    public object UserData { get; set; }

    /// <summary>
    /// 重置数据
    /// </summary>
    public void Reset()
    {
        EventType = PointerEventType.None;
        GameObject = null;
        PointerEventData = null;
        Position = Vector2.zero;
        Delta = Vector2.zero;
        PressPosition = Vector2.zero;
        ScrollDelta = Vector2.zero;
        ClickCount = 0;
        LongPressDuration = 0f;
        IsDragging = false;
        UserData = null;
    }
}

}
