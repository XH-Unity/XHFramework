using System;
using Google.Protobuf;

namespace XHFramework.Core
{
    /// <summary>
    /// 通道连接成功事件参数
    /// </summary>
    public class NetworkConnectedEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkConnectedEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; private set; }

        public static NetworkConnectedEventArgs Create(NetworkChannelType channelType, string channelName, string host, int port)
        {
            NetworkConnectedEventArgs args = ReferencePool.Acquire<NetworkConnectedEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            args.Host = host;
            args.Port = port;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
            Host = null;
            Port = 0;
        }
    }

    /// <summary>
    /// 通道断开连接事件参数
    /// </summary>
    public class NetworkDisconnectedEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkDisconnectedEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// 断开原因
        /// </summary>
        public DisconnectReason Reason { get; private set; }

        public static NetworkDisconnectedEventArgs Create(NetworkChannelType channelType, string channelName, DisconnectReason reason)
        {
            NetworkDisconnectedEventArgs args = ReferencePool.Acquire<NetworkDisconnectedEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            args.Reason = reason;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
            Reason = default;
        }
    }

    /// <summary>
    /// 通道连接失败事件参数
    /// </summary>
    public class NetworkConnectFailedEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkConnectFailedEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        public static NetworkConnectFailedEventArgs Create(NetworkChannelType channelType, string channelName, string errorMessage)
        {
            NetworkConnectFailedEventArgs args = ReferencePool.Acquire<NetworkConnectFailedEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            args.ErrorMessage = errorMessage;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
            ErrorMessage = null;
        }
    }

    /// <summary>
    /// 通道错误事件参数
    /// </summary>
    public class NetworkErrorEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkErrorEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        public static NetworkErrorEventArgs Create(NetworkChannelType channelType, string channelName, string errorMessage)
        {
            NetworkErrorEventArgs args = ReferencePool.Acquire<NetworkErrorEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            args.ErrorMessage = errorMessage;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
            ErrorMessage = null;
        }
    }

    /// <summary>
    /// 通道正在重连事件参数
    /// </summary>
    public class NetworkReconnectingEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkReconnectingEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// 当前重连次数
        /// </summary>
        public int CurrentCount { get; private set; }

        /// <summary>
        /// 最大重连次数
        /// </summary>
        public int MaxCount { get; private set; }

        public static NetworkReconnectingEventArgs Create(NetworkChannelType channelType, string channelName, int currentCount, int maxCount)
        {
            NetworkReconnectingEventArgs args = ReferencePool.Acquire<NetworkReconnectingEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            args.CurrentCount = currentCount;
            args.MaxCount = maxCount;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
            CurrentCount = 0;
            MaxCount = 0;
        }
    }

    /// <summary>
    /// 通道重连成功事件参数
    /// </summary>
    public class NetworkReconnectedEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkReconnectedEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; private set; }

        public static NetworkReconnectedEventArgs Create(NetworkChannelType channelType, string channelName, string host, int port)
        {
            NetworkReconnectedEventArgs args = ReferencePool.Acquire<NetworkReconnectedEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            args.Host = host;
            args.Port = port;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
            Host = null;
            Port = 0;
        }
    }

    /// <summary>
    /// 通道重连失败事件参数
    /// </summary>
    public class NetworkReconnectFailedEventArgs : GlobalEventArgs
    {
        public override int Id => typeof(NetworkReconnectFailedEventArgs).GetHashCode();

        /// <summary>
        /// 通道类型
        /// </summary>
        public NetworkChannelType ChannelType { get; private set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; private set; }

        public static NetworkReconnectFailedEventArgs Create(NetworkChannelType channelType, string channelName)
        {
            NetworkReconnectFailedEventArgs args = ReferencePool.Acquire<NetworkReconnectFailedEventArgs>();
            args.ChannelType = channelType;
            args.ChannelName = channelName;
            return args;
        }

        public override void Clear()
        {
            ChannelType = default;
            ChannelName = null;
        }
    }
}
