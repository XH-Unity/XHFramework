## 🎯 <span style="color: red; font-size: 20px; font-weight: bold;">大家感兴趣的话可以进入QQ群一起交流学习！</span>

## 🎯 <span style="color: red; font-size: 22px; font-weight: bold;">QQ交流群：621790749</span>

# XHFramework - Unity 游戏客户端框架
XHFramework 是一个模块化的 Unity 游戏客户端框架，采用 **AOT层（固定核心）+ 热更层（动态业务）** 的双层架构设计。框架集成了 HybridCLR（代码热更新）和 YooAsset（资源热更新），实现了不发版本即可更新游戏逻辑和资源的能力。

## 一、核心技术栈
- **Unity** — 游戏引擎
- **HybridCLR** — C# 代码热更新（基于 IL2CPP 的补充元数据方案）
- **YooAsset** — 资源管理与热更新
- **Protobuf (Google.Protobuf)** — 网络序列化协议
- **Luban** — 配置表生成工具
- **UniTask** — Unity 异步编程框架（替代协程）
- **Newtonsoft.Json** — JSON 序列化

## 二、XHFramework目录结构
├── XHFramework/                 # 框架入口 FW.cs、管理器基类 ManagerBase.cs  
│   ├── XHFrameworkClient/       # unity客户端目录  
│   ├── XHFrameworkOut/          # 输出文件目录（安装包、logs）    
│   ├── XHFrameworkServer/       # 资源服务器  
│   ├── XHFrameworkTool/         # 工具（Luban、Proto）


## XHFrameworkClient目录结构
Assets/
├── XFramework.Boot/          # 启动模块（AOT层）—— 游戏入口、资源更新、代码热更  
├── XFramework.Core/          # 🏗️ 核心框架（AOT层）—— 15个管理器子系统  
│   ├── Base/                 # 框架入口 FW.cs、管理器基类 ManagerBase.cs  
│   ├── Audio/                # 音频管理  
│   ├── DataNode/             # 数据节点（树形数据存储）    
│   ├── DataTable/            # 数据表管理（Luban配置表）  
│   ├── Entity/               # 实体管理（游戏对象生命周期）  
│   ├── Event/                # 全局事件系统  
│   ├── Fsm/                  # 有限状态机  
│   ├── Http/                 # HTTP短连接  
│   ├── Localization/         # 多语言本地化  
│   ├── Log/                  # 日志系统  
│   ├── Map/                  # A*寻路系统  
│   ├── Network/              # 网络通信（TCP/UDP/WebSocket）  
│   ├── ObjectPool/           # 对象池  
│   ├── ReferencePool/        # 引用池（轻量级对象复用）  
│   ├── Resource/             # 资源管理（YooAsset集成）  
│   ├── Scene/                # 场景管理  
│   ├── Setting/              # 游戏设置  
│   ├── Singleton/            # 单例基类  
│   └── UI/                   # UI框架（UGUI）  
├── XFramework.Game/          # 🎮 游戏业务逻辑（热更层）—— 可热更新的业务代码  
│   └── HotUpdateScripts/     # 热更脚本（Game.cs入口、各种Config、场景逻辑等）  
├── XFramework.Editor/        # 🔧 编辑器工具  
├── XFramework.Third/         # 📦 第三方库封装  
└── XFramework.Unity/         # 🎨 Unity原生资源（场景、预制体、配置文件等）  


## 三、框架启动流程（Boot 模块）
### 3.1 启动入口 —— Boot.cs  
Boot.cs (MonoBehaviour，挂在启动场景的 GameObject 上)  
Awake()  
│  
└── 禁用 Unity 默认日志（Debug.unityLogger.logEnabled = false）  

Start() [async]  
│  
├── ① FW.Init(gameObject)              → 初始化框架，注册所有15个AOT层管理器  
├── ② new YooAssetService()             → 创建资源服务  
├── ③ new HybridClrService()            → 创建热更服务  
├── ④ 实例化 PatchWindow                → 显示更新界面（进度条、错误提示）  
├── ⑤ yooAssetService.InitializeAndUpdate()  → 资源版本检查 + 下载更新资源  
├── ⑥ hybridClrService.StartHybridCLRUpdate() → 加载AOT元数据 + 热更DLL  
├── ⑦ hybridClrService.EnterMainEntry()       → 通过反射调用热更层 Game.Start()  
└── ⑧ 销毁 PatchWindow                 → 更新完成，进入游戏  

