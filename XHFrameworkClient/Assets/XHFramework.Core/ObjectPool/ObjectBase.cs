using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

    /// <summary>
    /// 对象池对象基类
    /// </summary>
    public abstract class ObjectBase: IReference
    {
        /// <summary>
        /// 对象名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 对象（实际使用到的对象放到这里）
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// 对象上次使用时间
        /// </summary>
        public DateTime LastUseTime { get; private set; }

        /// <summary>
        /// 对象的获取计数
        /// </summary>
        public int SpawnCount { get; set; }

        /// <summary>
        /// 对象是否正在使用
        /// </summary>
        public bool IsInUse => SpawnCount > 0;

        // /// <summary>
        // /// 初始化对象基类。
        // /// </summary>
        // /// <param name="target">对象。</param>
        // public void Initialize(object target,string name = "")
        // {
        //     Name = name;    
        //     Target = target;
        // }
        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public virtual void Clear()
        {
            Name = null;
            Target = null;
            LastUseTime = default(DateTime);
        }


        /// <summary>
        /// 获取对象
        /// </summary>
        public ObjectBase Spawn()
        {
            SpawnCount++;
            LastUseTime = DateTime.Now;
            OnSpawn();
            return this;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        public void Unspawn()
        {
            OnUnspawn();
            LastUseTime = DateTime.Now;
            SpawnCount--;
        }
        
        /// <summary>
        /// 释放对象时  真正销毁
        /// </summary>
        public void Release()
        {
            GameObject go = (GameObject)Target;
            FW.ResourceManager.ReleaseGameObject(go);
            ReferencePool.Release(this);
            OnRelease();
        }

        #region 生命周期

        /// <summary>
        /// 获取对象时
        /// </summary>
        protected virtual void OnSpawn()
        {

        }

        /// <summary>
        /// 回收对象时
        /// </summary>
        protected virtual void OnUnspawn()
        {

        }
        
        public virtual void OnRelease()
        {
       
        }

        #endregion


    }

}
