# Framework.ReferencePool 引用池系统模块

## 概述

Framework.ReferencePool 提供了轻量级的引用池系统，专门用于管理实现了IReference接口的对象。通过对象复用减少GC压力，特别适用于频繁创建销毁的小对象。

## 模块结构

```
Framework/ReferencePool/
├── IReference.cs           # 引用接口
├── ReferencePool.cs        # 引用池管理器
├── ReferenceCollection.cs  # 引用集合
└── README.md              # 说明文档
```

## 核心功能

### 1. 引用对象定义

```csharp
// 定义可引用池化的对象
public class PlayerData : IReference
{
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public Vector3 Position { get; set; }
    
    public void Clear()
    {
        PlayerName = string.Empty;
        Level = 0;
        Experience = 0;
        Position = Vector3.zero;
    }
}

// 网络消息对象
public class NetworkMessage : IReference
{
    public int MessageId { get; set; }
    public byte[] Data { get; set; }
    public int Length { get; set; }
    
    public void Clear()
    {
        MessageId = 0;
        Data = null;
        Length = 0;
    }
}

// 事件参数对象（结合事件系统使用）
public class ItemPickupEventArgs : EventArgs, IReference
{
    public override int EventId => GetHashCode();
    
    public int ItemId { get; set; }
    public int PlayerId { get; set; }
    public int Quantity { get; set; }
    
    public override void Clear()
    {
        ItemId = 0;
        PlayerId = 0;
        Quantity = 0;
    }
}
```

### 2. 基本使用方法

```csharp
public class GameDataManager : MonoBehaviour
{
    private void Start()
    {
        // 预热引用池
        ReferencePool.Add<PlayerData>(10);
        ReferencePool.Add<NetworkMessage>(50);
    }
    
    public void ProcessPlayerData()
    {
        // 从引用池获取对象
        var playerData = ReferencePool.Acquire<PlayerData>();
        
        // 使用对象
        playerData.PlayerName = "Player1";
        playerData.Level = 10;
        playerData.Experience = 1500;
        playerData.Position = new Vector3(10, 0, 5);
        
        // 处理数据...
        ProcessData(playerData);
        
        // 使用完毕后释放回引用池
        ReferencePool.Release(playerData);
    }
    
    public void SendNetworkMessage(int msgId, byte[] data)
    {
        // 获取网络消息对象
        var message = ReferencePool.Acquire<NetworkMessage>();
        
        message.MessageId = msgId;
        message.Data = data;
        message.Length = data.Length;
        
        // 发送消息...
        NetworkManager.Send(message);
        
        // 发送完毕后释放
        ReferencePool.Release(message);
    }
    
    private void ProcessData(PlayerData data)
    {
        Debug.Log($"处理玩家数据: {data.PlayerName}, 等级: {data.Level}");
    }
}
```

### 3. 与事件系统结合使用

```csharp
public class InventoryManager : MonoBehaviour
{
    private void Start()
    {
        // 订阅物品拾取事件
        EventManager.Instance.Subscribe<ItemPickupEventArgs>(OnItemPickup);
    }
    
    public void PickupItem(int itemId, int playerId, int quantity)
    {
        // 从引用池获取事件参数
        var eventArgs = ReferencePool.Acquire<ItemPickupEventArgs>();
        eventArgs.ItemId = itemId;
        eventArgs.PlayerId = playerId;
        eventArgs.Quantity = quantity;
        
        // 发送事件（事件系统会自动释放事件参数）
        EventManager.Instance.Fire(eventArgs);
    }
    
    private void OnItemPickup(ItemPickupEventArgs eventArgs)
    {
        Debug.Log($"玩家 {eventArgs.PlayerId} 拾取了物品 {eventArgs.ItemId} x{eventArgs.Quantity}");
        
        // 更新背包...
        AddItemToInventory(eventArgs.ItemId, eventArgs.Quantity);
    }
    
    private void AddItemToInventory(int itemId, int quantity)
    {
        // 背包逻辑
    }
}
```

### 4. 批量操作

