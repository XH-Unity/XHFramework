using  XHFramework.Core;
namespace XHFramework.Game
{
    public static class HttpExtension
    {
        #region API 路径定义

        /// <summary>
        /// API 路径常量（按模块分组）
        /// </summary>
        public static class Api
        {
            // ========== 用户相关 ==========
            public const string Login = "/api/user/login";
            public const string Logout = "/api/user/logout";
            public const string UserInfo = "/api/user/info";
            public const string Register = "/api/user/register";

            // ========== 游戏相关 ==========
            public const string GameConfig = "/api/game/config";
            public const string GameNotice = "/api/game/notice";
            public const string ServerList = "/api/game/servers";

            // ========== 商城相关 ==========
            public const string ShopList = "/api/shop/list";
            public const string ShopBuy = "/api/shop/buy";

            // ========== 排行榜相关 ==========
            public const string RankList = "/api/rank/list";
            public const string RankMy = "/api/rank/my";
        }

        #endregion
        /// <summary>
        /// 登录请求
        /// </summary>
        public static async Cysharp.Threading.Tasks.UniTask<HttpResponse<T>> LoginAsync<T>(this HttpManager HttpManager, string username, string password)
        {
            return await HttpManager.PostAsync<T>(Api.Login, new
            {
                username = username,
                password = password
            });
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public static async Cysharp.Threading.Tasks.UniTask<HttpResponse<T>> GetUserInfoAsync<T>(this HttpManager HttpManager)
        {
            return await HttpManager.GetAsync<T>(Api.UserInfo);
        }

        /// <summary>
        /// 获取服务器列表
        /// </summary>
        public static async Cysharp.Threading.Tasks.UniTask<HttpResponse<T>> GetServerListAsync<T>(this HttpManager HttpManager)
        {
            return await HttpManager.GetAsync<T>(Api.ServerList);
        }

        /// <summary>
        /// 获取游戏公告
        /// </summary>
        public static async Cysharp.Threading.Tasks.UniTask<HttpResponse<T>> GetNoticeAsync<T>(this HttpManager HttpManager)
        {
            return await HttpManager.GetAsync<T>(Api.GameNotice);
        }

    }
}