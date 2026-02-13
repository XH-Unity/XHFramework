# Framework.Base 核心基础模块

## 概述

Framework.Base 是整个游戏框架的核心基础模块，提供了单例系统和框架管理器。这是框架的最小核心，所有其他模块都依赖于此模块。

## 模块结构

```
Framework/Base/
├── Singleton/              # 单例系统
│   ├── ISingleton.cs       # 单例接口
│   ├── ScriptSingleton.cs  # 脚本单例基类
│   └── MonoSingleton.cs    # MonoBehaviour单例基类
├── FrameworkManager.cs     # 框架管理器
└── README.md              # 说明文档
```

## 核心功能

### 1. 单例系统

提供两种单例实现，满足不同场景需求：

#### ScriptSingleton<T> - 脚本单例
适用于纯C#类，支持热更新：

```csharp
public class ConfigManager : ScriptSingleton<ConfigManager>
{
    private Dictionary<string, string> configs = new Dictionary<string, string>();
    
    public override void Initialize()
    {
        // 初始化配置数据
        LoadConfigs();
    }
    
    public override void Shutdown()
    {
        // 清理配置数据
        configs.Clear();
    }
    
    private void LoadConfigs()
    {
        // 加载配置逻辑
        configs["GameVersion"] = "1.0.0";
        configs["ServerUrl"] = "https://api.game.com";
    }
    
    public string GetConfig(string key)
    {
        return configs.TryGetValue(key, out string value) ? value : string.Empty;
    }
}

// 使用方式
string version = ConfigManager.Instance.GetConfig("GameVersion");
```

#### MonoSingleton<T> - MonoBehaviour单例
适用于需要Unity生命周期的管理器：

```csharp
public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    public override void Initialize()
    {
        // 初始化音频组件
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        // 额外的Awake逻辑
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}

// 使用方式
AudioManager.Instance.PlayMusic(backgroundMusic);
AudioManager.Instance.PlaySFX(clickSound);
```

### 2. 框架管理器

统一管理框架的初始化和生命周期：

```csharp
public class GameLauncher : MonoBehaviour
{
    private void Start()
    {
        // 手动初始化框架
        FrameworkManager.Instance.InitializeFramework();
        
        // 检查框架是否初始化完成
        if (FrameworkManager.Instance.IsInitialized)
        {
            Debug.Log("框架初始化完成，开始游戏逻辑");
            StartGame();
        }
    }
    
    private void StartGame()
    {
        // 游戏启动逻辑
        ConfigManager.Instance.GetConfig("GameVersion");
        AudioManager.Instance.PlayMusic(backgroundMusic);
    }
    
    private void OnApplicationQuit()
    {
        // 框架会自动关闭，也可以手动关闭
        FrameworkManager.Instance.ShutdownFramework();
    }
}
```

## 使用注意事项

### 1. 单例选择原则
- **ScriptSingleton**: 用于纯逻辑管理器，如配置管理、数据管理等
- **MonoSingleton**: 用于需要Unity功能的管理器，如音频、输入、UI等

### 2. 初始化顺序
- 框架管理器会自动按依赖顺序初始化模块
- 自定义单例应在FrameworkManager初始化后使用

### 3. 线程安全
- 两种单例都实现了线程安全
- 但Unity API调用必须在主线程进行

### 4. 热更新兼容
- **ScriptSingleton**: 完全支持热更新
- **MonoSingleton**: 不支持热更新，适用于AOT部分

### 5. 内存管理
- 单例会在应用退出时自动销毁
- 可通过DestroyInstance()手动销毁
- 避免在单例中持有大量数据引用

## 最佳实践

### 1. 单例设计
```csharp
// ✅ 正确的单例设计
public class DataManager : ScriptSingleton<DataManager>
{
    public override void Initialize()
    {
        // 初始化逻辑
    }
    
    public override void Shutdown()
    {
        // 清理逻辑
    }
}

// ❌ 避免的设计
public class BadManager : ScriptSingleton<BadManager>
{
    // 不要在构造函数中做复杂操作
    public BadManager()
    {
        // 复杂初始化逻辑应该放在Initialize中
    }
}
```

### 2. 依赖管理
```csharp
// ✅ 正确的依赖使用
public class GameManager : ScriptSingleton<GameManager>
{
    public override void Initialize()
    {
        // 确保依赖的单例已初始化
        if (ConfigManager.HasInstance)
        {
            var version = ConfigManager.Instance.GetConfig("Version");
        }
    }
}

// ❌ 避免循环依赖
public class ManagerA : ScriptSingleton<ManagerA>
{
    public override void Initialize()
    {
        ManagerB.Instance.DoSomething(); // 如果ManagerB也依赖ManagerA，会造成循环依赖
    }
}
```

### 3. 错误处理
```csharp
public class SafeManager : ScriptSingleton<SafeManager>
{
    public bool TryGetData(string key, out string value)
    {
        value = string.Empty;
        
        try
        {
            // 安全的操作
            return dataDict.TryGetValue(key, out value);
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取数据失败: {ex.Message}");
            return false;
        }
    }
}
```

## 常见问题

### Q: 如何选择单例类型？
A: 如果需要Unity生命周期（Update、Coroutine、MonoBehaviour功能）使用MonoSingleton，纯逻辑管理使用ScriptSingleton。

### Q: 单例什么时候初始化？
A: 第一次访问Instance属性时自动初始化，也可以通过FrameworkManager统一管理初始化顺序。

### Q: 如何处理单例依赖？
A: 使用HasInstance检查依赖是否已创建，或通过FrameworkManager管理初始化顺序。

### Q: 单例在热更新时会丢失吗？
A: ScriptSingleton支持热更新，MonoSingleton不支持。建议业务逻辑使用ScriptSingleton。

## 扩展指南

### 添加新的单例模块
```csharp
public class NewManager : ScriptSingleton<NewManager>
{
    public override void Initialize()
    {
        Debug.Log("NewManager 初始化");
        // 初始化逻辑
    }
    
    public override void Shutdown()
    {
        Debug.Log("NewManager 关闭");
        // 清理逻辑
    }
    
    public void DoSomething()
    {
        // 业务逻辑
    }
}

// 在需要的地方使用
NewManager.Instance.DoSomething();
```

## 版本历史

- v1.0.0: 初始版本，包含基础的单例系统和框架管理器