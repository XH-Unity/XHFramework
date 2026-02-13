using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 全局事件基类（继承该类的事件类才能被事件池管理）
/// </summary>
public abstract class GlobalEventArgs : EventArgs,IReference
{
    /// <summary>
    /// 事件类型ID
    /// </summary>
    public abstract int Id { get; }

    public abstract void Clear();
}


/// <summary>
/// 事件池
/// </summary>
/// <typeparam name="T">事件类型</typeparam>
public class EventPool<T> where T:GlobalEventArgs
{
    /// <summary>
    /// 事件结点
    /// </summary>
    private class Event
    {

        public Event(object sender, T e)
        {
            Sender = sender;
            EventArgs = e;
        }
        /// <summary>
        /// 事件发送者
        /// </summary>
        public object Sender { get; private set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public T EventArgs { get; private set; }
    }

    #region 字段与属性

    /// <summary>
    /// 事件码与对应处理方法的字典
    /// </summary>
    private readonly Dictionary<int, EventHandler<T>> _eventHandlers;

    /// <summary>
    /// 事件队列
    /// </summary>
    private readonly Queue<Event> _events;

    #endregion

    #region 构造方法
    public EventPool()
    {
        _eventHandlers = new Dictionary<int, EventHandler<T>>();
        _events = new Queue<Event>();
    }
    #endregion

    #region 事件的订阅与取消
    /// <summary>
    /// 检查订阅事件处理方法是否存在
    /// </summary>
    public bool Check(int id, EventHandler<T> handler)
    {
        if (handler == null)
        {
            Debug.LogError("事件处理方法为空");
            return false;
        }

        EventHandler<T> handlers = null;
        if (!_eventHandlers.TryGetValue(id, out handlers))
        {
            return false;
        }

        if (handlers == null)
        {
            return false;
        }

        //遍历委托里的所有方法
        foreach (EventHandler<T> i in handlers.GetInvocationList())
        {
            if (i == handler)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    public void Subscribe(int id, EventHandler<T> handler)
    {
        if (handler == null)
        {
            Debug.LogError("事件处理方法为空，无法订阅");
            return;
        }

        EventHandler<T> eventHandler = null;
        //检查是否获取处理方法失败或获取到的为空
        if (!_eventHandlers.TryGetValue(id, out eventHandler) || eventHandler == null)
        {
            _eventHandlers[id] = handler;
        }
        //不为空，就检查是否处理方法重复了
        else if (Check(id,handler))
        {
            Debug.LogError("要订阅事件的处理方法已存在");
        }
        else
        {
            eventHandler += handler;
            _eventHandlers[id] = eventHandler;
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public void Unsubscribe(int id, EventHandler<T> handler)
    {
        if (handler == null)
        {
            Debug.LogError("事件处理方法为空，无法取消订阅");
            return;
        }

        if (_eventHandlers.ContainsKey(id))
        {
            _eventHandlers[id] -= handler;
        }
    }

    #endregion

    #region 事件抛出
    /// <summary>
    /// 抛出事件（线程安全）
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    public void Fire(object sender, T e)
    {
        //将事件源和事件参数封装为Event加入队列
        Event eventNode = new Event(sender, e);
        lock (_events)
        {
            _events.Enqueue(eventNode);
        }
    }

    /// <summary>
    /// 抛出事件（线程不安全）
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    public void FireNow(object sender, T e)
    {
        HandleEvent(sender, e);
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 事件池轮询（用于处理线程安全的事件）
    /// </summary>
    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        while (_events.Count > 0)
        {
            Event e = null;
            lock (_events)
            {
                e = _events.Dequeue();
            }
            //从封装的Event中取出事件数据进行处理
            HandleEvent(e.Sender, e.EventArgs);
        }
    }

    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="sender">事件来源</param>
    /// <param name="e">事件参数</param>
    private void HandleEvent(object sender, T e)
    {
        //尝试获取事件的处理方法
        int eventId = e.Id;
        EventHandler<T> handlers = null;
        if (_eventHandlers.TryGetValue(eventId, out handlers))
        {
            if (handlers != null)
            {
                handlers(sender, e);
            }
            else
            {
                Debug.LogError("事件没有对应处理方法：" + eventId);
            }
        }

        //向引用池归还事件引用
        ReferencePool.Release(e);
    }
    #endregion

    #region 清理与关闭
    /// <summary>
    /// 清理事件。
    /// </summary>
    public void Clear()
    {
        lock (_events)
        {
            // 将队列中的事件对象归还到引用池，避免内存泄漏
            while (_events.Count > 0)
            {
                Event e = _events.Dequeue();
                ReferencePool.Release(e.EventArgs);
            }
        }
    }

    /// <summary>
    /// 关闭并清理事件池。
    /// </summary>
    public void Shutdown()
    {
        Clear();
        _eventHandlers.Clear();
    }
    #endregion



}

}


