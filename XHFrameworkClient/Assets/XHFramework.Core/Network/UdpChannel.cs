using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using LiteNetLib;


namespace XHFramework.Core
{
    /// <summary>
    /// UDP网络通道（基于LiteNetLib）
    /// 用于实时性要求高的同步（移动、战斗、状态同步等）
    /// </summary>
    public class UdpChannel : NetworkChannelBase, INetEventListener
    {
        #region 常量

        /// <summary>
        /// 消息头长度（2字节消息ID）
        /// </summary>
        private const int HeaderSize = 2;

        #endregion

        #region 字段

        /// <summary>
        /// LiteNetLib网络管理器
        /// </summary>
        private NetManager _netManager;

        /// <summary>
        /// 当前连接的服务器Peer
        /// </summary>
        private NetPeer _serverPeer;

        /// <summary>
        /// 连接密钥
        /// </summary>
        private string _connectionKey = "XHFramework";

        /// <summary>
        /// 接收队列
        /// </summary>
        private readonly Queue<UdpReceiveNetPacket> _receiveQueue = new Queue<UdpReceiveNetPacket>();

        // ========== 重连相关 ==========
        private int _reconnectCount;
        private volatile bool _isReconnecting;
        private float _reconnectWaitTime;
        private bool _isManualDisconnect;

        #endregion

        #region 属性

        /// <summary>
        /// 通道类型
        /// </summary>
        public override NetworkChannelType ChannelType => NetworkChannelType.Udp;

        // ========== 重连配置属性 ==========
        public bool AutoReconnectEnabled { get; set; } = true;
        public int MaxReconnectCount { get; set; } = 5;
        public float ReconnectInterval { get; set; } = 3f;
        public bool IsReconnecting => _isReconnecting;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpChannel(string name, NetworkManager networkManager) : base(name, networkManager)
        {
        }

        #endregion

        #region 配置方法