Update()  
│  
└── FW.Update(deltaTime, unscaledDeltaTime)  → 每帧轮询所有管理器  

OnDestroy()  
    └── FW.Shutdown()                       → 关闭并清理所有管理器  

### 3.2 热更层入口 —— Game.cs
当 HybridCLR 加载完热更 DLL 后，通过反射调用 XFramework.Game.Game.Start()：  
Game.Start() [async]  
│  
├── ① PreloadDataTable()        → 加载所有 Luban 配置表（实体表、本地化表、音乐表等）  
├── ② InitHotUpdateConfigs()    → 初始化热更层配置  
│   ├── UIConfig.InitUI()           → 注册UI组（Background、Normal、Dialog、Tips等）  
│   ├── AudioConfig.InitAudio()     → 注册音频组（Music、Sound、UISound）  
│   ├── EntityConfig.InitEntity()   → 注册实体组  
│   ├── NetworkConfig.InitNetwork() → 配置TCP/UDP/WebSocket通道  
│   └── HttpConfig.InitHttp()       → 配置HTTP基础URL和Token  
├── ③ PreloadLoadLocalization()  → 初始化多语言系统（设置语言类型、数据获取委托）  
├── ④ LoadScene<GameScene>(1)    → 切换到游戏主场景  
├── ⑤ 加载地图资源 + 寻路数据  
└── ⑥ 加载角色资源  

### 3.3 BootConfig 配置
public static class BootConfig
{
    public static readonly string packageName = "DefaultPackage";
    public static readonly string packageUrl = "http://192.168.1.167:8084/XFramework/Res/";                                                
    public static readonly string hotUpdateDllName = "XFramework.Game.dll";        // 热更DLL名
    public static readonly string hotUpdateEntryScript = "XFramework.Game.Game";   // 热更入口类
    public static readonly string hotUpdateEntryMethod = "Start";                  // 热更入口方法
}

#### ✨ 核心设计洞察
- 为什么用双层架构？ AOT层（XFramework.Core）编译后不可变，保证框架稳定性；热更层（XFramework.Game）通过 HybridCLR 可以随时更新业务逻辑，无需重新发版。
- 反射调用热更入口：HybridCLR 加载 DLL 后，通过 Assembly.GetType().GetMethod().Invoke() 调用 Game.Start()，这是 AOT 层和热更层的桥梁。
- PatchWindow 的事件驱动设计：通过 OnStepChange、OnDownloadProgress、OnError 等事件回调，将更新进度反馈给 UI，实现了更新逻辑和 UI 显示的解耦。

## 四、核心架构设计（FW 静态门面）
### 4.1 FW.cs —— 框架中枢
FW 是整个框架的静态门面类（Facade Pattern），维护所有管理器的注册、轮询和销毁。

#### 核心数据结构
- LinkedList<ManagerBase> m_Managers —— 按优先级排序的管理器链表，用于 Update 轮询
- Dictionary<Type, ManagerBase> m_ManagerDict —— 类型到实例的映射，用于快速获取管理器

#### 15 个 AOT 层管理器（按优先级从高到低）  
| 优先级 | 管理器               | 职责                          |  
|--------|----------------------|-------------------------------|  
| 150    | SettingManager       | 游戏设置、帧率、游戏速度     
| 140    | DataNodeManager      | 树形数据节点存储             
| 130    | DataTableManager     | Luban 配置表管理             
| 120    | LocalizationManager  | 多语言本地化                  
| 120    | PathFindingManager   | A* 寻路                       
| 110    | ResourceManager      | 资源加载与管理（YooAsset）   
| 100    | EventManager         | 全局事件总线                  
| 90     | ObjectPoolManager    | 对象池管理                      
| 85     | HttpManager          | HTTP 短连接                  
| 80     | NetworkManager       | 网络通信（TCP/UDP/WS）    
| 60     | UIManager            | UI 界面管理                    
| 60     | FsmManager           | 有限状态机                     
| 50     | AudioManager         | 音频管理                        
| 40     | EntityManager        | 实体管理                        
| 30     | SceneManager         | 场景管理                        