```csharp
public class BatchProcessor : MonoBehaviour
{
    private void Start()
    {
        // 批量添加对象到引用池
        ReferencePool.Add<PlayerData>(100);
        
        // 处理大量数据
        ProcessBatchData();
    }
    
    private void ProcessBatchData()
    {
        var dataList = new List<PlayerData>();
        
        // 批量获取对象
        for (int i = 0; i < 50; i++)
        {
            var data = ReferencePool.Acquire<PlayerData>();
            data.PlayerName = $"Player{i}";
            data.Level = i + 1;
            data.Experience = (i + 1) * 100;
            dataList.Add(data);
        }
        
        // 处理数据...
        foreach (var data in dataList)
        {
            ProcessPlayerData(data);
        }
        
        // 批量释放对象
        foreach (var data in dataList)
        {
            ReferencePool.Release(data);
        }
        
        dataList.Clear();
    }
    
    private void ProcessPlayerData(PlayerData data)
    {
        // 处理单个玩家数据
        Debug.Log($"处理玩家: {data.PlayerName}");
    }
}
```

### 5. 引用池监控

```csharp
public class ReferencePoolMonitor : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ShowReferencePoolStats();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearUnusedReferences();
        }
    }
    
    private void ShowReferencePoolStats()
    {
        Debug.Log($"引用池总数: {ReferencePool.Count}");
        
        // 可以扩展显示更详细的统计信息
        // 比如每种类型的对象数量、使用率等
    }
    
    private void ClearUnusedReferences()
    {
        // 清理特定类型的引用
        ReferencePool.RemoveAll<PlayerData>();
        
        // 或清理所有引用池
        // ReferencePool.ClearAll();
        
        Debug.Log("已清理未使用的引用对象");
    }
}
```

### 6. 自定义引用池管理

```csharp
public class CustomReferenceManager : ScriptSingleton<CustomReferenceManager>
{
    private Dictionary<Type, int> typeUsageCount = new Dictionary<Type, int>();
    
    public override void Initialize()
    {
        // 预热常用类型
        PrewarmCommonTypes();
    }
    
    private void PrewarmCommonTypes()
    {
        ReferencePool.Add<PlayerData>(20);
        ReferencePool.Add<NetworkMessage>(100);
        ReferencePool.Add<ItemPickupEventArgs>(30);
    }
    
    public T AcquireWithTracking<T>() where T : class, IReference, new()
    {
        var type = typeof(T);
        if (!typeUsageCount.ContainsKey(type))
        {
            typeUsageCount[type] = 0;
        }
        typeUsageCount[type]++;
        
        return ReferencePool.Acquire<T>();
    }
    
    public void ReleaseWithTracking<T>(T reference) where T : class, IReference
    {
        var type = typeof(T);
        if (typeUsageCount.ContainsKey(type))
        {
            typeUsageCount[type]--;
        }
        
        ReferencePool.Release(reference);
    }
    
    public void PrintUsageStats()
    {
        Debug.Log("引用池使用统计:");
        foreach (var kvp in typeUsageCount)
        {
            Debug.Log($"{kvp.Key.Name}: {kvp.Value}");
        }
    }
    
    public override void Shutdown()
    {
        PrintUsageStats();
        ReferencePool.ClearAll();
        base.Shutdown();
    }
}
```

## 使用注意事项

### 1. 接口实现
- 必须实现`IReference`接口
- `Clear()`方法要重置所有属性
- 避免在`Clear()`中进行复杂操作

### 2. 线程安全
- 引用池是线程安全的
- 可以在多线程环境中使用
- 但要注意对象本身的线程安全性

### 3. 内存管理
- 及时释放不再使用的对象
- 定期清理长时间未使用的引用池
- 避免持有大对象的引用

### 4. 性能考虑
- 适用于频繁创建销毁的小对象
- 不适用于生命周期很长的对象
- 预热可以避免运行时的分配开销

## 最佳实践

