using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 引用池对象接口
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 清理引用
        /// </summary>
        void Clear();
    }
}