#### 管理器注册流程（RegisterManager）
RegisterManager<T>()  
├── 检查是否已存在（字典查重）  
├── Activator.CreateInstance(typeof(T))  → 反射创建实例  
├── 注册到 m_ManagerDict 字典  
├── 按 Priority 插入 m_Managers 链表（优先级高的排前面）  
└── 调用 manager.Init() 初始化  

#### 轮询与关闭
- **Update()**：按链表顺序（优先级从高到低）轮询所有管理器
- **Shutdown()**：按链表逆序（优先级从低到高）关闭所有管理器，最后清理引用池

### 4.2 ManagerBase —— 管理器基类
public abstract class ManagerBase
{
    public virtual int Priority => 0;   // 优先级，越高越先被轮询，越后被关闭
    public abstract void Init();        // 初始化
    public abstract void Update(float elapseSeconds, float realElapseSeconds);  // 每帧轮询
    public abstract void Shutdown();    // 关闭清理
}

#### ✨ 核心设计洞察
- 为什么用 LinkedList 而不是 List？ 因为管理器注册时需要按优先级插入到正确位置，LinkedList 的插入操作是 O(1)，而 List 的中间插入是 O(n)。
- 为什么关闭顺序是逆序？ 高优先级的管理器（如 ResourceManager）可能被低优先级的管理器（如 SceneManager）依赖，所以先关闭低优先级的，确保依赖关系不被破坏。
- 热更层动态注册：通过 m_ManagerDict 字典和 GetManager<T>() 方法，热更层可以动态注册和获取自定义管理器，扩展框架能力。

## 五、核心子系统详解
### 5.1 资源管理系统（ResourceManager）
**优先级：110** | 集成 YooAsset
资源管理是框架最核心的模块之一，负责所有资源的加载、缓存和释放。

#### 四种资源组
| 资源组                          | 说明                           | 释放策略                     |
|---------------------------------|--------------------------------|------------------------------|
| NormalResourceGroup             | 普通资源                       | 延迟释放，超时后自动回收     
| ResidentResourceGroup（Global） | 全局常驻资源（字体、通用图集等）| 永不释放                     
| ResidentResourceGroup（Scene）  | 场景常驻资源                   | 切换场景时释放               
| TransientResourceGroup          | 临时资源                       | 加载后立即释放句柄           

#### 内存管理策略
- 配置了内存压力阈值（ResourceMemoryConfig）
- 当内存超过阈值时自动触发资源释放
- 场景切换时自动清理场景级资源

#### 主要 API
// 异步加载资源
await FW.ResourceManager.LoadAssetAsync<T>(assetPath, priority);
await FW.ResourceManager.LoadGameObjectAsync(assetPath, priority);

// 预加载常驻资源
await FW.ResourceManager.PreloadGlobalResidentAssetAsync<T>(assetPath);
await FW.ResourceManager.PreloadSceneResidentGameObjectAsync(assetPath);

// 场景加载
await FW.ResourceManager.LoadSceneAsync(scenePath, loadMode, priority);

// 释放资源
FW.ResourceManager.Release(asset, delayTime);

### 5.2 UI 管理系统（UIManager）
**优先级：60** | 基于 UGUI

#### 核心概念
- **UIGroup（界面组）**：如 Background、Normal、Dialog、Tips，每个组有独立的深度范围
- **UIForm（界面基类）**：所有 UI 界面继承此类，拥有完整的生命周期
- **DepthFactor**：UIGroup 之间深度间隔 10000，UIForm 之间深度间隔 100，确保层级不冲突

#### UIForm 生命周期
Init（首次创建）  
    │  
    ├── BindInit()     → 自动绑定 UI 元素  
    └── OnInit()       → 子类初始化逻辑  
    │  
Open（每次打开）  
    │  
    └── OnOpen()       → 子类打开逻辑  
    │  
    ├── Pause()  ←→  Resume()     → 被上层界面覆盖时暂停/恢复  
    ├── Cover()  ←→  Reveal()     → 被遮挡/恢复可见  
    └── OnUpdate()                 → 每帧轮询  
    │  
Close（关闭）  
    │  
    └── OnClose()      → 子类关闭逻辑  
    │  
Recycle（回收到对象池）  

