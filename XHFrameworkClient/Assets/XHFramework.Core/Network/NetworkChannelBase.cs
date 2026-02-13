using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace XHFramework.Core
{
    /// <summary>
    /// 网络通道基类
    /// </summary>
    public abstract class NetworkChannelBase : INetworkChannel
    {
        #region 字段

        /// <summary>
        /// 消息处理器字典
        /// </summary>
        protected readonly Dictionary<int, Action<IMessage>> _messageHandlers = new Dictionary<int, Action<IMessage>>();

        /// <summary>
        /// 消息解析器注册表（消息ID -> MessageParser）
        /// 使用 MessageParser 代替反射，性能更好
        /// </summary>
        protected readonly Dictionary<int, MessageParser> _messageParsers = new Dictionary<int, MessageParser>();

        /// <summary>
        /// 接收队列锁
        /// </summary>
        protected readonly object _receiveLock = new object();

        /// <summary>
        /// NetworkManager引用
        /// </summary>
        protected NetworkManager _networkManager;

        #endregion

        #region 属性

        /// <summary>
        /// 通道名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 通道类型
        /// </summary>
        public abstract NetworkChannelType ChannelType { get; }

        /// <summary>
        /// 当前连接状态
        /// </summary>
        public NetworkState State { get; protected set; } = NetworkState.Disconnected;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => State == NetworkState.Connected;

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; protected set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; protected set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        protected NetworkChannelBase(string name, NetworkManager networkManager)
        {
            Name = name;
            _networkManager = networkManager;
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 连接服务器
        /// </summary>
        public abstract void Connect(string host, int port);

        /// <summary>
        /// 断开连接
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// 发送消息
        /// </summary>
        public abstract void Send(int messageId, IMessage message);

        /// <summary>
        /// 轮询（由子类实现）
        /// </summary>
        public abstract void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理通道（由子类实现）
        /// </summary>
        public virtual void Shutdown()
        {
            Disconnect();
            _messageHandlers.Clear();
            _messageParsers.Clear();
        }

        #endregion

        #region 消息处理器注册

        /// <summary>
        /// 注册消息处理器
        /// </summary>
        public void RegisterHandler<T>(int messageId, Action<T> handler) where T : IMessage<T>, new()
        {
            // 注册 MessageParser
            if (!_messageParsers.ContainsKey(messageId))
            {
                _messageParsers[messageId] = new MessageParser<T>(() => new T());
            }

            _messageHandlers[messageId] = (message) =>
            {
                if (message is T typedMessage)
                {
                    handler(typedMessage);
                }
            };
        }

        /// <summary>
        /// 注销消息处理器
        /// </summary>
        public void UnregisterHandler(int messageId)
        {
            _messageHandlers.Remove(messageId);
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 解析并处理消息
        /// </summary>
        protected void UnpackNetPacket(ReceiveNetPacketBase packet)
        {
            // 尝试解析消息
            IMessage message = null;
            if (_messageParsers.TryGetValue(packet.MessageId, out MessageParser parser))
            {
                try
                {
                    message = parser.ParseFrom(packet.Body, 0, packet.BodyLength);
                }
                catch (Exception ex)
                {
                    Log.Error("解析消息失败 MessageId={0}, Error={1}", packet.MessageId, ex.Message);
                    return;
                }
            }

            // 调用通道自己的处理器
            if (message != null && _messageHandlers.TryGetValue(packet.MessageId, out Action<IMessage> handler))
            {
                try
                {
                    handler(message);
                }
                catch (Exception ex)
                {
                    Log.Error("处理消息失败 MessageId={0}, Error={1}", packet.MessageId, ex.Message);
                }
            }
        }

        #endregion

        #region 事件触发

        /// <summary>
        /// 触发连接成功事件
        /// </summary>
        protected void OnConnected()
        {
            State = NetworkState.Connected;
            _networkManager?.OnChannelConnected(this);
        }

        /// <summary>
        /// 触发断开连接事件
        /// </summary>
        protected void OnDisconnected(DisconnectReason reason)
        {
            State = NetworkState.Disconnected;
            _networkManager?.OnChannelDisconnected(this, reason);
        }

        /// <summary>
        /// 触发连接失败事件
        /// </summary>
        protected void OnConnectFailed(string error)
        {
            State = NetworkState.Disconnected;
            _networkManager?.OnChannelConnectFailed(this, error);
        }

        /// <summary>
        /// 触发网络错误事件
        /// </summary>
        protected void OnError(string error)
        {
            _networkManager?.OnChannelError(this, error);
        }

        #endregion
    }
}
