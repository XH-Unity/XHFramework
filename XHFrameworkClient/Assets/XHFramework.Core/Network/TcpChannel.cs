using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;

namespace XHFramework.Core
{
    /// <summary>
    /// TCP网络通道
    /// 用于可靠的游戏逻辑传输（登录、背包、战斗指令等）
    /// 支持心跳检测和自动重连
    /// </summary>
    public class TcpChannel : NetworkChannelBase
    {
        #region 常量

        /// <summary>
        /// 最大消息长度（64KB）
        /// </summary>
        private const int MaxMessageSize = 65536;

        /// <summary>
        /// 连接超时（毫秒）
        /// </summary>
        private const int ConnectTimeout = 5000;

        /// <summary>
        /// TCP消息头长度（4字节消息体长度 + 2字节消息ID）
        /// </summary>
        private const int HeaderSize = 6;

        /// <summary>
        /// 心跳消息ID（约定一个特殊的消息ID）
        /// </summary>
        public const int HeartbeatMessageId = 0;

        /// <summary>
        /// 数据缓冲区初始大小（8KB）
        /// </summary>
        private const int InitialBufferSize = 8 * 1024;

        /// <summary>
        /// 数据缓冲区最大大小（1MB）
        /// </summary>
        private const int MaxBufferSize = 1024 * 1024;

        #endregion

        #region 字段

        /// <summary>
        /// TCP Socket
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// 接收缓冲区（从 Socket 读取数据的临时缓冲区）
        /// </summary>
        private readonly byte[] _receiveBuffer;

        /// <summary>
        /// 数据缓冲区（用于处理粘包，支持自动扩容和缩容）
        /// </summary>
        private byte[] _dataBuffer;

        /// <summary>
        /// 数据缓存中有效数据长度
        /// </summary>
        private int _dataLength;

        /// <summary>
        /// 发送线程
        /// </summary>
        private Thread _sendThread;

        /// <summary>
        /// 接收线程
        /// </summary>
        private Thread _receiveThread;

        /// <summary>
        /// 发送队列
        /// </summary>
        private readonly System.Collections.Generic.Queue<byte[]> _sendQueue = new System.Collections.Generic.Queue<byte[]>();

        /// <summary>
        /// 接收队列
        /// </summary>
        private readonly System.Collections.Generic.Queue<TcpReceiveNetPacket> _receiveQueue = new System.Collections.Generic.Queue<TcpReceiveNetPacket>();

        /// <summary>
        /// 发送队列锁
        /// </summary>
        private readonly object _sendLock = new object();

        /// <summary>
        /// 发送信号量
        /// </summary>
        private readonly AutoResetEvent _sendEvent = new AutoResetEvent(false);

        /// <summary>
        /// 是否正在运行
        /// </summary>
        private volatile bool _isRunning;

        // ========== 心跳相关 ==========

        /// <summary>
        /// 上次发送心跳的时间
        /// </summary>
        private float _lastHeartbeatSendTime;

        /// <summary>
        /// 上次收到心跳响应的时间
        /// </summary>
        private float _lastHeartbeatReceiveTime;

        /// <summary>
        /// 心跳超时计数
        /// </summary>
        private int _heartbeatTimeoutCount;

        // ========== 重连相关 ==========

        /// <summary>
        /// 当前重连次数
        /// </summary>
        private int _reconnectCount;

        /// <summary>
        /// 是否正在重连
        /// </summary>
        private volatile bool _isReconnecting;

        /// <summary>
        /// 重连等待时间累计
        /// </summary>
        private float _reconnectWaitTime;

        /// <summary>
        /// 是否是用户主动断开（主动断开不触发重连）
        /// </summary>
        private bool _isManualDisconnect;

        #endregion

        #region 属性

        /// <summary>
        /// 通道类型
        /// </summary>
        public override NetworkChannelType ChannelType => NetworkChannelType.Tcp;

        /// <summary>
        /// 是否启用心跳
        /// </summary>
        public bool HeartbeatEnabled { get; set; } = true;

