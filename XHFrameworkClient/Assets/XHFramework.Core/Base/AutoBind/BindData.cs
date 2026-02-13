using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 绑定数据组件
    /// 挂载此组件的对象，其子物体可以添加组件绑定
    /// </summary>
    public class BindData : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// 目标脚本（编辑器用，用于生成 partial 类）
        /// </summary>
        [SerializeField]
        private UnityEditor.MonoScript _targetScript;

        public UnityEditor.MonoScript TargetScript
        {
            get => _targetScript;
            set => _targetScript = value;
        }
#endif

        [SerializeField]
        private List<Component> _bindings = new List<Component>();

        /// <summary>
        /// 获取绑定组件（按索引）
        /// </summary>
        public T Get<T>(int index) where T : Component
        {
            if (index >= 0 && index < _bindings.Count)
            {
                return _bindings[index] as T;
            }
            return null;
        }

        /// <summary>
        /// 获取 GameObject（按索引）
        /// </summary>
        public GameObject GetGameObject(int index)
        {
            if (index >= 0 && index < _bindings.Count && _bindings[index] != null)
            {
                return _bindings[index].gameObject;
            }
            return null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 获取所有绑定（编辑器用）
        /// </summary>
        public List<Component> GetBindings()
        {
            return _bindings;
        }

        /// <summary>
        /// 添加绑定（编辑器用）
        /// </summary>
        public void AddBinding(Component target)
        {
            if (target != null && !_bindings.Contains(target))
            {
                _bindings.Add(target);
            }
        }

        /// <summary>
        /// 移除绑定（编辑器用）
        /// </summary>
        public void RemoveBinding(Component target)
        {
            _bindings.Remove(target);
        }

        /// <summary>
        /// 移除绑定（按索引，编辑器用）
        /// </summary>
        public void RemoveBindingAt(int index)
        {
            if (index >= 0 && index < _bindings.Count)
            {
                _bindings.RemoveAt(index);
            }
        }

        /// <summary>
        /// 清空绑定（编辑器用）
        /// </summary>
        public void ClearBindings()
        {
            _bindings.Clear();
        }

        /// <summary>
        /// 是否包含绑定
        /// </summary>
        public bool HasBinding(Component target)
        {
            return _bindings.Contains(target);
        }

        /// <summary>
        /// 绑定数量
        /// </summary>
        public int BindingCount => _bindings.Count;
#endif
    }
}
