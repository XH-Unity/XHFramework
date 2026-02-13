using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace XHFramework.Core
{
    /// <summary>
    /// 网络管理器
    /// 统一管理TCP、UDP、WebSocket三种网络通道
    /// </summary>
    public class NetworkManager : ManagerBase
    {
        /// <summary>
        /// 模块优先级
        /// </summary>
        public override int Priority => 80;

        /// <summary>
        /// TCP通道
        /// </summary>
        public TcpChannel TcpChannel { get; private set; }

        /// <summary>
        /// UDP通道
        /// </summary>
        public UdpChannel UdpChannel { get; private set; }

        /// <summary>
        /// WebSocket通道
        /// </summary>
        public WebSocketChannel WebSocketChannel { get; private set; }

        /// <summary>
        /// 所有通道列表
        /// </summary>
        private readonly List<NetworkChannelBase> _channels = new List<NetworkChannelBase>();

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            // 创建三个通道
            TcpChannel = new TcpChannel("TcpChannel", this);
            UdpChannel = new UdpChannel("UdpChannel", this);
            WebSocketChannel = new WebSocketChannel("WebSocketChannel", this);

            _channels.Add(TcpChannel);
            _channels.Add(UdpChannel);
            _channels.Add(WebSocketChannel);

            Log.Info("NetworkManager初始化完成");
        }

        /// <summary>
        /// 轮询
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 轮询所有通道
            foreach (var channel in _channels)
            {
                channel.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理
        /// </summary>
        public override void Shutdown()
        {
            // 关闭所有通道
            foreach (var channel in _channels)
            {
                channel.Shutdown();
            }
            _channels.Clear();

            TcpChannel = null;
            UdpChannel = null;
            WebSocketChannel = null;

            Log.Info("NetworkManager已关闭");
        }

      
        /// <summary>
        /// 断开所有连接
        /// </summary>
        public void DisconnectAll()
        {
            foreach (var channel in _channels)
            {
                channel.Disconnect();
            }
        }

        /// <summary>
        /// 通道连接成功回调
        /// </summary>
        internal void OnChannelConnected(NetworkChannelBase channel)
        {
            var eventArgs = NetworkConnectedEventArgs.Create(
                channel.ChannelType,
                channel.Name,
                channel.Host,
                channel.Port);
            Log.Info($"{channel.Name}频道连接成功，地址:{channel.Host}端口:{channel.Port}");
            FW.EventManager?.Fire(this, eventArgs);
        }

        /// <summary>
        /// 通道断开连接回调
        /// </summary>
        internal void OnChannelDisconnected(NetworkChannelBase channel, DisconnectReason reason)
        {
            var eventArgs = NetworkDisconnectedEventArgs.Create(
                channel.ChannelType,
                channel.Name,
                reason);
            Log.Error($"{channel.Name}频道断开连接,断开原因:{reason}");
            FW.EventManager?.Fire(this, eventArgs);
        }

        /// <summary>
        /// 通道连接失败回调
        /// </summary>
        internal void OnChannelConnectFailed(NetworkChannelBase channel, string error)
        {
            var eventArgs = NetworkConnectFailedEventArgs.Create(
                channel.ChannelType,
                channel.Name,
                error);
            Log.Error($"{channel.Name}频道连接失败,失败原因:{error}");
            FW.EventManager?.Fire(this, eventArgs);
        }

        /// <summary>
        /// 通道错误回调
        /// </summary>
        internal void OnChannelError(NetworkChannelBase channel, string error)
        {
            var eventArgs = NetworkErrorEventArgs.Create(
                channel.ChannelType,
                channel.Name,
                error);
            Log.Error($"{channel.Name}频道错误,错误信息:{error}");
            FW.EventManager?.Fire(this, eventArgs);
        }

        /// <summary>
        /// 通道正在重连回调
        /// </summary>
        internal void OnChannelReconnecting(NetworkChannelBase channel, int currentCount, int maxCount)
        {
            var eventArgs = NetworkReconnectingEventArgs.Create(
                channel.ChannelType,
                channel.Name,
                currentCount,
                maxCount);
            Log.Info($"{channel.Name}频道正在重连 ({currentCount}/{maxCount})");
            FW.EventManager?.Fire(this, eventArgs);
        }

        /// <summary>
        /// 通道重连成功回调
        /// </summary>
        internal void OnChannelReconnected(NetworkChannelBase channel)
        {
            var eventArgs = NetworkReconnectedEventArgs.Create(
                channel.ChannelType,
                channel.Name,
                channel.Host,
                channel.Port);
            Log.Info($"{channel.Name}频道重连成功");
            FW.EventManager?.Fire(this, eventArgs);
        }

        /// <summary>
        /// 通道重连失败回调
        /// </summary>
        internal void OnChannelReconnectFailed(NetworkChannelBase channel)
        {
            var eventArgs = NetworkReconnectFailedEventArgs.Create(
                channel.ChannelType,
                channel.Name);
            Log.Error($"{channel.Name}频道重连失败，已达最大重连次数");
            FW.EventManager?.Fire(this, eventArgs);
        }
    }
}
