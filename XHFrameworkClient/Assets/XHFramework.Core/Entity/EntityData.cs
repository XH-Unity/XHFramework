using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 实体数据基类
/// </summary>
public abstract class EntityData : IReference
{
    #region 属性

    /// <summary>
    /// 实体编号
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 实体类型编号（用于从数据表里读取对应数据行）
    /// </summary>
    public int EntityTableID { get; set; }

    /// <summary>
    /// 实体位置
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// 实体旋转
    /// </summary>
    public Quaternion Rotation { get; set; }

    #endregion

    #region IReference 实现

    /// <summary>
    /// 清理实体数据
    /// </summary>
    public virtual void Clear()
    {
        Id = 0;
        EntityTableID = 0;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
    }

    #endregion
}

}
