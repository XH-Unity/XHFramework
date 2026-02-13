using System;
using Google.Protobuf;
using XHFramework.Core;

namespace XHFramework.Game
{
    /// <summary>
    /// 网络配置（热更层）
    /// 配置三种网络通道的参数：TCP、UDP、WebSocket
    /// </summary>
    public static class NetworkConfig
    {
        #region 服务器地址配置

        /// <summary>
        /// TCP 服务器地址
        /// </summary>
        public static readonly string TcpHost = "127.0.0.1";

        /// <summary>
        /// TCP 服务器端口
        /// </summary>
        public static readonly int TcpPort = 9001;

        /// <summary>
        /// UDP 服务器地址
        /// </summary>
        public static readonly string UdpHost = "127.0.0.1";

        /// <summary>
        /// UDP 服务器端口
        /// </summary>
        public static readonly int UdpPort = 9002;

        /// <summary>
        /// WebSocket 服务器地址
        /// </summary>
        public static readonly string WsHost = "127.0.0.1";

        /// <summary>
        /// WebSocket 服务器端口
        /// </summary>
        public static readonly int WsPort = 9003;

        /// <summary>
        /// WebSocket 是否使用 SSL
        /// </summary>
        public static readonly bool WsUseSsl = false;

        #endregion

        #region TCP 通道配置

        /// <summary>
        /// TCP 是否启用心跳
        /// </summary>
        public static readonly bool TcpHeartbeatEnabled = true;

        /// <summary>
        /// TCP 心跳间隔（秒）
        /// </summary>
        public static readonly float TcpHeartbeatInterval = 5f;

        /// <summary>
        /// TCP 心跳超时时间（秒）
        /// </summary>
        public static readonly float TcpHeartbeatTimeout = 15f;

        /// <summary>
        /// TCP 最大心跳超时次数
        /// </summary>
        public static readonly int TcpMaxHeartbeatTimeoutCount = 3;

        /// <summary>
        /// TCP 是否启用自动重连
        /// </summary>
        public static readonly bool TcpAutoReconnectEnabled = true;

        /// <summary>
        /// TCP 最大重连次数
        /// </summary>
        public static readonly int TcpMaxReconnectCount = 5;

        /// <summary>
        /// TCP 重连间隔（秒）
        /// </summary>
        public static readonly float TcpReconnectInterval = 3f;

        #endregion

        #region UDP 通道配置

        /// <summary>
        /// UDP 是否启用自动重连
        /// </summary>
        public static readonly bool UdpAutoReconnectEnabled = true;

        /// <summary>
        /// UDP 最大重连次数
        /// </summary>
        public static readonly int UdpMaxReconnectCount = 5;

        /// <summary>
        /// UDP 重连间隔（秒）
        /// </summary>
        public static readonly float UdpReconnectInterval = 3f;

        /// <summary>
        /// UDP 连接密钥
        /// </summary>
        public static readonly string UdpConnectionKey = "XHFramework";

        #endregion

        #region WebSocket 通道配置

        /// <summary>
        /// WebSocket 是否启用心跳
        /// </summary>
        public static readonly bool WsHeartbeatEnabled = true;

        /// <summary>
        /// WebSocket 心跳间隔（秒）
        /// </summary>
        public static readonly float WsHeartbeatInterval = 5f;

        /// <summary>
        /// WebSocket 心跳超时时间（秒）
        /// </summary>
        public static readonly float WsHeartbeatTimeout = 15f;

        /// <summary>
        /// WebSocket 是否启用自动重连
        /// </summary>
        public static readonly bool WsAutoReconnectEnabled = true;

        /// <summary>
        /// WebSocket 最大重连次数
        /// </summary>
        public static readonly int WsMaxReconnectCount = 5;

        /// <summary>
        /// WebSocket 重连间隔（秒）
        /// </summary>
        public static readonly float WsReconnectInterval = 3f;

        /// <summary>
        /// WebSocket 心跳消息ID
        /// </summary>
        public static readonly int WsHeartbeatMessageId = 1;

