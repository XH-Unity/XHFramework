using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T">池子对象的类型</typeparam>
    public class ObjectPool<T> : IObjectPool where T:ObjectBase //,new()
    {
        #region 字段与属性
        private int _capacity;
        private float _expireTime;

        /// <summary>
        /// 对象池名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 池对象的链表
        /// </summary>
        private readonly LinkedList<ObjectBase> _objects;

        /// <summary>
        /// 池对象类型
        /// </summary>
        public Type ObjectType => typeof(T);

        /// <summary>
        /// 池对象的数量
        /// </summary>
        public int Count => _objects.Count;

        /// <summary>
        /// 池对象是否可被多次获取
        /// </summary>
        public bool AllowMultiSpawn { get; private set; }

        /// <summary>
        /// 可释放的对象的数量
        /// </summary>
        public int CanReleaseCount => GetCanReleaseObjects().Count;

        /// <summary>
        /// 对象池自动释放可释放对象计时
        /// </summary>
        public float AutoReleaseTime { get; private set; }

        /// <summary>
        /// 对象池自动释放可释放对象的间隔秒数
        /// </summary>
        public float AutoReleaseInterval { get; set; }

        /// <summary>
        /// 对象池容量
        /// </summary>
        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value < 0)
                {
                    Debug.LogError("设置对象池容量<0，无法设置");
                }

                if (_capacity == value)
                {
                    return;
                }

                _capacity = value;
                Release();
            }
        }

        /// <summary>
        /// 池对象过期秒数
        /// </summary>
        public float ExpireTime
        {
            get => _expireTime;

            set
            {
                if (value < 0)
                {
                    Debug.LogError("设置对象过期秒数<0，无法设置");
                }

                if (Mathf.Approximately(_expireTime, value))
                {
                    return;
                }

                _expireTime = value;
                Release();
            }
        }
        #endregion

        #region 构造方法

        public ObjectPool(string name,int capacity, float expireTime, bool allowMultiSpawn)
        {
            Name = name;
            _objects = new LinkedList<ObjectBase>();

            Capacity = capacity;
            AutoReleaseInterval = expireTime; //TODO ？？？
            ExpireTime = expireTime;
            AutoReleaseTime = 0f;
            AllowMultiSpawn = allowMultiSpawn;
        }

        #endregion
        

        #region 对象的创建，获取与回收
        /// <summary>
        /// 注册对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="spawned">对象是否已被获取</param>
        public void Register(T obj, bool spawned = false)
        {
            if (obj == null)
            {
                Debug.LogError("要放入对象池的对象为空:" + typeof(T).FullName);
                return;
            }
            //已被获取就让计数+1
            if (spawned)
            {
                obj.SpawnCount++;
            }
            _objects.AddLast(obj);

            Release();
            
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <returns>要获取的对象</returns>
        public T Spawn(string name = "")
        {
            foreach (ObjectBase obj in _objects)
            {
                if (obj.Name != name)
                {
                    continue;
                }

                if (AllowMultiSpawn|| !obj.IsInUse)
                {
                    Debug.Log("获取了对象：" + typeof(T).FullName +"/"+ obj.Name);
                    return obj.Spawn() as T;
                }
            }
            return null;
        }


        /// <summary>
        /// 回收对象
        /// </summary>
        public void Unspawn(object target)
        {
            if (target == null)
            {
                Debug.LogError("要回收的对象为空：" + typeof(object).FullName);
            }

            foreach (ObjectBase obj in _objects)
            {
                if (obj.Target == target)
                {
                    obj.Unspawn();
                    Debug.Log("对象被回收了：" + typeof(T).FullName + "/" + obj.Name);
                    Release();
                    return;
                }
            }

            Debug.LogError("找不到要回收的对象：" + typeof(object).FullName);
        }
        #endregion

        #region 释放对象
        /// <summary>
        /// 获取所有可以释放的对象
        /// </summary>
        private LinkedList<T> GetCanReleaseObjects()
        {
            LinkedList<T> canReleaseObjects = new LinkedList<T>();

            foreach (ObjectBase obj in _objects)
            {
                if (obj.IsInUse)
                {
                    continue;
                }

                canReleaseObjects.AddLast(obj as T);
            }

            return canReleaseObjects;
        }

        /// <summary>
        /// 释放超出对象池容量的可释放对象
        /// </summary>
        public void Release()
        {
            Release(_objects.Count - _capacity);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量</param>
        /// <param name="releaseObjectFilterCallback">释放对象筛选方法</param>
        public void Release(int toReleaseCount)
        {
            //重置计时
            AutoReleaseTime = 0;

            if (toReleaseCount <= 0)
            {
                return;
            }

            //计算对象过期参考时间
            DateTime expireTime = DateTime.MinValue;
            if (_expireTime < float.MaxValue)
            {
                //当前时间 - 过期秒数 = 过期参考时间
                expireTime = DateTime.Now.AddSeconds(-_expireTime);
            }

            //获取能释放的对象和实际要释放的对象
            LinkedList<T> canReleaseObjects = GetCanReleaseObjects();
            LinkedList<T> toReleaseObjects = DefaultReleaseObjectFilterCallBack(canReleaseObjects, toReleaseCount, expireTime);
            if (toReleaseObjects == null || toReleaseObjects.Count <= 0)
            {
                return;
            }

            //遍历寻找实际释放的对象
            foreach (ObjectBase toReleaseObject in toReleaseObjects)
            {
                if (toReleaseObject == null)
                {
                    Debug.LogError("无法释放空对象");
                }

                foreach (ObjectBase obj in _objects)
                {
                    if (obj != toReleaseObject)
                    {
                        continue;
                    }

                    //释放对象
                    _objects.Remove(obj);
                    obj.Release();
                    Debug.Log("对象被释放了：" + obj.Name);
                    break;
                }


            }

        }

        /// <summary>
        /// 释放对象池中所有未使用对象
        /// </summary>
        public void ReleaseAllUnused()
        {
            LinkedListNode<ObjectBase> current = _objects.First;
            while (current != null)
            {
                if (current.Value.IsInUse)
                {
                    current = current.Next;
                    continue;
                }

                LinkedListNode<ObjectBase> next = current.Next;
                _objects.Remove(current);
                current.Value.Release();
                Debug.Log("对象被释放了：" + current.Value.Name);
                current = next;
            }
        }

        /// <summary>
        /// 默认的释放对象筛选方法（未被使用且过期的对象）
        /// </summary>
        private LinkedList<T> DefaultReleaseObjectFilterCallBack(LinkedList<T> candidateObjects, int toReleaseCount, DateTime expireTime)
        {
            LinkedList<T> toReleaseObjects = new LinkedList<T>();

            if (expireTime > DateTime.MinValue)
            {
                LinkedListNode<T> current = candidateObjects.First;
                while (current != null)
                {
                    //对象最后使用时间 <= 过期参考时间，就需要释放
                    if ( current.Value.LastUseTime <= expireTime)
                    {
                        toReleaseObjects.AddLast(current.Value);
                        LinkedListNode<T> next = current.Next;
                        candidateObjects.Remove(current);
                        toReleaseCount--;
                        if (toReleaseCount <= 0)
                        {
                            return toReleaseObjects;
                        }
                        current = next;
                        continue;
                    }

                    current = current.Next;
                }

            }

            return toReleaseObjects;
        }

        /// <summary>
        /// 清理对象池
        /// </summary>
        public void Shutdown()
        {
            LinkedListNode<ObjectBase> current = _objects.First;
            while (current != null)
            {
                LinkedListNode<ObjectBase> next = current.Next;
                _objects.Remove(current);
                current.Value.Release();
                Debug.Log("对象被释放了：" + current.Value.Name);
                current = next;
            }
            _objects.Clear();
        }
        #endregion

        /// <summary>
        /// 对象池的定时释放
        /// </summary>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            AutoReleaseTime += realElapseSeconds;
            if (AutoReleaseTime < AutoReleaseInterval)
            {
                return;
            }

            Release();
        }


    }

}