#### UIForm 初始化时的自动处理
// Init 方法中自动完成：
_cachedCanvas = gameObject.GetOrAddComponent<Canvas>();       // 添加 Canvas
_cachedCanvas.overrideSorting = true;                         // 覆盖排序
_canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();   // 添加 CanvasGroup
rectTransform.anchorMin = Vector2.zero;                       // 全屏拉伸
rectTransform.anchorMax = Vector2.one;
gameObject.GetOrAddComponent<GraphicRaycaster>();             // 添加射线检测

#### PointerEventListener 事件系统
UIForm 内置了完整的指针事件支持，通过位标记（Flags）选择性绑定：
- Click、DoubleClick、LongPress（开始/持续/结束）
- PointerEnter/Exit、PointerDown/Up
- BeginDrag/Drag/EndDrag/Drop
- Scroll、Select/Deselect

### 5.3 实体管理系统（EntityManager）
**优先级：40**
用于管理游戏中的动态对象（角色、NPC、怪物、特效等）。

#### 核心概念
- **EntityGroup（实体组）**：按类型分组管理（如 Player、Monster、Effect）
- **Entity（实体基类）**：继承 MonoBehaviour，拥有完整生命周期
- **EntityData（实体数据）**：携带位置、旋转、配置表ID等初始化数据
- **父子关系**：支持实体之间的附加/解除（如骑乘、装备挂载）

#### Entity 状态机
WillInit → Inited → WillShow → Showed → WillHide → Hidden → WillRecycle → Recycled

#### Entity 生命周期回调（子类可重写）
protected virtual void OnInit(object userData) { }          // 首次创建
protected virtual void OnShow(object userData) { }          // 显示（设置位置、旋转等）
protected virtual void OnHide(object userData) { }          // 隐藏
protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) { }  // 每帧更新
protected virtual void OnAttachTo(Entity parent, Transform attachPoint, object userData) { }   // 附加到父实体
protected virtual void OnDetachFrom(Entity parent, object userData) { }  // 从父实体解除
protected virtual void OnAttached(Entity child, Transform attachPoint, object userData) { }    // 子实体附加到自己
protected virtual void OnDetached(Entity child, object userData) { }     // 子实体从自己解除

### 5.4 网络通信系统（NetworkManager）
**优先级：80** | 三通道架构

#### 三种网络通道  
| 通道               | 协议     | 用途                          | 特点                         |    
|--------------------|----------|-------------------------------|------------------------------|  
| TcpChannel         | TCP      | 登录、背包、战斗指令等可靠逻辑 | 心跳检测、自动重连、粘包处理 |  
| UdpChannel         | UDP      | 移动同步、战斗状态同步         | 低延迟、无连接               |  
| WebSocketChannel   | WebSocket| 排行榜、社交功能               | 浏览器兼容                   |  

#### TCP 通道核心设计（TcpChannel.cs）
##### ① 消息协议格式
[Length: 4字节, 小端序] + [MessageId: 2字节, 小端序] + [Body: 变长, Protobuf序列化]
- HeaderSize = 6 字节
- 最大消息长度 = 64KB

##### ② 双线程收发模型  
主线程（Unity Update）   
├── UpdateHeartbeat()      → 心跳检测  
├── UpdateReconnect()      → 重连逻辑  
└── UpdateReceiveQueue()   → 处理接收队列中的消息  

发送线程（后台）  
└── SendLoop()  
    ├── _sendEvent.WaitOne()   → 信号量等待（无数据时阻塞，不消耗CPU）  
    └── _sendQueue.Dequeue()   → 从队列取数据发送    
  
接收线程（后台）  
└── ReceiveLoop()  
    ├── _socket.Receive()      → 阻塞接收数据  
    ├── 追加到 _dataBuffer     → 数据缓冲区  
    ├── ProcessReceivedData()  → 粘包处理  
    └── 解码后放入 _receiveQueue → 主线程消费  

##### ③ 粘包处理（ProcessReceivedData）
while (缓冲区数据 >= HeaderSize) {
    读取 bodyLength（前4字节）
    if (缓冲区数据 < HeaderSize + bodyLength)
        break;  // 数据不完整，等待更多数据
    提取完整消息 → 解码 → 放入接收队列
    移除已处理的数据（Array.Copy 前移）
}