### 1. 对象设计
```csharp
// ✅ 好的引用对象设计
public class GoodReference : IReference
{
    public int Value { get; set; }
    public string Text { get; set; }
    
    public void Clear()
    {
        Value = 0;
        Text = string.Empty; // 清理字符串引用
    }
}

// ❌ 避免的设计
public class BadReference : IReference
{
    public List<object> Items { get; set; } = new List<object>();
    
    public void Clear()
    {
        // 没有清理List内容，可能导致内存泄漏
    }
}
```

### 2. 使用模式
```csharp
// ✅ 正确的使用模式
public void ProcessData()
{
    var data = ReferencePool.Acquire<MyData>();
    try
    {
        // 使用数据
        data.Initialize(/* 参数 */);
        DoSomething(data);
    }
    finally
    {
        // 确保释放
        ReferencePool.Release(data);
    }
}

// ❌ 避免的使用模式
public void BadUsage()
{
    var data = ReferencePool.Acquire<MyData>();
    // 忘记释放，导致内存泄漏
    
    // 或者
    ReferencePool.Release(data);
    data.Value = 10; // 释放后继续使用，危险！
}
```

### 3. 预热策略
```csharp
public class PrewarmManager : MonoBehaviour
{
    private void Start()
    {
        // 根据预期使用量预热
        ReferencePool.Add<FrequentlyUsedData>(50);
        ReferencePool.Add<OccasionallyUsedData>(10);
        
        // 分帧预热避免卡顿
        StartCoroutine(PrewarmOverFrames());
    }
    
    private IEnumerator PrewarmOverFrames()
    {
        for (int i = 0; i < 100; i += 10)
        {
            ReferencePool.Add<LargeData>(10);
            yield return null; // 等待一帧
        }
    }
}
```

## 性能优化建议

### 1. 合理预热
- 在游戏启动或场景加载时预热
- 根据实际使用情况调整预热数量
- 避免过度预热占用内存

### 2. 及时清理
- 在场景切换时清理不需要的引用池
- 定期检查和清理长时间未使用的对象
- 在内存紧张时主动清理

### 3. 监控使用
- 记录各类型对象的使用频率
- 监控引用池的命中率
- 根据统计数据优化预热策略

## 常见问题

### Q: 引用池和对象池有什么区别？
A: 引用池专门用于轻量级的IReference对象，对象池更通用但功能更复杂。引用池更适合小对象和事件参数。

### Q: 什么时候使用引用池？
A: 当对象创建频繁、生命周期短、实现简单时使用引用池。如事件参数、网络消息、临时数据等。

### Q: 如何避免内存泄漏？
A: 确保每次Acquire后都有对应的Release，在Clear方法中正确清理所有引用。

### Q: 引用池的容量有限制吗？
A: 没有硬性限制，但要根据内存情况合理设置。可以通过Remove方法控制池的大小。

## 扩展示例

### 网络消息池
```csharp
public class NetworkMessagePool : MonoBehaviour
{
    public class LoginRequest : IReference
    {
        public string Username { get; set; }
        public string Password { get; set; }
        
        public void Clear()
        {
            Username = string.Empty;
            Password = string.Empty;
        }
    }
    
    public class LoginResponse : IReference
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string ErrorMessage { get; set; }
        
        public void Clear()
        {
            Success = false;
            Token = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
    
    private void Start()
    {
        // 预热网络消息池
        ReferencePool.Add<LoginRequest>(20);
        ReferencePool.Add<LoginResponse>(20);
    }
    
    public void SendLoginRequest(string username, string password)
    {
        var request = ReferencePool.Acquire<LoginRequest>();
        request.Username = username;
        request.Password = password;
        
        // 发送请求...
        NetworkManager.Send(request);
        
        // 发送完毕后释放
        ReferencePool.Release(request);
    }
    
    public void HandleLoginResponse(bool success, string token, string error)
    {
        var response = ReferencePool.Acquire<LoginResponse>();
        response.Success = success;
        response.Token = token;
        response.ErrorMessage = error;
        
        // 处理响应...
        ProcessLoginResponse(response);
        
        // 处理完毕后释放
        ReferencePool.Release(response);
    }
}