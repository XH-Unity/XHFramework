using System;
using Google.Protobuf;

namespace XHFramework.Core
{
    /// <summary>
    /// 网络通道接口
    /// </summary>
    public interface INetworkChannel
    {
        /// <summary>
        /// 通道名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 通道类型
        /// </summary>
        NetworkChannelType ChannelType { get; }

        /// <summary>
        /// 当前连接状态
        /// </summary>
        NetworkState State { get; }

        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        string Host { get; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 连接服务器
        /// </summary>
        void Connect(string host, int port);

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 发送消息
        /// </summary>
        void Send(int messageId, IMessage message);

        /// <summary>
        /// 轮询（处理接收队列）
        /// </summary>
        void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理通道
        /// </summary>
        void Shutdown();
    }
}