##### ④ 缓冲区自动扩缩容
- 初始大小：8KB
- 最大大小：1MB
- 扩容策略：翻倍增长（8KB → 16KB → 32KB → ...）
- 缩容条件：缓冲区 > 初始大小×4 且 数据量 < 缓冲区的 1/4

##### ⑤ 心跳机制
- 心跳间隔：5秒发送一次心跳包（MessageId=0 的空包）
- 心跳超时：15秒未收到响应算一次超时
- 最大超时次数：3次 → 断开连接
- 优化：发送任何消息都重置心跳计时器（避免无谓的心跳包）
- 优化：收到任何消息都重置心跳接收计时器

##### ⑥ 自动重连
- 最大重连次数：5次
- 重连间隔：3秒
- 触发条件：非主动断开 + 启用自动重连
- 重连流程：通知上层 → 等待间隔 → DoConnect() → 成功则重置计数，失败则继续尝试
- 主动断开（Disconnect）不触发重连

##### ⑦ 线程安全
- 发送队列：lock(_sendLock) + AutoResetEvent 信号量
- 接收队列：lock(_receiveLock)
- 运行标志：volatile bool _isRunning

### 5.5 对象池系统（ObjectPoolManager + ObjectPool<T>）
**优先级：90**
#### 两层池化设计
| 层级           | 类           | 用途                          | 存储结构                  |
|----------------|--------------|-------------------------------|---------------------------|
| 重量级对象池   | ObjectPool<T>| UI界面、实体、音频等 GameObject | LinkedList<ObjectBase>    |
| 轻量级引用池   | ReferencePool| 事件参数、网络包等纯C#对象     | Dictionary<Type, Queue>   |

#### ObjectPool<T> 核心机制
// 获取对象（Spawn）
T obj = objectPool.Spawn(name);
// 遍历链表，找到名称匹配且未使用（或允许多次获取）的对象
// 找到则 SpawnCount++，返回对象
// 找不到则返回 null（由上层决定是否创建新实例）

// 归还对象（Unspawn）
objectPool.Unspawn(target);
// 遍历链表，找到 Target 匹配的对象
// SpawnCount--，触发 Release() 检查是否需要释放超量对象

// 自动释放
objectPool.Update(elapseSeconds, realElapseSeconds);
// 累计时间 >= AutoReleaseInterval 时，释放超出容量的过期对象

#### 释放策略（DefaultReleaseObjectFilterCallBack）
1. 计算过期参考时间 = 当前时间 - ExpireTime
2. 遍历可释放对象（未使用的）
3. 如果对象的 LastUseTime <= 过期参考时间 → 加入释放列表
4. 释放数量达到 toReleaseCount 时停止

#### ReferencePool（引用池）
// 获取临时对象
GlobalEventArgs args = ReferencePool.Acquire<GlobalEventArgs>();

// 使用完毕后归还
ReferencePool.Release(args);  // 调用 args.Clear() 重置状态后放回池中

// 线程安全：内部使用 lock 同步

### 5.6 事件系统（EventManager）
**优先级：100**
全局事件总线，用于模块间解耦通信。

#### 核心设计
// 订阅事件
FW.EventManager.Subscribe(eventId, handler);

// 发布事件（延迟模式 —— 放入队列，下一帧处理）
FW.EventManager.Fire(sender, eventArgs);

// 发布事件（立即模式 —— 当场执行所有处理器）
FW.EventManager.FireNow(sender, eventArgs);

// 取消订阅
FW.EventManager.Unsubscribe(eventId, handler);

#### EventPool 内部实现
- 事件处理器存储：Dictionary<int, EventHandler<GlobalEventArgs>>（eventId → 多播委托）
- 延迟事件队列：Queue<Event>（线程安全，主线程 Update 时逐个处理）
- 事件参数通过 ReferencePool 池化，减少 GC

### 5.7 场景管理系统（SceneManager）
**优先级：30**

#### 场景加载流程  
LoadScene<T>(sceneID, sceneAssetName)  
│  
├── ① 卸载当前场景的 UI、实体、音频  
├── ② 打开 Loading 界面  
├── ③ 异步加载新场景（YooAsset）  
├── ④ 清理旧场景资源（UnloadSceneAssetsAsync）  
├── ⑤ 调用新场景的 OnPreload()（预加载场景资源）  
├── ⑥ 调用新场景的 OnEnter()（进入场景逻辑）  
└── ⑦ 关闭 Loading 界面  

