using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 数据结点
/// </summary>
public class DataNode
{

    public static readonly DataNode[] EmptyArray = new DataNode[] { };

    /// <summary>
    /// 结点名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 结点全名
    /// </summary>
    public string FullName => Parent == null ? Name : string.Format("{0}{1}{2}", Parent.FullName, DataNodeManager.PathSplit[0], Name);

    /// <summary>
    /// 结点数据
    /// </summary>
    private object _data;


    /// <summary>
    /// 父结点
    /// </summary>
    public DataNode Parent { get; private set; }

    /// <summary>
    /// 子结点
    /// </summary>
    private List<DataNode> _childs;

    /// <summary>
    /// 子结点数量
    /// </summary>
    public int ChildCount => _childs != null ? _childs.Count : 0;

    public DataNode(string name, DataNode parent)
    {
        if (!IsValidName(name))
        {
            Debug.LogError("数据结点名字不合法：" + name);
        }

        Name = name;
        _data = null;
        Parent = parent;
        _childs = null;
    }

    /// <summary>
    /// 检测数据结点名称是否合法。
    /// </summary>
    /// <param name="name">要检测的数据节点名称。</param>
    /// <returns>是否是合法的数据结点名称。</returns>
    private static bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        foreach (string pathSplit in DataNodeManager.PathSplit)
        {
            if (name.Contains(pathSplit))
            {
                return false;
            }
        }

        return true;
    }

    #region 结点数据操作
    /// <summary>
    /// 获取结点数据
    /// </summary>
    public T GetData<T>()
    {
        return (T)_data;
    }

    /// <summary>
    /// 设置结点数据
    /// </summary>
    public void SetData(object data)
    {
        _data = data;
    }
    #endregion

    #region 子结点操作

    #region 获取
    /// <summary>
    /// 根据索引获取子数据结点。
    /// </summary>
    /// <param name="index">子数据结点的索引。</param>
    /// <returns>指定索引的子数据结点，如果索引越界，则返回空。</returns>
    public DataNode GetChild(int index)
    {
        return index >= ChildCount ? null : _childs[index];
    }

    /// <summary>
    /// 根据名称获取子数据结点。
    /// </summary>
    /// <param name="name">子数据结点名称。</param>
    /// <returns>指定名称的子数据结点，如果没有找到，则返回空。</returns>
    public DataNode GetChild(string name)
    {
        if (!IsValidName(name))
        {
            Debug.LogError("子结点名称不合法，无法获取");
            return null;
        }

        if (_childs == null)
        {
            return null;
        }

        foreach (DataNode child in _childs)
        {
            if (child.Name == name)
            {
                return child;
            }
        }

        return null;
    }

    /// <summary>
    /// 根据名称获取或增加子数据结点。
    /// </summary>
    /// <param name="name">子数据结点名称。</param>
    /// <returns>指定名称的子数据结点，如果对应名称的子数据结点已存在，则返回已存在的子数据结点，否则增加子数据结点。</returns>
    public DataNode GetOrAddChild(string name)
    {
        DataNode node = GetChild(name);
        if (node != null)
        {
            return node;
        }

        node = new DataNode(name, this);

        if (_childs == null)
        {
            _childs = new List<DataNode>();
        }

        _childs.Add(node);

        return node;
    }
    #endregion

    #region 移除
    /// <summary>
    /// 根据索引移除子数据结点。
    /// </summary>
    /// <param name="index">子数据结点的索引位置。</param>
    public void RemoveChild(int index)
    {
        DataNode node = GetChild(index);
        if (node == null)
        {
            return;
        }

        node.Clear();
        _childs.Remove(node);
    }

    /// <summary>
    /// 根据名称移除子数据结点。
    /// </summary>
    /// <param name="name">子数据结点名称。</param>
    public void RemoveChild(string name)
    {
        DataNode node = GetChild(name);
        if (node == null)
        {
            return;
        }

        node.Clear();
        _childs.Remove(node);
    }

    /// <summary>
    /// 移除当前数据结点的数据和所有子数据结点。
    /// </summary>
    public void Clear()
    {
        _data = null;
        if (_childs != null)
        {
            foreach (DataNode child in _childs)
            {
                child.Clear();
            }

            _childs.Clear();
        }
    }

    #endregion

    #endregion


}

}

