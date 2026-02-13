using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TextAsset = UnityEngine.TextAsset;
using ByteBuf = Luban.ByteBuf;

namespace XHFramework.Core {

/// <summary>
/// 数据表基类接口
/// </summary>
public interface TableBase
{
    void LoadData(ByteBuf _buf);

    void Cleardata();
}

public class DataTableManager : ManagerBase
{
    public override int Priority => 130;

    // 所有数据表的存储（按类型索引）
    private readonly Dictionary<Type, TableBase> _tables = new Dictionary<Type, TableBase>();

    // 配置字节数据缓存
    private readonly Dictionary<string, byte[]> _dataTableBytesCache = new Dictionary<string, byte[]>();

    // 配置加载状态
    private bool IsLoaded { get; set; }

    public override void Init()
    {

    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        // 更新逻辑
    }

    public override void Shutdown()
    {
        ClearDataTableData();
    }
    
    /// <summary>
    /// 获取指定类型的数据表
    /// </summary>
    public T GetTable<T>() where T : TableBase
    {
        if (_tables.TryGetValue(typeof(T), out TableBase table))
        {
            return (T)table;
        }
        return default;
    }

    /// <summary>
    /// 获取配置表的原始字节数据
    /// </summary>
    /// <param name="tableName">表名（不含路径和扩展名）</param>
    /// <returns>字节数据，未找到返回null</returns>
    public byte[] GetTableBytes(string tableName)
    {
        if (_dataTableBytesCache.TryGetValue(tableName, out byte[] bytes))
        {
            return bytes;
        }
        return null;
    }

    /// <summary>
    /// 加载指定数据表
    /// </summary>
    public bool LoadTable<T>(string tableName) where T : TableBase, new()
    {
        if (!_dataTableBytesCache.TryGetValue(tableName, out byte[] bytes))
        {
            Log.Error($"数据表字节数据未找到: {tableName}");
            return false;
        }

        try
        {
            T table = new T();
            table.LoadData(new ByteBuf(bytes));
            _tables[typeof(T)] = table;
            Log.Info($"数据表加载成功: {typeof(T).Name}");
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"数据表加载失败: {typeof(T).Name}, {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 通过标签加载所有配置表字节数据
    /// </summary>
    public async UniTask LoadAllDataTablesByTagAsync(uint priority)
    {
        if (IsLoaded)
        {
            Debug.LogWarning("配置表已经加载过了！");
            return;
        }

        var assets = await FW.ResourceManager.LoadAssetsByTagAndReleaseAsync<TextAsset>("DataTable", priority);

        if (assets.Count == 0)
        {
            Log.Error("未找到任何DataTable文件");
            return;
        }

        foreach (var kvp in assets)
        {
            TextAsset textAsset = kvp.Value;
            if (textAsset != null)
            {
                string dataTableName = textAsset.name;
                _dataTableBytesCache[dataTableName] = textAsset.bytes;
                Log.Info($"配置表加载成功: {dataTableName} (大小: {textAsset.bytes.Length} 字节)");
            }
        }

        IsLoaded = true;
        Log.Info($"加载DataTable成功共 {assets.Count} 个DataTable文件");
    }


    /// <summary>
    /// 清除配置数据
    /// </summary>
    private void ClearDataTableData()
    {
        // 清除所有表数据
        foreach (var table in _tables.Values)
        {
            table.Cleardata();
        }
        _tables.Clear();
        _dataTableBytesCache.Clear();
        IsLoaded = false;
    }


}

}
