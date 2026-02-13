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
    public class EntityInstanceObject : ObjectBase
    {
        public static EntityInstanceObject Create(object target,string name="") 
        {
            EntityInstanceObject entityInstanceObject = ReferencePool.Acquire<EntityInstanceObject>();
            entityInstanceObject.Target = target;
            entityInstanceObject.Name = name;
            return entityInstanceObject;
        }
   
    }

}
