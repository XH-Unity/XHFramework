using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace XHFramework.Core {

    /// <summary>
    /// 界面实例对象
    /// </summary>
    public class UIFormInstanceObject : ObjectBase
    {
        public static UIFormInstanceObject Create(object target,string name="")
        {
            UIFormInstanceObject uiFormInstanceObject = ReferencePool.Acquire<UIFormInstanceObject>();
            uiFormInstanceObject.Target = target;
            uiFormInstanceObject.Name = name;
            return uiFormInstanceObject;
        }
    }

}