#### SceneBase 生命周期
public abstract class SceneBase
{
    public virtual async UniTask OnPreload() { }   // 预加载资源
    public virtual void OnEnter() { }               // 进入场景
    public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) { }  // 每帧更新
    public virtual void OnLeave() { }               // 离开场景
}

### 5.8 有限状态机（FsmManager + Fsm<T>）
**优先级：60**
泛型状态机，可用于游戏流程控制、AI 行为等。

// 创建状态机
IFsm fsm = FW.FsmManager.CreateFsm<GameController>("GameFsm", owner,
    new IdleState(),
    new RunState(),
    new AttackState()
);

// 启动状态机
fsm.Start<IdleState>();

#### Fsm<T> 核心能力
- 状态数据存储：SetData(name, data) / GetData<T>(name) —— 状态间共享数据
- 事件机制：FireEvent(sender, eventId) —— 状态内部可以触发事件
- 当前状态时间：自动记录当前状态已持续的时间

### 5.9 数据表系统（DataTableManager）
**优先级：130** | 集成 Luban
管理由 Luban 工具生成的配置表数据。

#### 加载流程
1. LoadAllDataTablesByTagAsync()   → 从 YooAsset 加载所有配置表的字节数据（缓存到字典）
2. LoadTable<TbEntity>("tbentity") → 解析指定表的字节数据，创建表实例
3. GetTable<TbEntity>()            → 获取已加载的表实例，查询数据

#### 当前项目已配置的表  
| 表名               | 类型         | 用途                          |  
|--------------------|--------------|-------------------------------|  
| TbEntity           | 实体表       | 实体配置（资源名、属性等）|  
| TbLocalizationText | 本地化文本表 | 多语言文本（CN、EN）|  
| TbMusic            | 音乐表       | 背景音乐配置                  |  
| TbScene            | 场景表       | 场景配置                      |  
| TbSound            | 音效表       | 音效配置                      |  
| TbUISound          | UI音效表     | UI交互音效                    |  
| TbMap              | 地图表       | 地图资源和寻路数据            |  
| TbRole             | 角色表       | 角色资源配置                  |  

### 5.10 本地化系统（LocalizationManager）
**优先级：120**
设计思路： 采用委托模式获取文本数据，不直接依赖具体的数据源，解耦了本地化系统和数据表系统。

// 设置支持的语言类型
FW.LocalizationManager.SetLanguageTypes(new string[] { "CN", "EN" });

// 设置数据获取委托（从 Luban 配置表读取）
FW.LocalizationManager.SetLanguageData((id, language) =>
{
    TableLocalizationText text = FW.DataTableManager.GetTable<TbLocalizationText>()[id];
    if (text == null) return null;
    return language switch 
    {
        "CN" => text.CN,
        "EN" => text.EN,
        _ => text.CN
    };
});

// 获取本地化文本
string text = FW.LocalizationManager.GetText(textId);

// 切换语言（会触发事件通知所有绑定组件刷新）
FW.LocalizationManager.SetLanguage("EN");

#### LocalizationTextBind 组件
挂在 UI 文本上，配置 textId 后自动绑定，语言切换时自动刷新显示。

### 5.11 音频系统（AudioManager）
**优先级：50**
#### 三层结构
AudioManager  
├── AudioGroup "Music"     → 背景音乐（通常1个Agent）  
│   └── AudioAgent         → 封装 AudioSource  
├── AudioGroup "Sound"     → 游戏音效（多个Agent）  
│   ├── AudioAgent  
│   ├── AudioAgent  
│   └── AudioAgent  
└── AudioGroup "UISound"   → UI音效（多个Agent）  
    ├── AudioAgent  
    └── AudioAgent  
 
#### 主要 API
// 播放音频（异步加载资源后播放）
int? serialId = await FW.AudioManager.PlayAudio(assetName, "Music", priority, playParams);

// 控制播放
FW.AudioManager.PauseAudio(serialId);
FW.AudioManager.ResumeAudio(serialId);
FW.AudioManager.StopAudio(serialId);

