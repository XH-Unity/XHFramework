using System;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 框架入口，维护所有模块管理器
    /// </summary>
    public static class FW
    {
        #region AOT层核心模块（发布后不可新增）
        public static ResourceManager ResourceManager { get; private set; }
        public static UIManager UIManager { get; private set; }
        public static ObjectPoolManager ObjectPoolManager { get; private set; }
        public static SceneManager SceneManager { get; private set; }
        public static DataTableManager DataTableManager { get; private set; }
        public static EventManager EventManager { get; private set; }
        public static DataNodeManager DataNodeManager { get; private set; }
        public static LocalizationManager LocalizationManager { get; private set; }
        public static AudioManager AudioManager { get; private set; }
        public static FsmManager FsmManager { get; private set; }
        public static EntityManager EntityManager { get; private set; }
        public static SettingManager SettingManager { get; private set; }
        public static NetworkManager NetworkManager { get; private set; }
        public static HttpManager HttpManager { get; private set; }
        public static PathFindingManager PathFindingManager { get; private set; }
        #endregion

        #region 热更层模块字典（支持动态注册）
        /// <summary>
        /// 模块类型到实例的映射（用于热更层动态获取模块）
        /// </summary>
        private static readonly Dictionary<Type, ManagerBase> m_ManagerDict = new Dictionary<Type, ManagerBase>();
        #endregion

        public static GameObject Root { get; private set; }

        public static void Init(GameObject root)
        {
            Root = root;
            ObjectPoolManager = RegisterManager<ObjectPoolManager>();
            EventManager = RegisterManager<EventManager>();
            SettingManager = RegisterManager<SettingManager>();
            DataNodeManager = RegisterManager<DataNodeManager>();
            FsmManager = RegisterManager<FsmManager>();
            ResourceManager = RegisterManager<ResourceManager>();
            DataTableManager = RegisterManager<DataTableManager>();
            LocalizationManager = RegisterManager<LocalizationManager>();
            AudioManager = RegisterManager<AudioManager>();
            NetworkManager = RegisterManager<NetworkManager>();
            HttpManager = RegisterManager<HttpManager>();
            UIManager = RegisterManager<UIManager>();
            EntityManager = RegisterManager<EntityManager>();
            SceneManager = RegisterManager<SceneManager>();
            PathFindingManager = RegisterManager<PathFindingManager>();
        }

        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            //轮询所有管理器
            foreach (ManagerBase manager in m_Managers)
            {
                manager.Update(elapseSeconds, realElapseSeconds);
            }
        }


        public static void Shutdown()
        {
            //关闭并清理所有管理器
            for (LinkedListNode<ManagerBase> current = m_Managers.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            m_Managers.Clear();
            m_ManagerDict.Clear();
            ReferencePool.ClearAll();
        }

        //------------------------------------------ManagerCenter--------------------------------
        /// <summary>
        /// 所有模块管理器的链表（按优先级排序，用于 Update 轮询）
        /// </summary>
        private static readonly LinkedList<ManagerBase> m_Managers = new LinkedList<ManagerBase>();

        #region 热更层公开接口

        /// <summary>
        /// 获取模块管理器（热更层使用）
        /// 用于获取 AOT 层或热更层注册的任意模块
        /// </summary>
        /// <typeparam name="T">模块类型</typeparam>
        /// <returns>模块实例，不存在则返回 null</returns>
        public static T GetManager<T>() where T : ManagerBase
        {
            Type managerType = typeof(T);
            if (m_ManagerDict.TryGetValue(managerType, out ManagerBase manager))
            {
                return (T)manager;
            }
            return null;
        }

        /// <summary>
        /// 检查模块是否已注册
        /// </summary>
        /// <typeparam name="T">模块类型</typeparam>
        /// <returns>是否已注册</returns>
        public static bool HasManager<T>() where T : ManagerBase
        {
            return m_ManagerDict.ContainsKey(typeof(T));
        }

        #endregion

        /// <summary>
        /// 创建模块管理器（内部使用）
        /// </summary>
        private static T RegisterManager<T>() where T : ManagerBase
        {
            Type managerType = typeof(T);

            // 检查是否已存在
            if (m_ManagerDict.TryGetValue(managerType, out ManagerBase existingManager))
            {
                return (T)existingManager;
            }

            // 创建新模块
            ManagerBase manager = (ManagerBase)Activator.CreateInstance(managerType);
            if (manager == null)
            {
                Debug.LogError("模块管理器创建失败：" + managerType.FullName);
                return null;
            }

            // 注册到字典
            m_ManagerDict[managerType] = manager;

            // 根据模块优先级决定它在链表里的位置
            LinkedListNode<ManagerBase> current = m_Managers.First;
            while (current != null)
            {
                if (manager.Priority > current.Value.Priority)
                {
                    break;
                }
                current = current.Next;
            }

            if (current != null)
            {
                m_Managers.AddBefore(current, manager);
            }
            else
            {
                m_Managers.AddLast(manager);
            }

            // 初始化模块管理器
            manager.Init();
            return (T)manager;
        }

        /// <summary>
        /// 移除模块管理器
        /// </summary>
        /// <typeparam name="T">要移除的管理器类型</typeparam>
        /// <returns>是否移除成功</returns>
        public static bool RemoveManager<T>() where T : ManagerBase
        {
            Type managerType = typeof(T);

            // 从字典移除
            m_ManagerDict.Remove(managerType);

            // 从链表移除
            for (LinkedListNode<ManagerBase> current = m_Managers.First; current != null; current = current.Next)
            {
                if (current.Value.GetType() == managerType)
                {
                    current.Value.Shutdown();
                    m_Managers.Remove(current);
                    return true;
                }
            }
            return false;
        }
    }
}