using System;
using System.Collections;
using System.IO;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;
using XHFramework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XHFramework.Boot
{
    public class YooAssetService
    {
        private readonly string _packageName ;
        private readonly string _packageURL ;
        // 事件
        public event Action<string> OnStepChange;
        public event Action<int, long> OnFoundUpdateFiles;
        public event Action<int, int, long, long> OnDownloadProgress;
        public event Action<string> OnError;

        private ResourcePackage _package;
        private ResourceDownloaderOperation _downloader;
        private string _packageVersion;

        public YooAssetService ()
        {
            _packageName = BootConfig.packageName;
            _packageURL = BootConfig.packageUrl;
        }

        /// <summary>
        /// 运行时获取播放模式
        /// </summary>
        private EPlayMode GetPlayMode()
        {
#if UNITY_EDITOR
            return EPlayMode.EditorSimulateMode; // 编辑器下使用配置的模式
#else
    #if RESOURCE_OFFLINE
            return EPlayMode.OfflinePlayMode;
    #else
            return EPlayMode.HostPlayMode;
    #endif
#endif
        }

        /// <summary>
        /// 初始化并开始更新流程
        /// </summary>
        public async UniTask<bool> InitializeAndUpdate()
        {
            try
            {
                Log.Info("开始YooAsset初始化和更新流程");
                OnStepChange?.Invoke("初始化YooAsset...");
                YooAssets.Initialize();
                Log.Info("YooAsset系统初始化完成");
                // 初始化资源包
                if (!await InitializePackage())
                    return false;

                var playMode = GetPlayMode();

                // 离线模式和编辑器模拟模式不需要网络更新，直接完成
                if (playMode == EPlayMode.OfflinePlayMode || playMode == EPlayMode.EditorSimulateMode)
                {
                    Log.Info($"{playMode} 模式，跳过网络更新流程");
                    OnStepChange?.Invoke("YooAsset更新完成");
                    FinishUpdate();
                    Log.Info("YooAsset初始化和更新流程全部完成");
                    return true;
                }

                // 联机模式：请求版本、更新清单、下载资源
                if (!await RequestPackageVersion())
                    return false;
                if (!await UpdatePackageManifest())
                    return false;
                if (!await CheckAndDownloadFiles())
                    return false;
                await ClearCacheFiles();

                OnStepChange?.Invoke("YooAsset更新完成");
                FinishUpdate();
                Log.Info("YooAsset初始化和更新流程全部完成");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"YooAsset更新失败: {e.Message}");
                OnError?.Invoke($"YooAsset更新失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 初始化资源包
        /// </summary>
        private async UniTask<bool> InitializePackage()
        {
            OnStepChange?.Invoke("初始化资源包!");
            var currentPlayMode = GetPlayMode();
            Log.Info($"当前运行模式: {currentPlayMode}");
            // 创建资源包裹类
            _package = YooAssets.TryGetPackage(_packageName);
            if (_package == null)
            {
                _package = YooAssets.CreatePackage(_packageName);
                Log.Info($"创建新的资源包: {_packageName}");
            }
            else
            {
                Log.Info($"使用已存在的资源包: {_packageName}");
            }

            InitializationOperation initializationOperation = null;

            // 编辑器下的模拟模式
            if (currentPlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(_packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = _package.InitializeAsync(createParameters);
            }
            // 单机运行模式
            else if (currentPlayMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = _package.InitializeAsync(createParameters);
            }
            // 联机运行模式
            else if (currentPlayMode == EPlayMode.HostPlayMode)
            {
                string platformURL = GetPlatformURL(_packageURL);
                IRemoteServices remoteServices = new RemoteServices(platformURL, platformURL);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = null;
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = _package.InitializeAsync(createParameters);
            }
            // WebGL运行模式
            else if (currentPlayMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                var createParameters = new WebPlayModeParameters();
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";
                string platformURL = GetPlatformURL(packageURL);
                Log.Info($"远程资源地址: {platformURL}");
                IRemoteServices remoteServices = new RemoteServices(platformURL, platformURL);
                createParameters.WebServerFileSystemParameters =
     WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
                initializationOperation = _package.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters();
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = _package.InitializeAsync(createParameters);
#endif
            }

            await initializationOperation.ToUniTask();

            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Log.Error($"初始化资源包失败: {initializationOperation.Error}");
                OnError?.Invoke("初始化资源包失败!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 请求资源版本
        /// </summary>
        private async UniTask<bool> RequestPackageVersion()
        {
            OnStepChange?.Invoke("请求资源版本!");

            var operation = _package.RequestPackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Log.Error($"请求资源版本失败: {operation.Error}");
                OnError?.Invoke("请求资源版本失败!");
                return false;
            }

            Log.Info($"Request package version: {operation.PackageVersion}");
            _packageVersion = operation.PackageVersion;
            return true;
        }

        /// <summary>
        /// 更新资源清单
        /// </summary>
        private async UniTask<bool> UpdatePackageManifest()
        {
            OnStepChange?.Invoke("更新资源清单!");

            var operation = _package.UpdatePackageManifestAsync(_packageVersion);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Log.Error($"更新资源清单失败: {operation.Error}");
                OnError?.Invoke("更新资源清单失败!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查并下载更新文件
        /// </summary>
        private async UniTask<bool> CheckAndDownloadFiles()
        {
            OnStepChange?.Invoke("创建资源下载器!");

            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            _downloader = _package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            if (_downloader.TotalDownloadCount == 0)
            {
                Log.Info("No files to download!");
                return true;
            }

            // 发现更新文件，直接开始下载（不等待用户点击）
            int totalDownloadCount = _downloader.TotalDownloadCount;
            long totalDownloadBytes = _downloader.TotalDownloadBytes;
            Log.Info($"发现需要下载的文件: {totalDownloadCount}个, 总大小: {totalDownloadBytes}字节");

            OnFoundUpdateFiles?.Invoke(totalDownloadCount, totalDownloadBytes);

            return await DownloadFiles();
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        private async UniTask<bool> DownloadFiles()
        {
            OnStepChange?.Invoke("开始下载资源文件!");

            // 设置下载回调
            _downloader.DownloadUpdateCallback = (updateData) =>
            {
                OnDownloadProgress?.Invoke(updateData.CurrentDownloadCount, updateData.TotalDownloadCount,
                    updateData.CurrentDownloadBytes, updateData.TotalDownloadBytes);
            };

            _downloader.DownloadErrorCallback = (errorData) =>
            {
                Log.Error($"下载文件失败: {errorData.FileName}, {errorData.ErrorInfo}");
                OnError?.Invoke($"下载文件失败: {errorData.FileName}");
            };

            _downloader.BeginDownload();
            await _downloader.ToUniTask();

            if (_downloader.Status != EOperationStatus.Succeed)
            {
                OnError?.Invoke("资源下载失败!");
                return false;
            }

            OnStepChange?.Invoke("资源文件下载完毕!");
            return true;
        }

        /// <summary>
        /// 清理缓存文件
        /// </summary>
        private async UniTask ClearCacheFiles()
        {
            OnStepChange?.Invoke("清理未使用的缓存文件!");

            var operation = _package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            await operation.ToUniTask();
        }

        /// <summary>
        /// 根据当前平台获取完整的资源URL
        /// </summary>
        private string GetPlatformURL(string baseURL)
        {
            string platformName = GetPlatformName();
            // 确保baseURL末尾没有斜杠
            baseURL = baseURL.TrimEnd('/');
            return $"{baseURL}/{platformName}";
        }

        /// <summary>
        /// 获取当前平台名称
        /// </summary>
        private string GetPlatformName()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_WEBGL
            return "WebGL";
#elif UNITY_STANDALONE_WIN
            return "Windows";
#elif UNITY_STANDALONE_OSX
            return "MacOS";
#elif UNITY_STANDALONE_LINUX
            return "Linux";
#else
            return "Default";
#endif
        }

        /// <summary>
        /// 完成YooAsset更新
        /// </summary>
        public void FinishUpdate()
        {
            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(_packageName);
            YooAssets.SetDefaultPackage(gamePackage);
            Log.Info("YooAsset更新完成");
            Clear();
        }

        public void Clear()
        {
            OnStepChange = null;
            OnFoundUpdateFiles = null;
            OnDownloadProgress = null;
            OnError = null;
            _package = null;
            _downloader = null;
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
    }
}