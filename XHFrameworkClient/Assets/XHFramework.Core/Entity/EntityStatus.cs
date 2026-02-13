using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

    /// <summary>
    /// 实体状态
    /// </summary>
    public enum EntityStatus
    {
        /// <summary>
        /// 将要初始化
        /// </summary>
        WillInit,

        /// <summary>
        /// 已初始化
        /// </summary>
        Inited,

        /// <summary>
        /// 将要显示
        /// </summary>
        WillShow,

        /// <summary>
        /// 已显示
        /// </summary>
        Showed,

        /// <summary>
        /// 将要隐藏
        /// </summary>
        WillHide,

        /// <summary>
        /// 已隐藏
        /// </summary>
        Hidden,

         /// <summary>
        /// 将要回收
        /// </summary>
        WillRecycle,

        /// <summary>
        /// 已回收
        /// </summary>
        Recycled,
    }

}