// 组级控制
audioGroup.Mute = true;       // 静音整个组
audioGroup.Volume = 0.5f;     // 调整组音量

#### AudioAgent 选择策略
播放时遍历组内所有 Agent，优先选择空闲的；如果都在使用中，则替换优先级最低的。

### 5.12 HTTP 系统（HttpManager）
**优先级：85**
基于 UnityWebRequest 封装的 HTTP 短连接客户端。

// GET 请求（泛型自动反序列化）
HttpResponse<UserInfo> resp = await FW.HttpManager.GetAsync<UserInfo>(url);

// POST 请求（自动序列化请求体）
HttpResponse<LoginResult> resp = await FW.HttpManager.PostAsync<LoginResult>(url, new { username, password });

// 文件下载（带进度回调）
HttpResponse resp = await FW.HttpManager.DownloadAsync(url, headers, timeout, progress);

// 表单提交
HttpResponse<T> resp = await FW.HttpManager.PostFormAsync<T>(url, formData);

#### 特性
支持 Bearer Token 认证、自定义 Headers、超时配置、JSON 自动序列化/反序列化。

### 5.13 数据节点系统（DataNodeManager）
**优先级：140**
树形数据结构，类似文件系统路径，用于存储运行时全局数据。

// 路径式访问（支持 "."、"/"、"\" 分隔符）
FW.DataNodeManager.SetData("player/level", 10);
FW.DataNodeManager.SetData("player/name", "Hero");
FW.DataNodeManager.SetData("game/score", 9999);

int level = FW.DataNodeManager.GetData<int>("player/level");

// 数据树结构：  
// Root  
// ├── player  
// │   ├── level = 10  
// │   └── name = "Hero"  
// └── game  
//     └── score = 9999  

#### 用途
替代全局静态变量，提供结构化的运行时数据存储，方便模块间共享数据。

### 5.14 A* 寻路系统（PathFindingManager）
**优先级：120**
// 加载地图数据（JSON格式的网格数据）
await FW.PathFindingManager.LoadMapAsync(mapJsonAssetPath);

// 网格坐标寻路
List<Vector2Int> path = FW.PathFindingManager.FindPath(startX, startY, endX, endY);

// 世界坐标寻路（自动转换）
List<Vector3> path = FW.PathFindingManager.FindPath(startWorld, endWorld);

// 检查某个位置是否可行走
bool walkable = FW.PathFindingManager.IsWalkable(worldPosition);

#### 特性
支持多地图管理、对角线移动选项、网格坐标与世界坐标自动转换。

### 5.15 设置系统（SettingManager）
**优先级：150**
基于 PlayerPrefs 的持久化配置管理。

// 游戏速度控制
FW.SettingManager.SetGameSpeed(2.0f);   // 2倍速
FW.SettingManager.PauseGame();           // 暂停（Time.timeScale = 0）
FW.SettingManager.ResumeGame();          // 恢复

// 帧率设置
FW.SettingManager.SetFrameRate(60);

// 通用配置存储
FW.SettingManager.SetString("lastLogin", "2026-02-06");
FW.SettingManager.SetObject("userSettings", settingsObj);  // JSON序列化存储
T obj = FW.SettingManager.GetObject<T>("userSettings");     // JSON反序列化读取

## 六、日志系统（Log）
// 条件编译，Release 版本自动移除日志代码
[Conditional("EnableLog")]
public static void Info(string format, params object[] args);

[Conditional("EnableLog")]
public static void Warn(string format, params object[] args);

[Conditional("EnableLog")]
public static void Error(string format, params object[] args);

### 特性
- 通过 [Conditional("EnableLog")] 实现零开销的日志移除（Release 版本不编译日志调用）
- 自动通过堆栈追踪获取调用者信息（类名、方法名）
- 支持多种日志处理器：ConsoleLogHandler（控制台）、FileLogHandler（文件）

## 七、单例模式
两种单例基类，适配不同使用场景：

### MonoBehaviour 单例（需要挂载到 GameObject）
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static T Instance { get; }  // 懒加载，自动创建 GameObject + DontDestroyOnLoad
}

### 纯 C# 单例（不依赖 Unity）
public abstract class ScriptSingleton<T> where T : class, new()
{
    public static T Instance { get; }  // 懒加载
}

