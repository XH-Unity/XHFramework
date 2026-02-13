using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 实体组
/// </summary>
public class EntityGroup
{
    #region 字段

    private GameObject _gameObject;
    private ObjectPool<EntityInstanceObject> _instancePool;
    private LinkedList<Entity> _entities;

    #endregion

    #region 属性

    /// <summary>
    /// 实体组名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 获取实体组中实体数量
    /// </summary>
    public int EntityCount
    {
        get { return _entities.Count; }
    }

    /// <summary>
    /// 实体组实例对象池自动释放可释放对象的间隔秒数
    /// </summary>
    public float InstanceAutoReleaseInterval
    {
        get { return _instancePool.AutoReleaseInterval; }
        set { _instancePool.AutoReleaseInterval = value; }
    }

    /// <summary>
    /// 实体组实例对象池的容量
    /// </summary>
    public int InstanceCapacity
    {
        get { return _instancePool.Capacity; }
        set { _instancePool.Capacity = value; }
    }

    /// <summary>
    /// 获取或设置实体组实例对象池对象过期秒数
    /// </summary>
    public float InstanceExpireTime
    {
        get { return _instancePool.ExpireTime; }
        set { _instancePool.ExpireTime = value; }
    }

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化实体组的新实例
    /// </summary>
    /// <param name="name">实体组名称</param>
    /// <param name="gameObject">实体组 GameObject</param>
    /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数</param>
    /// <param name="instanceCapacity">实体实例对象池容量</param>
    /// <param name="instanceExpireTime">实体实例对象池对象过期秒数</param>
    public EntityGroup(string name, GameObject gameObject, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime)
    {
        Name = name;
        _gameObject = gameObject;
        _instancePool = FW.ObjectPoolManager.CreateObjectPool<EntityInstanceObject>(instanceCapacity, instanceExpireTime);
        _instancePool.AutoReleaseInterval = instanceAutoReleaseInterval;
        _entities = new LinkedList<Entity>();
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 获取实体组 GameObject
    /// </summary>
    /// <returns>实体组 GameObject</returns>
    public GameObject GetGameObject()
    {
        return _gameObject;
    }

    /// <summary>
    /// 实体组轮询
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        LinkedListNode<Entity> current = _entities.First;
        while (current != null)
        {
            LinkedListNode<Entity> next = current.Next;
            current.Value.UpdateEntity(elapseSeconds, realElapseSeconds);
            current = next;
        }
    }

    #endregion

    #region 检查实体

    /// <summary>
    /// 实体组中是否存在实体
    /// </summary>
    /// <param name="entityId">实体序列编号</param>
    /// <returns>实体组中是否存在实体</returns>
    public bool HasEntity(int entityId)
    {
        foreach (Entity entity in _entities)
        {
            if (entity.Id == entityId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 实体组中是否存在实体
    /// </summary>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <returns>实体组中是否存在实体</returns>
    public bool HasEntity(string entityAssetName)
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("实体资源名称为空，无法检查实体是否存在");
            return false;
        }

        foreach (Entity entity in _entities)
        {
            if (entity.EntityAssetName == entityAssetName)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region 获取实体

    /// <summary>
    /// 从实体组中获取实体
    /// </summary>
    /// <param name="entityId">实体序列编号</param>
    /// <returns>要获取的实体</returns>
    public Entity GetEntity(int entityId)
    {
        foreach (Entity entity in _entities)
        {
            if (entity.Id == entityId)
            {
                return entity;
            }
        }
        return null;
    }

    /// <summary>
    /// 从实体组中获取实体
    /// </summary>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <returns>要获取的实体</returns>
    public Entity GetEntity(string entityAssetName)
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("实体资源名称为空，无法获取实体");
            return null;
        }

        foreach (Entity entity in _entities)
        {
            if (entity.EntityAssetName == entityAssetName)
            {
                return entity;
            }
        }
        return null;
    }

    /// <summary>
    /// 从实体组中获取实体
    /// </summary>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <returns>要获取的实体数组</returns>
    public Entity[] GetEntities(string entityAssetName)
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("实体资源名称为空，无法获取实体");
            return null;
        }

        List<Entity> entities = new List<Entity>();
        foreach (Entity entity in _entities)
        {
            if (entity.EntityAssetName == entityAssetName)
            {
                entities.Add(entity);
            }
        }
        return entities.ToArray();
    }

    /// <summary>
    /// 从实体组中获取所有实体
    /// </summary>
    /// <returns>实体组中的所有实体</returns>
    public Entity[] GetAllEntities()
    {
        List<Entity> entities = new List<Entity>();
        foreach (Entity entity in _entities)
        {
            entities.Add(entity);
        }
        return entities.ToArray();
    }

    #endregion

    #region 增加与移除实体

    /// <summary>
    /// 往实体组增加实体
    /// </summary>
    /// <param name="entity">要增加的实体</param>
    public void AddEntity(Entity entity)
    {
        _entities.AddLast(entity);
    }

    /// <summary>
    /// 从实体组移除实体
    /// </summary>
    /// <param name="entity">要移除的实体</param>
    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    #endregion

    #region 对象池操作

    /// <summary>
    /// 注册实体实例对象到对象池
    /// </summary>
    /// <param name="obj">实体实例对象</param>
    /// <param name="spawned">是否已生成</param>
    public void RegisterEntityInstanceObject(EntityInstanceObject obj, bool spawned = false)
    {
        _instancePool.Register(obj, spawned);
    }

    /// <summary>
    /// 从对象池获取实体实例对象
    /// </summary>
    /// <param name="name">实体资源名称</param>
    /// <returns>实体实例对象</returns>
    public EntityInstanceObject SpawnEntityInstanceObject(string name = "")
    {
        return _instancePool.Spawn(name);
    }

    /// <summary>
    /// 回收实体实例对象到对象池
    /// </summary>
    /// <param name="entity">要回收的实体</param>
    public void UnspawnEntity(Entity entity)
    {
        _instancePool.Unspawn(entity.GetGameObject());
    }

    #endregion
}

}
