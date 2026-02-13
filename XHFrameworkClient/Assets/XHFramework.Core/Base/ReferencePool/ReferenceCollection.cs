using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 引用集合
    /// </summary>
    public class ReferenceCollection
    {
        /// <summary>
        /// 引用队列
        /// </summary>
        private readonly Queue<IReference> _references = new();

        #region 引用队列的操作

        /// <summary>
        /// 获取引用
        /// </summary>
        public T Acquire<T>() where T : class, IReference, new()
        {
            lock (_references)
            {
                if (_references.Count > 0)
                {
                    return _references.Dequeue() as T;
                }
            }

            return new T();
        }

        /// <summary>
        /// 获取引用
        /// </summary>
        public IReference Acquire(Type referenceType)
        {
            lock (_references)
            {
                if (_references.Count > 0)
                {
                    return _references.Dequeue();
                }
            }
         
            return (IReference)Activator.CreateInstance(referenceType);
        }

        /// <summary>
        /// 释放引用
        /// </summary>
        public void Release<T>(T reference) where T : class, IReference
        {
            reference.Clear();
            lock (_references)
            {
                _references.Enqueue(reference);
            }
        }

        /// <summary>
        /// 添加引用
        /// </summary>
        public void Add<T>(int count) where T : class, IReference, new()
        {
            lock (_references)
            {
                while (count-- > 0)
                {
                    _references.Enqueue(new T());
                }
            }
        }

        /// <summary>
        /// 删除引用
        /// </summary>
        public void Remove<T>(int count) where T : class, IReference
        {
            lock (_references)
            {
                if (count > _references.Count)
                {
                    count = _references.Count;
                }

                while (count-- > 0)
                {
                    _references.Dequeue();
                }
            }
        }

        /// <summary>
        /// 删除所有引用
        /// </summary>
        public void RemoveAll()
        {
            lock (_references)
            {
                _references.Clear();
            }
        }
        #endregion
    }
}