## 八、设计模式  
| 设计模式            | 应用位置                       | 说明                          |  
|---------------------|--------------------------------|-------------------------------|  
| 门面模式 (Facade)    | FW.cs                           | 统一入口，隐藏子系统复杂性    |  
| 管理器模式 (Manager) | 所有 ManagerBase 子类           | 每个子系统一个管理器，职责单一|  
| 单例模式 (Singleton) | MonoSingleton、ScriptSingleton  | 全局唯一实例                  |  
| 对象池模式 (Object Pool) | ObjectPool<T>、ReferencePool  | 减少 GC，复用对象             |  
| 观察者模式 (Observer) | EventManager、EventPool         | 全局事件订阅/发布，模块解耦   |  
| 状态模式 (State)     | Fsm<T>、FsmState<T>             | 有限状态机，管理状态转换      |  
| 策略模式 (Strategy)  | NetworkChannelBase → TCP/UDP/WS | 同一接口，不同网络协议实现    |  
| 模板方法 (Template Method) | UIForm、Entity、SceneBase   | 基类定义流程骨架，子类重写具体步骤 |  
| 工厂模式 (Factory)   | FW.RegisterManager<T>()         | 通过反射创建管理器实例        |  
| 组合模式 (Composite) | DataNode 树形结构               | 树形数据节点，路径式访问      |  
| 生产者-消费者        | TcpChannel 收发队列             | 接收线程生产数据，主线程消费处理 |  

## 九、框架架构图  

![alt text](./images/image.png)

### 十、内部工具

### Unity XHFrameworkClient UI自动绑定工具  
1导入XHFrameworkClient之后  
2在需要使用自动绑定工具的对象上面挂载组件，并指定需要写入的脚本  
![alt text](./images/1.png)    
3这个对象包括子物体右边会有➕号    
![alt text](./images/2.png)    
4点击加号会出现这个对象身上所有组件，选择组件就会自动保存你需要的组件，并显示在+号前面  
![alt text](./images/3.png)    
5在显示所绑定的组件右键也可以删除和替换  
![alt text](./images/4.png)    
6绑定组件上面也会显示你所绑定的组件和对象，点击生成会自动生成，脚本的partial脚本  
![alt text](./images/5.png)    
7这个对象脚本就可以直接.出来对象，例如图上的LoadingForm._Scrollbar_LoadingScroll进行使用  
  


### Unity XHFrameworkClient 配置工具    
1导入XHFrameworkClient之后，选择游戏工具!  
![alt text](./images/6.png)      
2在配置工具可以设置luban导入路径和导出路径，点击导出即可  
![alt text](./images/7.png)      
3 网络模块proto文件也是如此!  
![alt text](./images/8.png)      



### Unity XHFrameworkClient 地图寻路工具    
1导入XHFrameworkClient之后  
2在地形预制体挂载MapPathFindingEditor组件  
![alt text](./images/10.png)      
3点击scene视图画板  
![alt text](./images/11.png)      
4选择画板绿色   
![alt text](./images/12.png)      
5在地形 你想设置边界的矩形区域 任意两个对象填充绿色，即设置寻路区域边界  
![alt text](./images/13.png)      
6再次选择红色方块画笔，在scene视图画不可行走区域  
![alt text](./images/14.png)      
7设置地图信息和导出路径，点击生成json，就会将地图信息和区域大小，和每个方块信息导入json，  
![alt text](./images/15.png)      



### Unity XHFrameworkClient 自动化打包工具    
1导入XHFrameworkClient之后，选择游戏工具  
![alt text](./images/16.png)  
2选择打包设置  
![alt text](./images/17.png)  
3需要手动修改代码内资源服务器地址  
启动请求时候资源服务器地址  
![alt text](./images/18.png)打包工具时候上传服务器目录，本项目做了一个本地的服务器，详情可查看  https://gitee.com/xframework886/xframework-server  
![alt text](./images/19.png)  
4配置构建信息  
5本工具配置之后，全为自动化，等待出包或者热更完成即可  




另外还有基于GAS模式的Unity技能框架与节点节能编辑器，非常值得一看  

XHFrameworkSkill：https://github.com/XH-Unity/XHFrameworkSkill
