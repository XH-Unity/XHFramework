using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace XHFramework.Core {

/// <summary>
/// 实体管理器
/// </summary>
public class EntityManager : ManagerBase
{
    public override int Priority => 40;

    #region 字段

    private GameObject _gameObject;
    private Dictionary<int, Entity> _entities = new();
    private Dictionary<string, EntityGroup> _entityGroups = new();
    private Dictionary<int, int> _entitiesBeingLoaded = new();
    private HashSet<int> _entitiesToReleaseOnLoad = new();
    private LinkedList<Entity> _recycleQueue = new();
    private int _serial=0;

    #endregion

    #region 属性

    /// <summary>
    /// 实体数量
    /// </summary>
    public int EntityCount
    {
        get { return _entities.Count; }
    }

    /// <summary>
    /// 实体组数量
    /// </summary>
    public int EntityGroupCount
    {
        get { return _entityGroups.Count; }
    }

    #endregion
    

    #region 生命周期

    public override void Init()
    {
        _gameObject = new GameObject("EntityManager");
        _gameObject.transform.SetParent(FW.Root.transform);
        _gameObject.transform.localScale = Vector3.one;

        Log.Info("EntityManager初始化");
        // EntityGroup 的初始化移到 InitEntityGroups()，由热更层调用
    }

    /// <summary>
    /// 初始化实体组（由热更层调用）
    /// </summary>
    /// <param name="groupConfigs">实体组配置列表</param>
    public void InitEntityGroups(List<EntityGroupConfig> groupConfigs)
    {
        if (groupConfigs == null || groupConfigs.Count == 0)
        {
            Log.Warn("EntityGroupConfigs is null or empty");
            return;
        }

        for (int i = 0; i < groupConfigs.Count; i++)
        {
            EntityGroupConfig config = groupConfigs[i];
            if (!AddEntityGroup(config.Name, config.InstanceAutoReleaseInterval, config.InstanceCapacity, config.InstanceExpireTime))
            {
                Log.Warn("Add Entity group '{0}' failure.", config.Name);
            }
        }

        Log.Info("EntityManager 实体组初始化完成，共 {0} 个组", groupConfigs.Count);
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        // 回收需要回收的实体
        while (_recycleQueue.Count > 0)
        {
            Entity entity = _recycleQueue.First.Value;
            _recycleQueue.RemoveFirst();

            EntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Debug.LogError("要回收的实体的实体组为空");
                continue;
            }

            entity.Status = EntityStatus.WillRecycle;
            entity.Recycle();
            entity.Status = EntityStatus.Recycled;
            entityGroup.UnspawnEntity(entity);
        }

        // 更新所有实体组
        foreach (var pair in _entityGroups)
        {
            pair.Value.Update(elapseSeconds, realElapseSeconds);
        }
    }

    public override void Shutdown()
    {
        HideAllLoadedEntities();
        _entityGroups.Clear();
        _entitiesBeingLoaded.Clear();
        _entitiesToReleaseOnLoad.Clear();
        _recycleQueue.Clear();
    }

    #endregion

    #region 实体组操作

    /// <summary>
    /// 是否存在实体组
    /// </summary>
    /// <param name="entityGroupName">实体组名称</param>
    /// <returns>是否存在实体组</returns>
    public bool HasEntityGroup(string entityGroupName)
    {
        if (string.IsNullOrEmpty(entityGroupName))
        {
            Debug.LogError("要检查是否存在的实体组名称为空");
            return false;
        }
        return _entityGroups.ContainsKey(entityGroupName);
    }

    /// <summary>
    /// 获取实体组
    /// </summary>
    /// <param name="entityGroupName">实体组名称</param>
    /// <returns>要获取的实体组</returns>
    public EntityGroup GetEntityGroup(string entityGroupName)
    {
        if (string.IsNullOrEmpty(entityGroupName))
        {
            Debug.LogError("要获取的实体组名称为空");
            return null;
        }

        _entityGroups.TryGetValue(entityGroupName, out EntityGroup entityGroup);
        return entityGroup;
    }

    /// <summary>
    /// 获取所有实体组
    /// </summary>
    /// <returns>所有实体组</returns>
    public EntityGroup[] GetAllEntityGroups()
    {
        int index = 0;
        EntityGroup[] entityGroups = new EntityGroup[_entityGroups.Count];
        foreach (var pair in _entityGroups)
        {
            entityGroups[index++] = pair.Value;
        }
        return entityGroups;
    }

    /// <summary>
    /// 增加实体组
    /// </summary>
    /// <param name="entityGroupName">实体组名称</param>
    /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数</param>
    /// <param name="instanceCapacity">实体实例对象池容量</param>
    /// <param name="instanceExpireTime">实体实例对象池对象过期秒数</param>
    /// <returns>是否增加实体组成功</returns>
    public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval = 60f,
        int instanceCapacity = 16, float instanceExpireTime = 60f)
    {
        if (string.IsNullOrEmpty(entityGroupName))
        {
            Debug.LogError("要增加的实体组名称为空");
            return false;
        }

        if (HasEntityGroup(entityGroupName))
        {
            Debug.LogError("要增加的实体组已存在");
            return false;
        }

        // 创建实体组 GameObject
        GameObject groupGameObject = new GameObject($"Entity Group - {entityGroupName}");
        groupGameObject.transform.SetParent(_gameObject.transform);

        // 创建实体组并放入字典
        _entityGroups.Add(entityGroupName,
            new EntityGroup(entityGroupName, groupGameObject, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime));

        return true;
    }

    #endregion

    #region 检查实体

    /// <summary>
    /// 是否存在实体
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <returns>是否存在实体</returns>
    public bool HasEntity(int entityId)
    {
        return _entities.ContainsKey(entityId);
    }

    /// <summary>
    /// 是否存在实体
    /// </summary>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <returns>是否存在实体</returns>
    public bool HasEntity(string entityAssetName)
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("要检查是否存在的实体资源名称为空");
            return false;
        }

        foreach (var pair in _entities)
        {
            if (pair.Value.EntityAssetName == entityAssetName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否正在加载实体
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <returns>是否正在加载实体</returns>
    public bool IsLoadingEntity(int entityId)
    {
        return _entitiesBeingLoaded.ContainsKey(entityId);
    }

    #endregion

    #region 获取实体

    /// <summary>
    /// 获取实体
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <returns>要获取的实体</returns>
    public Entity GetEntity(int entityId)
    {
        _entities.TryGetValue(entityId, out Entity entity);
        return entity;
    }

    /// <summary>
    /// 获取实体
    /// </summary>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <returns>要获取的实体</returns>
    public Entity GetEntity(string entityAssetName)
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("要获取实体的资源名称为空");
            return null;
        }

        foreach (var pair in _entities)
        {
            if (pair.Value.EntityAssetName == entityAssetName)
            {
                return pair.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据资源名称获取所有实体
    /// </summary>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <returns>要获取的实体数组</returns>
    public Entity[] GetEntities(string entityAssetName)
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("要获取实体的资源名称为空");
            return null;
        }

        List<Entity> entities = new List<Entity>();
        foreach (var pair in _entities)
        {
            if (pair.Value.EntityAssetName == entityAssetName)
            {
                entities.Add(pair.Value);
            }
        }
        return entities.ToArray();
    }

    /// <summary>
    /// 根据实体类型编号获取所有实体
    /// </summary>
    /// <param name="entityTypeId">实体类型编号</param>
    /// <returns>该类型的所有实体</returns>
    public Entity[] GetEntitiesByTypeId(int entityTypeId)
    {
        List<Entity> entities = new List<Entity>();
        foreach (var pair in _entities)
        {
            if (pair.Value.EntityTypeId == entityTypeId)
            {
                entities.Add(pair.Value);
            }
        }
        return entities.ToArray();
    }

    /// <summary>
    /// 获取所有已加载的实体
    /// </summary>
    /// <returns>所有已加载的实体</returns>
    public Entity[] GetAllLoadedEntities()
    {
        int index = 0;
        Entity[] entities = new Entity[_entities.Count];
        foreach (var pair in _entities)
        {
            entities[index++] = pair.Value;
        }
        return entities;
    }

    /// <summary>
    /// 获取所有正在加载实体的编号
    /// </summary>
    /// <returns>所有正在加载实体的编号</returns>
    public int[] GetAllLoadingEntityIds()
    {
        int index = 0;
        int[] entitiesBeingLoaded = new int[_entitiesBeingLoaded.Count];
        foreach (var pair in _entitiesBeingLoaded)
        {
            entitiesBeingLoaded[index++] = pair.Key;
        }
        return entitiesBeingLoaded;
    }

    #endregion

    #region 父子实体关系

    /// <summary>
    /// 获取父实体
    /// </summary>
    /// <param name="childEntityId">要获取父实体的子实体的实体编号</param>
    /// <returns>子实体的父实体</returns>
    public Entity GetParentEntity(int childEntityId)
    {
        Entity childEntity = GetEntity(childEntityId);
        if (childEntity == null)
        {
            Debug.LogError("要获取父实体的子实体的实体信息为空：" + childEntityId);
            return null;
        }
        return childEntity.ParentEntity;
    }

    /// <summary>
    /// 获取子实体
    /// </summary>
    /// <param name="parentEntityId">要获取子实体的父实体的实体编号</param>
    /// <returns>子实体数组</returns>
    public Entity[] GetChildEntities(int parentEntityId)
    {
        Entity parentEntity = GetEntity(parentEntityId);
        if (parentEntity == null)
        {
            Debug.LogError("要获取子实体的父实体的实体信息为空：" + parentEntityId);
            return null;
        }
        return parentEntity.GetChildEntities();
    }

    /// <summary>
    /// 附加子实体
    /// </summary>
    /// <param name="childEntityId">要附加的子实体的实体编号</param>
    /// <param name="parentEntityId">被附加的父实体的实体编号</param>
    /// <param name="attachTransform">附加点 Transform</param>
    /// <param name="userData">用户自定义数据</param>
    public void AttachEntity(int childEntityId, int parentEntityId, Transform attachTransform, object userData = null)
    {
        Entity childEntity = GetEntity(childEntityId);
        if (childEntity == null)
        {
            Debug.LogError("要附加的子实体的实体信息为空：" + childEntityId);
            return;
        }

        if (childEntity.Status >= EntityStatus.WillHide)
        {
            Debug.LogError("要附加的子实体状态不合法，无法附加：" + childEntity.Status);
            return;
        }

        Entity parentEntity = GetEntity(parentEntityId);
        if (parentEntity == null)
        {
            Debug.LogError("被附加的父实体的实体信息为空：" + parentEntityId);
            return;
        }

        if (parentEntity.Status >= EntityStatus.WillHide)
        {
            Debug.LogError("被附加的父实体状态不合法，无法附加：" + parentEntity.Status);
            return;
        }

        // 解除原来的父实体
        DetachEntity(childEntity.Id, userData);

        // 附加新的父实体
        childEntity.ParentEntity = parentEntity;
        childEntity.AttachTo(parentEntity, attachTransform, userData);

        // 父实体添加子实体
        parentEntity.AddChildEntity(childEntity);
        parentEntity.Attached(childEntity, attachTransform, userData);
    }

    /// <summary>
    /// 解除父实体
    /// </summary>
    /// <param name="childEntityId">要解除父实体的子实体的实体编号</param>
    /// <param name="userData">用户自定义数据</param>
    public void DetachEntity(int childEntityId, object userData = null)
    {
        Entity childEntity = GetEntity(childEntityId);
        if (childEntity == null)
        {
            Debug.LogError("要解除的子实体的实体信息为空：" + childEntityId);
            return;
        }

        Entity parentEntity = childEntity.ParentEntity;
        if (parentEntity == null)
        {
            return;
        }

        // 子实体解除父实体
        childEntity.ParentEntity = null;
        childEntity.DetachFrom(parentEntity, userData);

        // 父实体解除子实体
        parentEntity.RemoveChildEntity(childEntity);
        parentEntity.Detached(childEntity, userData);
    }

    /// <summary>
    /// 解除所有子实体
    /// </summary>
    /// <param name="parentEntityId">被解除的父实体的实体编号</param>
    /// <param name="userData">用户自定义数据</param>
    public void DetachChildEntities(int parentEntityId, object userData = null)
    {
        Entity parentEntity = GetEntity(parentEntityId);
        if (parentEntity == null)
        {
            Debug.LogError("要解除所有子实体的父实体的实体信息为空：" + parentEntityId);
            return;
        }

        Entity[] childEntities = parentEntity.GetChildEntities();
        foreach (Entity childEntity in childEntities)
        {
            DetachEntity(childEntity.Id, userData);
        }
    }

    #endregion

    #region 显示实体

    /// <summary>
    /// 显示实体
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="entityId">实体编号</param>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <param name="entityGroupName">实体组名称</param>
    /// <param name="priority">加载优先级</param>
    /// <param name="userData">用户自定义数据</param>
    /// <returns>显示的实体</returns>
    public async UniTask<Entity> ShowEntity<T>(int entityId, string entityAssetName, string entityGroupName, uint priority, object userData)
        where T : Entity, new()
    {
        if (string.IsNullOrEmpty(entityAssetName))
        {
            Debug.LogError("显示实体时的实体资源名称为空");
            return null;
        }

        if (string.IsNullOrEmpty(entityGroupName))
        {
            Debug.LogError("显示实体时的实体组名称为空");
            return null;
        }

        if (_entities.ContainsKey(entityId))
        {
            Debug.LogError("显示实体时的实体信息已存在");
            return null;
        }

        if (IsLoadingEntity(entityId))
        {
            Debug.LogError("要显示的实体已在加载");
            return null;
        }

        EntityGroup entityGroup = GetEntityGroup(entityGroupName);
        if (entityGroup == null)
        {
            Debug.LogError("显示实体时的实体组不存在");
            return null;
        }

        // 尝试从对象池获取实体实例
        EntityInstanceObject entityInstanceObject = entityGroup.SpawnEntityInstanceObject(entityAssetName);
        if (entityInstanceObject == null)
        {
            // 没获取到就加载该实体
            int serialId = _serial++;
            _entitiesBeingLoaded.Add(entityId, serialId);

            GameObject instance = await FW.ResourceManager.LoadGameObjectAsync(entityAssetName, priority);
            if (instance != null)
            {
                _entitiesBeingLoaded.Remove(entityId);

                if (_entitiesToReleaseOnLoad.Contains(serialId))
                {
                    Debug.LogError($"需要释放的实体：{entityId}(id：{serialId})加载成功");
                    _entitiesToReleaseOnLoad.Remove(serialId);
                    FW.ResourceManager.ReleaseGameObject(instance);
                    return null;
                }

                // 实例化实体，并将实体实例对象放入对象池
                entityInstanceObject = EntityInstanceObject.Create(instance, entityAssetName);
                entityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);

                return ShowEntityInternal<T>(entityId, entityAssetName, entityGroup, entityInstanceObject, true, userData);
            }
            else
            {
                _entitiesBeingLoaded.Remove(entityId);
                _entitiesToReleaseOnLoad.Remove(serialId);
                Log.Error("加载实体：{0} 失败", entityAssetName);
                return null;
            }
        }

        return ShowEntityInternal<T>(entityId, entityAssetName, entityGroup, entityInstanceObject, false, userData);
    }

    /// <summary>
    /// 显示实体内部方法
    /// </summary>
    private Entity ShowEntityInternal<T>(int entityId, string entityAssetName, EntityGroup entityGroup,
        EntityInstanceObject entityInstanceObject, bool isNewInstance, object userData) where T : Entity, new()
    {
        GameObject gameObject = entityInstanceObject.Target as GameObject;
        gameObject.transform.SetParent(entityGroup.GetGameObject().transform);

        // 获取或创建实体脚本（使用Unity原生组件系统）
        Entity entity = gameObject.GetComponent<T>();
        if (entity == null)
        {
            entity = gameObject.AddComponent<T>();
        }
        _entities.Add(entityId, entity);

        entity.Status = EntityStatus.WillInit;
        entity.Init(entityId, entityAssetName, entityGroup, isNewInstance, userData);
        entity.Status = EntityStatus.Inited;

        entityGroup.AddEntity(entity);

        entity.Status = EntityStatus.WillShow;
        entity.Show(userData);
        entity.Status = EntityStatus.Showed;

        return entity;
    }

    #endregion

    #region 隐藏实体

    /// <summary>
    /// 隐藏实体
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <param name="userData">用户自定义数据</param>
    public void HideEntity(int entityId, object userData = null)
    {
        // 要隐藏的实体正在加载，就直接释放
        if (IsLoadingEntity(entityId))
        {
            if (_entitiesBeingLoaded.TryGetValue(entityId, out int serialId))
            {
                _entitiesToReleaseOnLoad.Add(serialId);
            }
            else
            {
                Debug.LogError("没找到实体：" + entityId);
            }
            _entitiesBeingLoaded.Remove(entityId);
            return;
        }

        Entity entity = GetEntity(entityId);
        if (entity == null)
        {
            Debug.LogError("获取要隐藏的实体的实体信息为空：" + entityId);
            return;
        }

        // 使用迭代方式隐藏所有子实体，避免深层递归导致栈溢出
        Stack<Entity> entitiesToHide = new Stack<Entity>();
        entitiesToHide.Push(entity);

        // 收集所有需要隐藏的实体
        List<Entity> hideOrder = new List<Entity>();
        while (entitiesToHide.Count > 0)
        {
            Entity current = entitiesToHide.Pop();
            hideOrder.Add(current);

            Entity[] childEntities = current.GetChildEntities();
            foreach (Entity childEntity in childEntities)
            {
                entitiesToHide.Push(childEntity);
            }
        }

        // 从后往前遍历，确保先隐藏子实体再隐藏父实体
        for (int i = hideOrder.Count - 1; i >= 0; i--)
        {
            HideEntityInternal(hideOrder[i], userData);
        }
    }

    /// <summary>
    /// 隐藏实体内部方法
    /// </summary>
    private void HideEntityInternal(Entity entity, object userData)
    {
        // 解除自身与父实体的关系
        DetachEntity(entity.Id, userData);

        // 隐藏实体
        entity.Status = EntityStatus.WillHide;
        entity.Hide(userData);
        entity.Status = EntityStatus.Hidden;

        // 将隐藏的实体从实体组与实体信息字典中移除
        EntityGroup entityGroup = entity.EntityGroup;
        if (entityGroup == null)
        {
            Debug.LogError("隐藏的实体的实体组为空");
        }
        else
        {
            entityGroup.RemoveEntity(entity);
        }

        if (!_entities.Remove(entity.Id))
        {
            Debug.LogError("将隐藏的实体从实体信息字典中移除失败");
        }

        // 将隐藏的实体加入回收队列
        _recycleQueue.AddLast(entity);
    }

    /// <summary>
    /// 隐藏所有已加载的实体
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    public void HideAllLoadedEntities(object userData = null)
    {
        var entityIds = _entities.Keys.ToList();
        foreach (var id in entityIds)
        {
            HideEntity(id, userData);
        }
    }

    /// <summary>
    /// 隐藏所有正在加载的实体
    /// </summary>
    public void HideAllLoadingEntities()
    {
        foreach (var pair in _entitiesBeingLoaded)
        {
            _entitiesToReleaseOnLoad.Add(pair.Value);
        }
        _entitiesBeingLoaded.Clear();
    }

    #endregion
}

}
