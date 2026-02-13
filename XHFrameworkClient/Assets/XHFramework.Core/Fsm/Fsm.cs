using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

    /// <summary>
    /// 状态机
    /// </summary>
    /// <typeparam name="T">状态机持有者类型</typeparam>
    public class Fsm<T> : IFsm where T:class
    {
        #region 字段与属性
        /// <summary>
        /// 状态机名字
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 状态机持有者
        /// </summary>
        public T Owner { get; private set; }

        /// <summary>
        /// 状态机里所有状态的字典
        /// </summary>
        private Dictionary<string, FsmState<T>> m_States;

        /// <summary>
        /// 状态机里所有数据的字典
        /// </summary>
        private Dictionary<string, object> m_Datas;

        /// <summary>
        /// 状态机是否被销毁
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public FsmState<T> CurrentState { get; private set; }

        /// <summary>
        /// 当前状态运行时间
        /// </summary>
        public float CurrentStateTime { get; private set; }

        /// <summary>
        /// 获取状态机持有者类型
        /// </summary>
        public Type OwnerType
        {
            get
            {
                return typeof(T);
            }
        }
        #endregion

        #region 构造方法
        public Fsm(string name, T owner, params FsmState<T>[] states)
        {
            if (owner == null)
            {
                Debug.LogError("状态机持有者为空");
            }

            if (states == null || states.Length < 1)
            {
                Debug.LogError("状态机没有状态");
            }

            Name = name;
            Owner = owner;
            m_States = new Dictionary<string, FsmState<T>>();
            m_Datas = new Dictionary<string, object>();

            foreach (FsmState<T> state in states)
            {
                if (state == null)
                {
                    Debug.LogError("要添加进状态机的状态为空");
                }

                string stateName = state.GetType().FullName;
                if (m_States.ContainsKey(stateName))
                {
                    Debug.LogError("要添加进状态机的状态已存在：" + stateName);
                }

                m_States.Add(stateName, state);
                state.OnInit(this);
            }

            CurrentStateTime = 0f;
            CurrentState = null;
            IsDestroyed = false;

        }
        #endregion

        #region 生命周期
        /// <summary>
        /// 状态机轮询。
        /// </summary>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (CurrentState == null)
            {
                return;
            }

            CurrentStateTime += elapseSeconds;
            CurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
        }


        /// <summary>
        /// 关闭并清理状态机。
        /// </summary>
        public void Shutdown()
        {
            if (CurrentState != null)
            {
                CurrentState.OnLeave(this, true);
                CurrentState = null;
                CurrentStateTime = 0f;
            }

            foreach (KeyValuePair<string, FsmState<T>> state in m_States)
            {
                state.Value.OnDestroy(this);
            }

            m_States.Clear();
            m_Datas.Clear();

            IsDestroyed = true;
        }
        #endregion

        #region 状态机状态操作

        #region 获取状态
        /// <summary>
        /// 获取状态
        /// </summary>
        public TState GetState<TState>() where TState : FsmState<T>
        {
            return GetState(typeof(TState)) as TState;
        }
        /// <summary>
        /// 获取状态
        /// </summary>
        public FsmState<T> GetState(Type stateType)
        {
            if (stateType == null)
            {
                Debug.LogError("要获取的状态为空");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                Debug.LogError("要获取的状态" + stateType.FullName + "没有直接或间接的实现" + typeof(FsmState<T>).FullName);
            }

            FsmState<T> state = null;
            if (m_States.TryGetValue(stateType.FullName, out state))
            {
                return state;
            }

            return null;
        }
        #endregion

        #region 开始状态机
        /// <summary>
        /// 开始状态机
        /// </summary>
        /// <typeparam name="TState">开始的状态类型</typeparam>
        public void Start<TState>() where TState : FsmState<T>
        {
            Start(typeof(TState));
        }
        /// <summary>
        /// 开始状态机
        /// </summary>
        /// <param name="stateType">要开始的状态类型。</param>
        public void Start(Type stateType)
        {
            if (CurrentState != null)
            {
                Debug.LogError("当前状态机已开始，无法再次开始");
            }

            if (stateType == null)
            {
                Debug.LogError("要开始的状态为空，无法开始");
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                Debug.Log("获取到的状态为空，无法开始");
            }

            CurrentStateTime = 0f;
            CurrentState = state;
            CurrentState.OnEnter(this);
        }


        #endregion

        #region 切换状态
        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState<TState>() where TState : FsmState<T>
        {
            ChangeState(typeof(TState));
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState(Type type)
        {
            if (CurrentState == null)
            {
                Debug.LogError("当前状态机状态为空，无法切换状态");
            }

            FsmState<T> state = GetState(type);
            if (state == null)
            {
                Debug.Log("获取到的状态为空，无法切换：" + type.FullName);
            }
            CurrentState.OnLeave(this, false);
            CurrentStateTime = 0f;
            CurrentState = state;
            CurrentState.OnEnter(this);
        }


        #endregion

        #region 抛出事件
        /// <summary>
        /// 抛出状态机事件
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="eventId">事件编号</param>
        public void FireEvent(object sender, int eventId)
        {
            if (CurrentState == null)
            {
                Debug.Log("当前状态为空，无法抛出事件");
            }

            CurrentState.OnEvent(this, sender, eventId, null);
        }
        #endregion

        #endregion

        #region 状态机数据操作
        /// <summary>
        /// 是否存在状态机数据
        /// </summary>
        public bool HasData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("要查询的状态机数据名字为空");
            }

            return m_Datas.ContainsKey(name);
        }

        /// <summary>
        /// 获取状态机数据
        /// </summary>
        public TDate GetData<TDate>(string name)
        {
            return (TDate)GetData(name);
        }

        /// <summary>
        /// 获取状态机数据
        /// </summary>
        public object GetData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("要获取的状态机数据名字为空");
            }

            object data = null;
            m_Datas.TryGetValue(name, out data);
            return data;
        }

        /// <summary>
        /// 设置状态机数据
        /// </summary>
        public void SetData(string name, object data)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("要设置的状态机数据名字为空");
            }

            m_Datas[name] = data;
        }

        /// <summary>
        /// 移除状态机数据
        /// </summary>
        public bool RemoveData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("要移除的状态机数据名字为空");
            }

            return m_Datas.Remove(name);
        }
        #endregion



    }

}
