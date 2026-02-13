namespace XHFramework.Core
{
    /// <summary>
    /// 网络通道类型
    /// </summary>
    public enum NetworkChannelType
    {
        /// <summary>
        /// TCP通道 - 用于可靠的游戏逻辑传输
        /// </summary>
        Tcp = 0,

        /// <summary>
        /// UDP通道 - 用于实时性要求高的同步（如移动、战斗）
        /// </summary>
        Udp = 1,

        /// <summary>
        /// WebSocket通道 - 用于实时推送、匹配等场景
        /// </summary>
        WebSocket = 2
    }

    /// <summary>
    /// 网络连接状态
    /// </summary>
    public enum NetworkState
    {
        /// <summary>
        /// 未连接
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// 连接中
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 2,

        /// <summary>
        /// 断开中
        /// </summary>
        Disconnecting = 3
    }

    /// <summary>
    /// 断开连接原因
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>
        /// 未知原因
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 主动断开
        /// </summary>
        Manual = 1,

        /// <summary>
        /// 连接超时
        /// </summary>
        Timeout = 2,

        /// <summary>
        /// 服务器关闭
        /// </summary>
        ServerClosed = 3,

        /// <summary>
        /// 网络错误
        /// </summary>
        NetworkError = 4,

        /// <summary>
        /// 被踢出
        /// </summary>
        Kicked = 5
    }
}
