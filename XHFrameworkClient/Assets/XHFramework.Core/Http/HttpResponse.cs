using System.Collections.Generic;

namespace XHFramework.Core
{
    /// <summary>
    /// HTTP 响应封装
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// 请求结果
        /// </summary>
        public HttpResult Result { get; set; }

        /// <summary>
        /// HTTP 状态码
        /// </summary>
        public long StatusCode { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => Result == HttpResult.Success;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 原始响应文本
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// 原始响应字节
        /// </summary>
        public byte[] RawBytes { get; set; }

        /// <summary>
        /// 响应头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static HttpResponse Success(long statusCode, string rawText, byte[] rawBytes, Dictionary<string, string> headers)
        {
            return new HttpResponse
            {
                Result = HttpResult.Success,
                StatusCode = statusCode,
                RawText = rawText,
                RawBytes = rawBytes,
                Headers = headers
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static HttpResponse Fail(HttpResult result, long statusCode, string error)
        {
            return new HttpResponse
            {
                Result = result,
                StatusCode = statusCode,
                Error = error
            };
        }
    }

    /// <summary>
    /// HTTP 响应封装（带泛型数据）
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class HttpResponse<T> : HttpResponse
    {
        /// <summary>
        /// 解析后的数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 从基础响应创建
        /// </summary>
        public static HttpResponse<T> FromResponse(HttpResponse response, T data = default)
        {
            return new HttpResponse<T>
            {
                Result = response.Result,
                StatusCode = response.StatusCode,
                Error = response.Error,
                RawText = response.RawText,
                RawBytes = response.RawBytes,
                Headers = response.Headers,
                Data = data
            };
        }
    }
}
