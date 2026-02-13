using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace XHFramework.Core {

    /// <summary>
    /// 状态机管理器
    /// </summary>
    public class FsmManager : ManagerBase
    {
        /// <summary>
        /// 所有状态机的字典
        /// </summary>
        private readonly Dictionary<string, IFsm> _fsms = new();
        private readonly List<IFsm> _tempFsms = new();

        public override int Priority
        {
            get
            {
                return 60;
            }
        }

        #region 生命周期
        public override void Init()
        {

        }

        public override void Shutdown()
        {
            foreach (KeyValuePair<string, IFsm> fsm in _fsms)
            {
                fsm.Value.Shutdown();
            }

            _fsms.Clear();
            _tempFsms.Clear();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _tempFsms.Clear();
            if (_fsms.Count <= 0)
            {
                return;
            }

            foreach (KeyValuePair<string, IFsm> fsm in _fsms)
            {
                _tempFsms.Add(fsm.Value);
            }

            foreach (IFsm fsm in _tempFsms)
            {
                if (fsm.IsDestroyed)
                {
                    continue;
                }
                //轮询状态机
                fsm.Update(elapseSeconds, realElapseSeconds);
            }
        }
        #endregion

        #region 状态机的操作

        #region 是否存在状态机
        /// <summary>
        /// 是否存在状态机
        /// </summary>
        private bool HasFsm(string fullName)
        {
            return _fsms.ContainsKey(fullName);
        }

        /// <summary>
        /// 是否存在状态机
        /// </summary>
        public bool HasFsm<T>()
        {
            return HasFsm(typeof(T));
        }

        /// <summary>
        /// 是否存在状态机
        /// </summary>
        public bool HasFsm(Type type)
        {
            return HasFsm(type.FullName);
        }
        #endregion



        /// <summary>
        /// 创建状态机。
        /// </summary>
        /// <typeparam name="T">状态机持有者类型</typeparam>
        /// <param name="name">状态机名称</param>
        /// <param name="owner">状态机持有者</param>
        /// <param name="states">状态机状态集合</param>
        /// <returns>要创建的状态机</returns>
        public Fsm<T> CreateFsm<T>(T owner, string name = "", params FsmState<T>[] states) where T : class
        {
            if (HasFsm<T>())
            {
                Debug.LogError("要创建的状态机已存在");
            }
            if (name == "")
            {
                name = typeof(T).FullName;
            }
            Fsm<T> fsm = new Fsm<T>(name, owner, states);
            _fsms.Add(name, fsm);
            return fsm;
        }

        #region 销毁状态机
        /// <summary>
        /// 销毁状态机
        /// </summary>
        public bool DestroyFsm(string name)
        {
            IFsm fsm = null;
            if (_fsms.TryGetValue(name, out fsm))
            {
                fsm.Shutdown();
                return _fsms.Remove(name);
            }

            return false;
        }
        public bool DestroyFsm<T>() where T : class
        {
            return DestroyFsm(typeof(T).FullName);
        }
        public bool DestroyFsm(IFsm fsm)
        {
            return DestroyFsm(fsm.Name);
        }

        #endregion




        #endregion
    }

}