        /// <summary>
        /// 设置连接密钥
        /// </summary>
        public void SetConnectionKey(string key)
        {
            _connectionKey = key;
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 连接服务器
        /// </summary>
        public override void Connect(string host, int port)
        {
            if (State == NetworkState.Connecting || State == NetworkState.Connected)
            {
                Log.Warn("[UdpChannel] 已经在连接或已连接状态，请先断开");
                return;
            }
         
            Host = host;
            Port = port;
            _isManualDisconnect = false;
            _reconnectCount = 0;
            _isReconnecting = false;

            DoConnect();
        }

        /// <summary>
        /// 执行连接
        /// </summary>
        private void DoConnect()
        {
            State = NetworkState.Connecting;

            try
            {
                // 创建NetManager
                _netManager = new NetManager(this)
                {
                    AutoRecycle = true,
                    EnableStatistics = false,
                    UnconnectedMessagesEnabled = false,
                    NatPunchEnabled = false,
                    DisconnectTimeout = 10000, // 10秒超时
                    PingInterval = 1000,       // 1秒ping一次
                    ReconnectDelay = 500,
                    MaxConnectAttempts = 5
                };

                _netManager.Start();

                // 发起连接
                _serverPeer = _netManager.Connect(Host, Port, _connectionKey);
                if (_serverPeer == null)
                {
                    HandleConnectFailed("无法创建连接");
                    return;
                }

                Log.Info("[UdpChannel] 正在连接: {0}:{1}", Host, Port);
            }
            catch (Exception ex)
            {
                _netManager?.Stop();
                _netManager = null;
                HandleConnectFailed(ex.Message);
                Log.Error("[UdpChannel] 连接失败: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 处理连接失败
        /// </summary>
        private void HandleConnectFailed(string message)
        {
            if (_isReconnecting)
            {
                Log.Warn("[UdpChannel] 重连失败，继续尝试...");
                State = NetworkState.Disconnected;
                StartReconnect();
            }
            else
            {
                OnConnectFailed(message);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public override void Disconnect()
        {
            if (State == NetworkState.Disconnected)
                return;

            _isManualDisconnect = true;
            _isReconnecting = false;
            State = NetworkState.Disconnecting;

            try
            {
                if (_serverPeer != null)
                {
                    _serverPeer.Disconnect();
                    _serverPeer = null;
                }

                _netManager?.Stop();
                _netManager = null;
            }
            catch (Exception ex)
            {
                Log.Warn("[UdpChannel] 断开连接异常: {0}", ex.Message);
            }

            OnDisconnected(DisconnectReason.Manual);
            Log.Info("[UdpChannel] 已断开连接");
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public override void Send(int messageId, IMessage message)
        {
            if (!IsConnected || _serverPeer == null)
            {
                Log.Warn("[UdpChannel] 未连接，无法发送消息");
                return;
            }

            byte[] data = Encode(messageId, message);
            SendData(data, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// 发送消息（指定传输方式）
        /// </summary>
        public void Send(int messageId, IMessage message, DeliveryMethod deliveryMethod)
        {
            if (!IsConnected || _serverPeer == null)
            {
                Log.Warn("[UdpChannel] 未连接，无法发送消息");
                return;
            }

            byte[] data = Encode(messageId, message);
            SendData(data, deliveryMethod);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 处理重连逻辑
            if (_isReconnecting)
            {
                UpdateReconnect(elapseSeconds);
                return;
            }

            // 轮询LiteNetLib
            _netManager?.PollEvents();

            // 处理接收队列
            while (true)
            {
                UdpReceiveNetPacket packet = null;
                lock (_receiveLock)
                {
                    if (_receiveQueue.Count == 0)
                        break;
                    packet = _receiveQueue.Dequeue();
                }

                if (packet != null)
                {
                    UnpackNetPacket(packet);
                    ReferencePool.Release(packet);
                }
            }
        }

        /// <summary>
        /// 关闭并清理通道
        /// </summary>
        public override void Shutdown()
        {
            _isManualDisconnect = true;
            _isReconnecting = false;
            Disconnect();
            ClearReceiveQueue();
            base.Shutdown();
        }

        #endregion

        #region 重连相关

        private void StartReconnect()
        {
            if (!AutoReconnectEnabled || _isManualDisconnect)
                return;

            if (_reconnectCount >= MaxReconnectCount)
            {
                Log.Error("[UdpChannel] 重连次数已达上限 ({0})", MaxReconnectCount);
                _isReconnecting = false;
                OnReconnectFailed();
                return;
            }

            _isReconnecting = true;
            _reconnectWaitTime = 0;
            _reconnectCount++;

            Log.Info("[UdpChannel] 准备重连 ({0}/{1})，等待 {2} 秒...",
                _reconnectCount, MaxReconnectCount, ReconnectInterval);

            _networkManager?.OnChannelReconnecting(this, _reconnectCount, MaxReconnectCount);
        }

        private void UpdateReconnect(float elapseSeconds)
        {
            _reconnectWaitTime += elapseSeconds;

            if (_reconnectWaitTime >= ReconnectInterval)
            {
                _reconnectWaitTime = 0;
                DoConnect();
            }
        }

        private void OnReconnectFailed()
        {
            _networkManager?.OnChannelReconnectFailed(this);
        }

        #endregion

        #region 编解码

        /// <summary>
        /// 编码消息为字节数组
        /// 消息格式：[MessageId(2字节)] + [Body(变长)]
        /// </summary>
        public byte[] Encode(int messageId, IMessage message)
        {
            byte[] bodyData = message.ToByteArray();
            int totalLength = HeaderSize + bodyData.Length;
            byte[] data = new byte[totalLength];

            // 写入消息ID（2字节，小端序）
            data[0] = (byte)(messageId & 0xFF);
            data[1] = (byte)((messageId >> 8) & 0xFF);

            // 写入消息体
            Array.Copy(bodyData, 0, data, HeaderSize, bodyData.Length);

            return data;
        }

        /// <summary>
        /// 解码字节数组，返回 UdpNetPacket
        /// 消息格式：[MessageId(2字节)] + [Body(变长)]
        /// </summary>
        public UdpReceiveNetPacket Decode(byte[] data)
        {
            if (data == null || data.Length < HeaderSize)
                return null;

            // 读取消息ID（2字节）
            int messageId = data[0] | (data[1] << 8);

            // 提取消息体
            int bodyLength = data.Length - HeaderSize;
            byte[] body = new byte[bodyLength];
            Array.Copy(data, HeaderSize, body, 0, bodyLength);

            return UdpReceiveNetPacket.Create(messageId, body, bodyLength);
        }

        #endregion

        #region 发送相关

        /// <summary>
        /// 发送数据
        /// </summary>
        private void SendData(byte[] data, DeliveryMethod deliveryMethod)
        {
            try
            {
                _serverPeer.Send(data, deliveryMethod);
            }
            catch (Exception ex)
            {
                Log.Error("[UdpChannel] 发送失败: {0}", ex.Message);
            }
        }

        #endregion

        #region 接收相关

        /// <summary>
        /// 清空接收队列
        /// </summary>
        private void ClearReceiveQueue()
        {
            lock (_receiveLock)
            {
                while (_receiveQueue.Count > 0)
                {
                    UdpReceiveNetPacket packet = _receiveQueue.Dequeue();
                    ReferencePool.Release(packet);
                }
            }
        }

        #endregion

        #region INetEventListener 实现

        public void OnPeerConnected(NetPeer peer)
        {
            // 重连成功
            if (_isReconnecting)
            {
                _isReconnecting = false;
                _reconnectCount = 0;
                Log.Info("[UdpChannel] 重连成功: {0}", peer.Address);
                _serverPeer = peer;
                OnConnected();
                _networkManager?.OnChannelReconnected(this);
            }
            else
            {
                Log.Info("[UdpChannel] 连接成功: {0}", peer.Address);
                _serverPeer = peer;
                OnConnected();
            }
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Info("[UdpChannel] 断开连接: {0}, 原因: {1}", peer.Address, disconnectInfo.Reason);
            _serverPeer = null;

            // 停止NetManager
            _netManager?.Stop();
            _netManager = null;

            DisconnectReason reason;
            switch (disconnectInfo.Reason)
            {
                case LiteNetLib.DisconnectReason.ConnectionFailed:
                    reason = DisconnectReason.NetworkError;
                    break;
                case LiteNetLib.DisconnectReason.Timeout:
                    reason = DisconnectReason.Timeout;
                    break;
                case LiteNetLib.DisconnectReason.RemoteConnectionClose:
                    reason = DisconnectReason.ServerClosed;
                    break;
                case LiteNetLib.DisconnectReason.DisconnectPeerCalled:
                    reason = DisconnectReason.Manual;
                    break;
                default:
                    reason = DisconnectReason.Unknown;
                    break;
            }

            if (State != NetworkState.Disconnecting)
            {
                State = NetworkState.Disconnected;
                OnDisconnected(reason);

                // 非主动断开，尝试重连
                if (!_isManualDisconnect && AutoReconnectEnabled)
                {
                    StartReconnect();
                }
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Log.Error("[UdpChannel] 网络错误: {0}, {1}", endPoint, socketError);
            OnError(socketError.ToString());
        }

        /// <summary>
        /// 接收网络数据
        /// 消息格式：[MessageId(2字节)] + [Body(变长)]
        /// </summary>
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            try
            {
                // 读取原始数据
                byte[] data = new byte[reader.AvailableBytes];
                reader.GetBytes(data, data.Length);

                // 解码并加入接收队列
                UdpReceiveNetPacket packet = Decode(data);
                if (packet != null)
                {
                    lock (_receiveLock)
                    {
                        _receiveQueue.Enqueue(packet);
                    }
                }
                else
                {
                    Log.Error("[UdpChannel] 解码失败");
                }
            }
            catch (Exception ex)
            {
                Log.Error("[UdpChannel] 处理接收数据异常: {0}", ex.Message);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // 未连接消息，忽略
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // 延迟更新，可用于网络质量监控
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            // 客户端不接受连接请求
            request.Reject();
        }

        #endregion
    }
}
