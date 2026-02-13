using System;

namespace XHFramework.Core {

/// <summary>
/// 指针事件类型枚举
/// 适用于UI和3D物体
/// </summary>
[Flags]
public enum PointerEventType
{
    None = 0,

    /// <summary>
    /// 指针进入
    /// </summary>
    PointerEnter = 1 << 0,

    /// <summary>
    /// 指针离开
    /// </summary>
    PointerExit = 1 << 1,

    /// <summary>
    /// 指针按下
    /// </summary>
    PointerDown = 1 << 2,

    /// <summary>
    /// 指针抬起
    /// </summary>
    PointerUp = 1 << 3,

    /// <summary>
    /// 点击
    /// </summary>
    Click = 1 << 4,

    /// <summary>
    /// 双击
    /// </summary>
    DoubleClick = 1 << 5,

    /// <summary>
    /// 长按开始
    /// </summary>
    LongPressStart = 1 << 6,

    /// <summary>
    /// 长按持续中
    /// </summary>
    LongPressing = 1 << 7,

    /// <summary>
    /// 长按结束
    /// </summary>
    LongPressEnd = 1 << 8,

    /// <summary>
    /// 开始拖拽
    /// </summary>
    BeginDrag = 1 << 9,

    /// <summary>
    /// 拖拽中
    /// </summary>
    Drag = 1 << 10,

    /// <summary>
    /// 结束拖拽
    /// </summary>
    EndDrag = 1 << 11,

    /// <summary>
    /// 拖拽放下（在目标上释放）
    /// </summary>
    Drop = 1 << 12,

    /// <summary>
    /// 滚动
    /// </summary>
    Scroll = 1 << 13,

    /// <summary>
    /// 选中
    /// </summary>
    Select = 1 << 14,

    /// <summary>
    /// 取消选中
    /// </summary>
    Deselect = 1 << 15,

    /// <summary>
    /// 所有事件
    /// </summary>
    All = ~0
}

}