        /// <summary>
        /// 心跳间隔（秒）
        /// </summary>
        public float HeartbeatInterval { get; set; } = 5f;

        /// <summary>
        /// 心跳超时时间（秒）
        /// </summary>
        public float HeartbeatTimeout { get; set; } = 15f;

        /// <summary>
        /// 最大心跳超时次数（超过此次数断开连接）
        /// </summary>
        public int MaxHeartbeatTimeoutCount { get; set; } = 3;

        /// <summary>
        /// 是否启用自动重连
        /// </summary>
        public bool AutoReconnectEnabled { get; set; } = true;

        /// <summary>
        /// 最大重连次数
        /// </summary>
        public int MaxReconnectCount { get; set; } = 5;

        /// <summary>
        /// 重连间隔（秒）
        /// </summary>
        public float ReconnectInterval { get; set; } = 3f;

        /// <summary>
        /// 是否正在重连中
        /// </summary>
        public bool IsReconnecting => _isReconnecting;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpChannel(string name, NetworkManager networkManager) : base(name, networkManager)
        {
            _receiveBuffer = new byte[MaxMessageSize];
            _dataBuffer = new byte[InitialBufferSize];
            _dataLength = 0;
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
                Log.Warn("[TcpChannel] 已经在连接或已连接状态，请先断开");
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
        /// 断开连接
        /// </summary>
        public override void Disconnect()
        {
            if (State == NetworkState.Disconnected)
                return;

            _isManualDisconnect = true;
            _isReconnecting = false;
            State = NetworkState.Disconnecting;
            StopThreads();

            try
            {
                if (_socket != null)
                {
                    if (_socket.Connected)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                    }
                    _socket.Close();
                    _socket = null;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("[TcpChannel] 断开连接异常: {0}", ex.Message);
            }

            _dataLength = 0;
            ResetBuffer();
            ClearSendQueue();
            OnDisconnected(DisconnectReason.Manual);
            Log.Info("[TcpChannel] 已断开连接");
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public override void Send(int messageId, IMessage message)
        {
            if (!IsConnected)
            {
                Log.Warn("[TcpChannel] 未连接，无法发送消息");
                return;
            }

            byte[] data = Encode(messageId, message);
            EnqueueSend(data);

            // 发送任何消息都重置心跳发送计时器
            _lastHeartbeatSendTime = 0;
        }

        /// <summary>
        /// 轮询处理接收队列
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 处理重连逻辑
            if (_isReconnecting)
            {
                UpdateReconnect(elapseSeconds);
                return;
            }

            // 处理心跳
            if (IsConnected && HeartbeatEnabled)
            {
                UpdateHeartbeat(elapseSeconds);
            }

            // 处理接收队列
            UpdateReceiveQueue();
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
            _sendEvent.Dispose();
            base.Shutdown();
        }

        #endregion

        #region 心跳相关

        /// <summary>
        /// 发送心跳包
        /// </summary>
        public void SendHeartbeat()
        {
            if (!IsConnected)
                return;

            // 发送空的心跳包：[Length=0] + [MessageId=0]
            byte[] heartbeatData = new byte[HeaderSize];
            // Length = 0 (4字节)
            heartbeatData[0] = 0;
            heartbeatData[1] = 0;
            heartbeatData[2] = 0;
            heartbeatData[3] = 0;
            // MessageId = 0 (2字节)
            heartbeatData[4] = 0;
            heartbeatData[5] = 0;

            EnqueueSend(heartbeatData);
            _lastHeartbeatSendTime = 0;
        }

        /// <summary>
        /// 更新心跳逻辑
        /// </summary>
        private void UpdateHeartbeat(float elapseSeconds)
        {
            _lastHeartbeatSendTime += elapseSeconds;
            _lastHeartbeatReceiveTime += elapseSeconds;

            // 发送心跳
            if (_lastHeartbeatSendTime >= HeartbeatInterval)
            {
                SendHeartbeat();
            }

            // 检查心跳超时
            if (_lastHeartbeatReceiveTime >= HeartbeatTimeout)
            {
                _heartbeatTimeoutCount++;
                _lastHeartbeatReceiveTime = 0;

                Log.Warn("[TcpChannel] 心跳超时 ({0}/{1})", _heartbeatTimeoutCount, MaxHeartbeatTimeoutCount);

                if (_heartbeatTimeoutCount >= MaxHeartbeatTimeoutCount)
                {
                    Log.Error("[TcpChannel] 心跳超时次数过多，断开连接");
                    HandleDisconnect(DisconnectReason.Timeout);
                }
            }
        }

        /// <summary>
        /// 收到心跳响应时调用
        /// </summary>
        private void OnHeartbeatReceived()
        {
            _lastHeartbeatReceiveTime = 0;
            _heartbeatTimeoutCount = 0;
        }

        /// <summary>
        /// 重置心跳状态
        /// </summary>
        private void ResetHeartbeat()
        {
            _lastHeartbeatSendTime = 0;
            _lastHeartbeatReceiveTime = 0;
            _heartbeatTimeoutCount = 0;
        }

        #endregion

        #region 重连相关

        /// <summary>
        /// 开始重连
        /// </summary>
        private void StartReconnect()
        {
            if (!AutoReconnectEnabled || _isManualDisconnect)
            {
                return;
            }

            if (_reconnectCount >= MaxReconnectCount)
            {
                Log.Error("[TcpChannel] 重连次数已达上限 ({0})", MaxReconnectCount);
                _isReconnecting = false;
                OnReconnectFailed();
                return;
            }

            _isReconnecting = true;
            _reconnectWaitTime = 0;
            _reconnectCount++;

            Log.Info("[TcpChannel] 准备重连 ({0}/{1})，等待 {2} 秒...",
                _reconnectCount, MaxReconnectCount, ReconnectInterval);

            // 通知上层正在重连
            _networkManager?.OnChannelReconnecting(this, _reconnectCount, MaxReconnectCount);
        }

        /// <summary>
        /// 更新重连逻辑
        /// </summary>
        private void UpdateReconnect(float elapseSeconds)
        {
            _reconnectWaitTime += elapseSeconds;

            if (_reconnectWaitTime >= ReconnectInterval)
            {
                _reconnectWaitTime = 0;
                DoConnect();
            }
        }

        /// <summary>
        /// 重连失败
        /// </summary>
        private void OnReconnectFailed()
        {
            _networkManager?.OnChannelReconnectFailed(this);
        }

        #endregion

        #region 缓冲区管理

        /// <summary>
        /// 确保缓冲区有足够空间（自动扩容）
        /// </summary>
        /// <param name="requiredLength">需要的总长度</param>
        /// <returns>是否成功确保空间</returns>
        private bool ExpandBuffer(int requiredLength)
        {
            if (requiredLength <= _dataBuffer.Length)
                return true;

            // 超过最大限制，拒绝扩容
            if (requiredLength > MaxBufferSize)
            {
                Log.Error("[TcpChannel] 数据超过最大缓冲区限制: {0} > {1}", requiredLength, MaxBufferSize);
                return false;
            }

            // 扩容：翻倍增长，但不超过最大值
            int newSize = _dataBuffer.Length;
            while (newSize < requiredLength)
            {
                newSize = Math.Min(newSize * 2, MaxBufferSize);
            }

            byte[] newBuffer = new byte[newSize];
            if (_dataLength > 0)
            {
                Array.Copy(_dataBuffer, 0, newBuffer, 0, _dataLength);
            }
            _dataBuffer = newBuffer;

            Log.Info("[TcpChannel] 缓冲区扩容: {0} -> {1} bytes", _dataBuffer.Length, newSize);
            return true;
        }

        /// <summary>
        /// 尝试缩容缓冲区（在处理完消息后调用）
        /// 当缓冲区使用率低于 25% 且大于初始大小的 4 倍时进行缩容
        /// </summary>
        private void ShrinkBuffer()
        {
            // 条件：缓冲区大于初始大小的4倍，且数据量不足缓冲区的1/4
            if (_dataBuffer.Length > InitialBufferSize * 4 &&
                _dataLength < _dataBuffer.Length / 4)
            {
                // 缩容为当前大小的一半，但不小于初始大小
                int newSize = Math.Max(_dataBuffer.Length / 2, InitialBufferSize);

                // 确保新大小能容纳现有数据
                if (newSize >= _dataLength)
                {
                    byte[] newBuffer = new byte[newSize];
                    if (_dataLength > 0)
                    {
                        Array.Copy(_dataBuffer, 0, newBuffer, 0, _dataLength);
                    }

                    Log.Info("[TcpChannel] 缓冲区缩容: {0} -> {1} bytes", _dataBuffer.Length, newSize);
                    _dataBuffer = newBuffer;
                }
            }
        }

        /// <summary>
        /// 重置缓冲区到初始大小
        /// </summary>
        private void ResetBuffer()
        {
            if (_dataBuffer.Length != InitialBufferSize)
            {
                _dataBuffer = new byte[InitialBufferSize];
            }
            _dataLength = 0;
        }

        #endregion

        #region 编解码

        /// <summary>
        /// 编码消息为字节数组
        /// 消息格式：[Length(4字节)] + [MessageId(2字节)] + [Body(变长)]
        /// </summary>
        public byte[] Encode(int messageId, IMessage message)
        {
            byte[] bodyData = message.ToByteArray();
            int totalLength = HeaderSize + bodyData.Length;
            byte[] data = new byte[totalLength];

            // 写入消息体长度（4字节，小端序）
            int bodyLength = bodyData.Length;
            data[0] = (byte)(bodyLength & 0xFF);
            data[1] = (byte)((bodyLength >> 8) & 0xFF);
            data[2] = (byte)((bodyLength >> 16) & 0xFF);
            data[3] = (byte)((bodyLength >> 24) & 0xFF);

            // 写入消息ID（2字节，小端序）
            data[4] = (byte)(messageId & 0xFF);
            data[5] = (byte)((messageId >> 8) & 0xFF);

            // 写入消息体
            Array.Copy(bodyData, 0, data, HeaderSize, bodyData.Length);

            return data;
        }

        /// <summary>
        /// 解码字节数组，返回 TcpNetPacket
        /// 消息格式：[Length(4字节)] + [MessageId(2字节)] + [Body(变长)]
        /// </summary>
        public TcpReceiveNetPacket Decode(byte[] data)
        {
            if (data == null || data.Length < HeaderSize)
                return null;

            // 读取消息体长度
            int bodyLength = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);

            if (bodyLength < 0 || bodyLength > MaxMessageSize || data.Length < HeaderSize + bodyLength)
                return null;

            // 读取消息ID（2字节）
            int messageId = data[4] | (data[5] << 8);

            // 提取消息体
            byte[] body = new byte[bodyLength];
            Array.Copy(data, HeaderSize, body, 0, bodyLength);

            return TcpReceiveNetPacket.Create(messageId, body, bodyLength);
        }