        /// <summary>
        /// WebSocket 心跳消息工厂（返回心跳消息，null 则使用 WebSocket Ping）
        /// </summary>
        public static Func<IMessage> WsHeartbeatMessageFactory = null;

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化网络配置（热更层入口调用）
        /// </summary>
        public static void InitNetwork()
        {
            // 配置 TCP 通道
            ConfigureTcpChannel();

            // 配置 UDP 通道
            ConfigureUdpChannel();

            // 配置 WebSocket 通道
            ConfigureWebSocketChannel();

            Log.Info("NetworkConfig 初始化完成");
        }

        /// <summary>
        /// 配置 TCP 通道
        /// </summary>
        private static void ConfigureTcpChannel()
        {
            var tcp = FW.NetworkManager.TcpChannel;
            if (tcp == null) return;

            // 心跳配置
            tcp.HeartbeatEnabled = TcpHeartbeatEnabled;
            tcp.HeartbeatInterval = TcpHeartbeatInterval;
            tcp.HeartbeatTimeout = TcpHeartbeatTimeout;
            tcp.MaxHeartbeatTimeoutCount = TcpMaxHeartbeatTimeoutCount;

            // 重连配置
            tcp.AutoReconnectEnabled = TcpAutoReconnectEnabled;
            tcp.MaxReconnectCount = TcpMaxReconnectCount;
            tcp.ReconnectInterval = TcpReconnectInterval;
        }

        /// <summary>
        /// 配置 UDP 通道
        /// </summary>
        private static void ConfigureUdpChannel()
        {
            var udp = FW.NetworkManager.UdpChannel;
            if (udp == null) return;

            // 重连配置
            udp.AutoReconnectEnabled = UdpAutoReconnectEnabled;
            udp.MaxReconnectCount = UdpMaxReconnectCount;
            udp.ReconnectInterval = UdpReconnectInterval;

            // 连接密钥
            udp.SetConnectionKey(UdpConnectionKey);
        }

        /// <summary>
        /// 配置 WebSocket 通道
        /// </summary>
        private static void ConfigureWebSocketChannel()
        {
            var ws = FW.NetworkManager.WebSocketChannel;
            if (ws == null) return;

            // SSL 配置
            ws.UseSsl = WsUseSsl;

            // 心跳配置
            ws.HeartbeatEnabled = WsHeartbeatEnabled;
            ws.HeartbeatInterval = WsHeartbeatInterval;
            ws.HeartbeatTimeout = WsHeartbeatTimeout;
            ws.HeartbeatMessageId = WsHeartbeatMessageId;
            ws.HeartbeatMessageFactory = WsHeartbeatMessageFactory;

            // 重连配置
            ws.AutoReconnectEnabled = WsAutoReconnectEnabled;
            ws.MaxReconnectCount = WsMaxReconnectCount;
            ws.ReconnectInterval = WsReconnectInterval;
        }

        #endregion

        #region 便捷连接方法

        /// <summary>
        /// 连接 TCP 服务器（使用默认配置）
        /// </summary>
        public static void ConnectTcp()
        {
            FW.NetworkManager.TcpChannel?.Connect(TcpHost, TcpPort);
        }

        /// <summary>
        /// 连接 TCP 服务器（指定地址）
        /// </summary>
        public static void ConnectTcp(string host, int port)
        {
            FW.NetworkManager.TcpChannel?.Connect(host, port);
        }

        /// <summary>
        /// 连接 UDP 服务器（使用默认配置）
        /// </summary>
        public static void ConnectUdp()
        {
            FW.NetworkManager.UdpChannel?.Connect(UdpHost, UdpPort);
        }

        /// <summary>
        /// 连接 UDP 服务器（指定地址）
        /// </summary>
        public static void ConnectUdp(string host, int port)
        {
            FW.NetworkManager.UdpChannel?.Connect(host, port);
        }

        /// <summary>
        /// 连接 WebSocket 服务器（使用默认配置）
        /// </summary>
        public static void ConnectWebSocket()
        {
            FW.NetworkManager.WebSocketChannel?.Connect(WsHost, WsPort);
        }

        /// <summary>
        /// 连接 WebSocket 服务器（指定地址）
        /// </summary>
        public static void ConnectWebSocket(string host, int port)
        {
            FW.NetworkManager.WebSocketChannel?.Connect(host, port);
        }

        /// <summary>
        /// 连接 WebSocket 服务器（使用完整 URL）
        /// </summary>
        public static void ConnectWebSocket(string url)
        {
            FW.NetworkManager.WebSocketChannel?.Connect(url);
        }

        #endregion
    }
}
