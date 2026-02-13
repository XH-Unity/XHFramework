using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

    /// <summary>
    /// 状态机事件的响应方法模板
    /// </summary>
    public delegate void FsmEventHandler<T>(Fsm<T> fsm, object sender, object userData) where T : class;

    /// <summary>
    /// 状态基类
    /// </summary>
    /// <typeparam name="T">状态持有者类型</typeparam>
    public abstract class FsmState<T> where T : class
    {
        /// <summary>
        /// 状态订阅的事件字典
        /// </summary>
        private Dictionary<int, FsmEventHandler<T>> m_EventHandlers;

        public FsmState()
        {
            m_EventHandlers = new Dictionary<int, FsmEventHandler<T>>();
        }

        #region 状态生命周期
        /// <summary>
        /// 状态机状态初始化时调用。
        /// </summary>
        /// <param name="fsm">状态机引用。</param>
        public virtual void OnInit(Fsm<T> fsm)
        {

        }

        /// <summary>
        /// 状态机状态进入时调用。
        /// </summary>
        /// <param name="fsm">状态机引用。</param>
        public virtual void OnEnter(Fsm<T> fsm)
        {

        }

        /// <summary>
        /// 状态机状态轮询时调用
        /// </summary>
        /// <param name="fsm">状态机引用</param>
        public virtual void OnUpdate(Fsm<T> fsm, float elapseSeconds, float realElapseSeconds)
        {

        }

        /// <summary>
        /// 状态机状态离开时调用。
        /// </summary>
        /// <param name="fsm">状态机引用。</param>
        /// <param name="isShutdown">是关闭状态机时触发</param>
        public virtual void OnLeave(Fsm<T> fsm, bool isShutdown)
        {

        }

        /// <summary>
        /// 状态机状态销毁时调用
        /// </summary>
        /// <param name="fsm">状态机引用。</param>
        public virtual void OnDestroy(Fsm<T> fsm)
        {
            m_EventHandlers.Clear();
        }
        #endregion

        #region 状态切换
        /// <summary>
        /// 切换状态
        /// </summary>
        protected void ChangeState<TState>(Fsm<T> fsm) where TState : FsmState<T>
        {
            ChangeState(fsm, typeof(TState));
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        protected void ChangeState(Fsm<T> fsm,Type type)
        {
            if (fsm == null)
            {
                Debug.Log("需要切换状态的状态机为空，无法切换");
            }

            if (type == null)
            {
                Debug.Log("需要切换到的状态为空，无法切换");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(type))
            {
                Debug.Log("要切换的状态没有直接或间接实现FsmState<T>，无法切换");
            }

            fsm.ChangeState(type);
        }
        #endregion

        #region 状态机事件
        /// <summary>
        /// 订阅状态机事件。
        /// </summary>
        protected void SubscribeEvent(int eventId, FsmEventHandler<T> eventHandler)
        {
            if (eventHandler == null)
            {
                Debug.LogError("状态机事件处理方法为空，无法订阅状态机事件");
            }

            if (!m_EventHandlers.ContainsKey(eventId))
            {
                m_EventHandlers[eventId] = eventHandler;
            }
            else
            {
                m_EventHandlers[eventId] += eventHandler;
            }
        }

        /// <summary>
        /// 取消订阅状态机事件。
        /// </summary>
        protected void UnsubscribeEvent(int eventId, FsmEventHandler<T> eventHandler)
        {
            if (eventHandler == null)
            {
                Debug.LogError("状态机事件处理方法为空，无法取消订阅状态机事件");
            }

            if (m_EventHandlers.ContainsKey(eventId))
            {
                m_EventHandlers[eventId] -= eventHandler;
            }
        }

        /// <summary>
        /// 响应状态机事件。
        /// </summary>
        public void OnEvent(Fsm<T> fsm, object sender, int eventId, object userData)
        {
            FsmEventHandler<T> eventHandlers = null;
            if (m_EventHandlers.TryGetValue(eventId, out eventHandlers))
            {
                if (eventHandlers != null)
                {
                    eventHandlers(fsm, sender, userData);
                }
            }
        }
        #endregion
    }

}