        #endregion

        #region 发送相关

        /// <summary>
        /// 将数据加入发送队列
        /// </summary>
        private void EnqueueSend(byte[] data)
        {
            lock (_sendLock)
            {
                _sendQueue.Enqueue(data);
            }
            _sendEvent.Set();
        }

        /// <summary>
        /// 清空发送队列
        /// </summary>
        private void ClearSendQueue()
        {
            lock (_sendLock)
            {
                _sendQueue.Clear();
            }
        }

        /// <summary>
        /// 发送循环
        /// </summary>
        private void SendLoop()
        {
            while (_isRunning)
            {
                _sendEvent.WaitOne();

                while (_isRunning)
                {
                    byte[] data = null;
                    lock (_sendLock)
                    {
                        if (_sendQueue.Count == 0)
                            break;
                        data = _sendQueue.Dequeue();
                    }

                    if (data != null)
                    {
                        try
                        {
                            _socket?.Send(data);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("[TcpChannel] 发送失败: {0}", ex.Message);
                            HandleDisconnect(DisconnectReason.NetworkError);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region 接收相关

        /// <summary>
        /// 处理接收队列
        /// </summary>
        private void UpdateReceiveQueue()
        {
            while (true)
            {
                TcpReceiveNetPacket packet = null;
                lock (_receiveLock)
                {
                    if (_receiveQueue.Count == 0)
                        break;
                    packet = _receiveQueue.Dequeue();
                }

                if (packet != null)
                {
                    // 收到任何消息都重置心跳接收计时器
                    OnHeartbeatReceived();

                    // 心跳包不需要处理，直接释放
                    if (packet.MessageId == HeartbeatMessageId)
                    {
                        ReferencePool.Release(packet);
                        continue;
                    }

                    UnpackNetPacket(packet);
                    ReferencePool.Release(packet);
                }
            }
        }

        /// <summary>
        /// 清空接收队列
        /// </summary>
        private void ClearReceiveQueue()
        {
            lock (_receiveLock)
            {
                while (_receiveQueue.Count > 0)
                {
                    TcpReceiveNetPacket packet = _receiveQueue.Dequeue();
                    ReferencePool.Release(packet);
                }
            }
        }

        /// <summary>
        /// 接收循环
        /// </summary>
        private void ReceiveLoop()
        {
            while (_isRunning && _socket != null && _socket.Connected)
            {
                try
                {
                    int bytesRead = _socket.Receive(_receiveBuffer);
                    if (bytesRead <= 0)
                    {
                        // 服务器关闭连接
                        HandleDisconnect(DisconnectReason.ServerClosed);
                        return;
                    }

                    // 检查并扩容缓冲区
                    if (!ExpandBuffer(_dataLength + bytesRead))
                    {
                        HandleDisconnect(DisconnectReason.NetworkError);
                        return;
                    }

                    // 将接收到的数据追加到缓存
                    Array.Copy(_receiveBuffer, 0, _dataBuffer, _dataLength, bytesRead);
                    _dataLength += bytesRead;

                    // 处理粘包
                    ProcessReceivedData();

                    // 处理完消息后尝试缩容
                    ShrinkBuffer();
                }
                catch (SocketException ex)
                {
                    if (_isRunning)
                    {
                        Log.Error("[TcpChannel] 接收异常: {0}", ex.Message);
                        HandleDisconnect(DisconnectReason.NetworkError);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Log.Error("[TcpChannel] 接收异常: {0}", ex.Message);
                        HandleDisconnect(DisconnectReason.NetworkError);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// 处理接收到的数据（粘包处理）
        /// 消息格式：[Length(4字节)] + [MessageId(2字节)] + [Body(变长)]
        /// </summary>
        private void ProcessReceivedData()
        {
            while (_dataLength >= HeaderSize)
            {
                // 先读取消息体长度（长度在前，便于判断数据完整性）
                int bodyLength = _dataBuffer[0] | (_dataBuffer[1] << 8) | (_dataBuffer[2] << 16) | (_dataBuffer[3] << 24);

                // 验证消息长度
                if (bodyLength < 0 || bodyLength > MaxMessageSize)
                {
                    Log.Error("[TcpChannel] 无效的消息长度: {0}", bodyLength);
                    HandleDisconnect(DisconnectReason.NetworkError);
                    return;
                }

                int totalLength = HeaderSize + bodyLength;

                // 数据不完整，等待更多数据
                if (_dataLength < totalLength)
                    return;

                // 提取完整的消息数据
                byte[] packetData = new byte[totalLength];
                Array.Copy(_dataBuffer, 0, packetData, 0, totalLength);

                // 解码并加入接收队列
                TcpReceiveNetPacket packet = Decode(packetData);
                if (packet != null)
                {
                    lock (_receiveLock)
                    {
                        _receiveQueue.Enqueue(packet);
                    }
                }
                else
                {
                    Log.Error("[TcpChannel] 解码失败");
                    HandleDisconnect(DisconnectReason.NetworkError);
                    return;
                }

                // 移除已处理的数据
                int remainingLength = _dataLength - totalLength;
                if (remainingLength > 0)
                {
                    Array.Copy(_dataBuffer, totalLength, _dataBuffer, 0, remainingLength);
                }
                _dataLength = remainingLength;
            }
        }

        #endregion

        #region 线程管理

        /// <summary>
        /// 启动收发线程
        /// </summary>
        private void StartThreads()
        {
            _isRunning = true;

            _sendThread = new Thread(SendLoop)
            {
                IsBackground = true,
                Name = $"TcpChannel_{Name}_Send"
            };
            _sendThread.Start();

            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = $"TcpChannel_{Name}_Receive"
            };
            _receiveThread.Start();
        }

        /// <summary>
        /// 停止收发线程
        /// </summary>
        private void StopThreads()
        {
            _isRunning = false;
            _sendEvent.Set(); // 唤醒发送线程使其退出

            if (_sendThread != null)
            {
                if (!_sendThread.Join(1000))
                {
                    Log.Warn("[TcpChannel] 发送线程未能在超时时间内结束");
                }
                _sendThread = null;
            }

            if (_receiveThread != null)
            {
                // 接收线程可能阻塞在Receive，关闭socket会使其抛出异常退出
                if (!_receiveThread.Join(1000))
                {
                    Log.Warn("[TcpChannel] 接收线程未能在超时时间内结束");
                }
                _receiveThread = null;
            }
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 执行连接
        /// </summary>
        private void DoConnect()
        {
            State = NetworkState.Connecting;

            try
            {
                // 解析主机地址
                IPAddress[] addresses = Dns.GetHostAddresses(Host);
                if (addresses.Length == 0)
                {
                    OnConnectFailed("无法解析主机地址: " + Host);
                    if (_isReconnecting)
                    {
                        StartReconnect();
                    }
                    return;
                }

                // 创建Socket
                _socket = new Socket(addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.NoDelay = true;
                _socket.SendTimeout = 5000;
                _socket.ReceiveTimeout = 0; // 接收不超时

                // 异步连接
                IAsyncResult result = _socket.BeginConnect(addresses[0], Port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(ConnectTimeout, true);

                if (success && _socket.Connected)
                {
                    _socket.EndConnect(result);
                    ResetBuffer();
                    ResetHeartbeat();
                    StartThreads();

                    // 重连成功
                    if (_isReconnecting)
                    {
                        _isReconnecting = false;
                        _reconnectCount = 0;
                        Log.Info("[TcpChannel] 重连成功: {0}:{1}", Host, Port);
                        OnConnected();
                        _networkManager?.OnChannelReconnected(this);
                    }
                    else
                    {
                        Log.Info("[TcpChannel] 连接成功: {0}:{1}", Host, Port);
                        OnConnected();
                    }
                }
                else
                {
                    _socket?.Close();
                    _socket = null;

                    if (_isReconnecting)
                    {
                        Log.Warn("[TcpChannel] 重连失败，继续尝试...");
                        StartReconnect();
                    }
                    else
                    {
                        OnConnectFailed("连接超时");
                    }
                }
            }
            catch (Exception ex)
            {
                _socket?.Close();
                _socket = null;

                if (_isReconnecting)
                {
                    Log.Warn("[TcpChannel] 重连异常: {0}，继续尝试...", ex.Message);
                    StartReconnect();
                }
                else
                {
                    OnConnectFailed(ex.Message);
                    Log.Error("[TcpChannel] 连接失败: {0}", ex.Message);
                }
            }
        }

        /// <summary>
        /// 处理断开连接
        /// </summary>
        private void HandleDisconnect(DisconnectReason reason)
        {
            if (State == NetworkState.Disconnected || State == NetworkState.Disconnecting)
                return;

            _isRunning = false;
            State = NetworkState.Disconnected;

            try
            {
                _socket?.Close();
            }
            catch { }
            _socket = null;

            // 通知断开
            OnDisconnected(reason);

            // 非主动断开，尝试重连
            if (!_isManualDisconnect && AutoReconnectEnabled)
            {
                StartReconnect();
            }
        }

        #endregion
    }
}
