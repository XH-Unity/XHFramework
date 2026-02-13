using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

    /// <summary>
    /// 状态机接口
    /// </summary>
    public interface IFsm
    {
        /// <summary>
        /// 状态机名字
        /// </summary>
        string Name { get;}

        /// <summary>
        /// 状态机持有者类型
        /// </summary>
        Type OwnerType { get; }

        /// <summary>
        /// 状态机是否被销毁
        /// </summary>
        bool IsDestroyed { get;}


        /// <summary>
        /// 当前状态运行时间
        /// </summary>
        float CurrentStateTime { get;}

        /// <summary>
        /// 状态机轮询
        /// </summary>
        void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理状态机。
        /// </summary>
         void Shutdown();

    }

}
