namespace XHFramework.Core
{
    /// <summary>
    /// HTTP 请求结果枚举
    /// </summary>
    public enum HttpResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 网络错误（无网络、DNS解析失败等）
        /// </summary>
        NetworkError = 1,

        /// <summary>
        /// 连接超时
        /// </summary>
        Timeout = 2,

        /// <summary>
        /// 服务器错误（5xx）
        /// </summary>
        ServerError = 3,

        /// <summary>
        /// 客户端错误（4xx）
        /// </summary>
        ClientError = 4,

        /// <summary>
        /// JSON 解析错误
        /// </summary>
        ParseError = 5,

        /// <summary>
        /// 未知错误
        /// </summary>
        Unknown = 99
    }
}
