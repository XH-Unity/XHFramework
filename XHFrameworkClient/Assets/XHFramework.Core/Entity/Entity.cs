using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 实体基类
/// 继承 MonoBehaviour，支持热更新（HybridCLR + YooAsset）
/// </summary>
public class Entity : MonoBehaviour
{
    #region 字段

    private static readonly Entity[] EmptyArray = new Entity[] { };

    private Transform _originalTransform;
    private EntityData _entityData;
    private List<Entity> _childEntities;

    #endregion

    #region 属性

    /// <summary>
    /// 实体编号
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// 实体类型编号（用于快速筛选和分类实体）
    /// </summary>
    public int EntityTypeId { get; private set; }

    /// <summary>
    /// 实体资源名称
    /// </summary>
    public string EntityAssetName { get; private set; }

    /// <summary>
    /// 实体状态
    /// </summary>
    public EntityStatus Status { get; set; }

    /// <summary>
    /// 实体所属的实体组
    /// </summary>
    public EntityGroup EntityGroup { get; private set; }

    /// <summary>
    /// 父实体
    /// </summary>
    public Entity ParentEntity { get; set; }

    /// <summary>
    /// 获取实体 GameObject 实例
    /// </summary>
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #endregion

    #region 构造函数

    public Entity()
    {
        Status = EntityStatus.WillInit;
        ParentEntity = null;
        _childEntities = null;
    }

    #endregion

    #region 生命周期方法

    /// <summary>
    /// 实体初始化
    /// </summary>
    /// <param name="entityId">实体编号</param>
    /// <param name="entityAssetName">实体资源名称</param>
    /// <param name="entityGroup">实体所属的实体组</param>
    /// <param name="isNewInstance">是否是新实例</param>
    /// <param name="userData">用户自定义数据</param>
    public void Init(int entityId, string entityAssetName, EntityGroup entityGroup, bool isNewInstance, object userData)
    {
        Id = entityId;
        EntityAssetName = entityAssetName;

        if (!isNewInstance)
            return;

        EntityGroup = entityGroup;
        _originalTransform = transform.parent;

        OnInit(userData);
    }

    /// <summary>
    /// 实体显示
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    public void Show(object userData)
    {
        gameObject.SetActive(true);
        _entityData = userData as EntityData;
        if (_entityData == null)
        {
            Log.Error("Entity data is invalid.");
            return;
        }

        EntityTypeId = _entityData.EntityTableID;
        transform.localPosition = _entityData.Position;
        transform.localRotation = _entityData.Rotation;
        transform.localScale = Vector3.one;

        OnShow(userData);
    }

    /// <summary>
    /// 实体隐藏
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    public void Hide(object userData)
    {
        OnHide(userData);
    }

    /// <summary>
    /// 实体回收
    /// </summary>
    public void Recycle()
    {
        Id = 0;
        EntityTypeId = 0;
    }

    /// <summary>
    /// 实体轮询
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
    public void UpdateEntity(float elapseSeconds, float realElapseSeconds)
    {
        OnUpdate(elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    /// 清理实体引用（实体回收时调用）
    /// </summary>
    public void Clear()
    {
        Id = 0;
        EntityTypeId = 0;
        EntityAssetName = null;
        Status = EntityStatus.WillInit;

        ReferencePool.Release(_entityData);
        _entityData = null;
        _originalTransform = null;
        EntityGroup = null;
        ParentEntity = null;

        if (_childEntities != null)
        {
            _childEntities.Clear();
            _childEntities = null;
        }
    }

    #endregion

    #region 父子实体关系

    /// <summary>
    /// 获取所有子实体
    /// </summary>
    /// <returns>子实体数组</returns>
    public Entity[] GetChildEntities()
    {
        if (_childEntities == null)
        {
            return EmptyArray;
        }
        return _childEntities.ToArray();
    }

    /// <summary>
    /// 添加子实体
    /// </summary>
    /// <param name="childEntity">要添加的子实体</param>
    public void AddChildEntity(Entity childEntity)
    {
        if (_childEntities == null)
        {
            _childEntities = new List<Entity>();
        }

        if (_childEntities.Contains(childEntity))
        {
            Debug.LogError("要添加的子实体已存在");
            return;
        }

        _childEntities.Add(childEntity);
    }

    /// <summary>
    /// 删除子实体
    /// </summary>
    /// <param name="childEntity">要删除的子实体</param>
    public void RemoveChildEntity(Entity childEntity)
    {
        if (_childEntities == null || !_childEntities.Remove(childEntity))
        {
            Debug.LogError("删除子实体失败");
        }
    }

    /// <summary>
    /// 实体附加到父实体
    /// </summary>
    /// <param name="parentEntity">被附加的父实体</param>
    /// <param name="attachTransform">附加点 Transform</param>
    /// <param name="userData">用户自定义数据</param>
    public void AttachTo(Entity parentEntity, Transform attachTransform, object userData)
    {
        transform.SetParent(attachTransform);
        OnAttachTo(parentEntity, attachTransform, userData);
    }

    /// <summary>
    /// 实体解除父实体
    /// </summary>
    /// <param name="parentEntity">被解除的父实体</param>
    /// <param name="userData">用户自定义数据</param>
    public void DetachFrom(Entity parentEntity, object userData)
    {
        OnDetachFrom(parentEntity, userData);
    }

    /// <summary>
    /// 实体附加子实体回调
    /// </summary>
    /// <param name="childEntity">附加的子实体</param>
    /// <param name="attachTransform">附加点 Transform</param>
    /// <param name="userData">用户自定义数据</param>
    public void Attached(Entity childEntity, Transform attachTransform, object userData)
    {
        OnAttached(childEntity, attachTransform, userData);
    }

    /// <summary>
    /// 实体解除子实体回调
    /// </summary>
    /// <param name="childEntity">解除的子实体</param>
    /// <param name="userData">用户自定义数据</param>
    public void Detached(Entity childEntity, object userData)
    {
        OnDetached(childEntity, userData);
    }

    #endregion

    #region 虚方法（供子类重写）

    /// <summary>
    /// 实体初始化回调
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnInit(object userData)
    {
    }

    /// <summary>
    /// 实体显示回调
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnShow(object userData)
    {
    }

    /// <summary>
    /// 实体隐藏回调
    /// </summary>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnHide(object userData)
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 实体轮询回调
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
    protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    /// 实体附加到父实体回调
    /// </summary>
    /// <param name="parentEntity">被附加的父实体</param>
    /// <param name="attachTransform">附加点 Transform</param>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnAttachTo(Entity parentEntity, Transform attachTransform, object userData)
    {
    }

    /// <summary>
    /// 实体解除父实体回调
    /// </summary>
    /// <param name="parentEntity">被解除的父实体</param>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnDetachFrom(Entity parentEntity, object userData)
    {
        transform.SetParent(_originalTransform);
    }

    /// <summary>
    /// 实体附加子实体回调
    /// </summary>
    /// <param name="childEntity">附加的子实体</param>
    /// <param name="attachTransform">附加点 Transform</param>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnAttached(Entity childEntity, Transform attachTransform, object userData)
    {
    }

    /// <summary>
    /// 实体解除子实体回调
    /// </summary>
    /// <param name="childEntity">解除的子实体</param>
    /// <param name="userData">用户自定义数据</param>
    protected virtual void OnDetached(Entity childEntity, object userData)
    {
    }

    #endregion
}

}
