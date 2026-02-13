using System.Collections.Generic;
using XHFramework.Core;

namespace XHFramework.Game
{
    /// <summary>
    /// HTTP 配置（热更层）
    /// 配置 HTTP 请求的基础参数
    /// </summary>
    public static class HttpConfig
    {
        #region 基础配置

        /// <summary>
        /// API 基础地址
        /// </summary>
        public static readonly string BaseUrl = "http://127.0.0.1:8080";

        /// <summary>
        /// 默认超时时间（秒）
        /// </summary>
        public static readonly int DefaultTimeout = 30;

        /// <summary>
        /// 默认请求头
        /// </summary>
        public static readonly Dictionary<string, string> DefaultHeaders = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Accept", "application/json" },
            // { "X-Client-Version", "1.0.0" },
        };

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化 HTTP 配置（热更层入口调用）
        /// </summary>
        public static void InitHttp()
        {
            var http = FW.HttpManager;
            if (http == null) return;

            // 配置基础 URL
            http.BaseUrl = BaseUrl;

            // 配置超时时间
            http.DefaultTimeout = DefaultTimeout;

            // 配置默认请求头
            foreach (var header in DefaultHeaders)
            {
                http.DefaultHeaders[header.Key] = header.Value;
            }

            Log.Info("HttpConfig 初始化完成");
        }

        #endregion

       

       
    }
}
