using UnityEngine;
using XHFramework.Core;

namespace XHFramework.Boot
{
    public class Boot : MonoBehaviour
    {
        void Awake()
        {
            Debug.unityLogger.logEnabled = false;
        }

        async void Start()
        {
            try
            {
                Log.Info("开始启动游戏");
                Log.Info("框架初始化");
                FW.Init(gameObject);
                Log.Info("热更初始化");
                YooAssetService yooAssetService = new YooAssetService();
                HybridClrService hybridClrService = new HybridClrService();
                Log.Info("加载更新界面");
                GameObject patchWindowGameObject = Instantiate(Resources.Load<GameObject>("PatchWindow"));
                PatchWindow patchWindow = patchWindowGameObject.AddComponent<PatchWindow>();
                // 连接UI事件，用于显示进度和错误信息
                yooAssetService.OnStepChange += patchWindow.OnStepChange;
                yooAssetService.OnFoundUpdateFiles += patchWindow.OnFoundUpdateFiles;
                yooAssetService.OnDownloadProgress += patchWindow.OnDownloadProgress;
                yooAssetService.OnError += patchWindow.OnError;
                hybridClrService.OnStepChange += patchWindow.OnStepChange;
                hybridClrService.OnError += patchWindow.OnError;
                Log.Info("开始资源更新流程");
                await yooAssetService.InitializeAndUpdate();
                Log.Info("开始代码更新流程");
                await hybridClrService.StartHybridCLRUpdate();
                Log.Info("进入主入口");
                await hybridClrService.EnterMainEntry();
                Destroy(patchWindowGameObject);
                Log.Info("游戏启动流程完成");
            }
            catch (System.Exception e)
            {
                Log.Error($"启动流程异常: {e.Message}");
            }
        }

        public void Update()
        {
            FW.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            FW.Shutdown();
        }

        public static void GameQuit(GameQuitType gameQuitType)
        {
            switch (gameQuitType)
            {
                case GameQuitType.Restart:
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Boot");
                    break;
                case GameQuitType.Quit:
                    Application.Quit();
                    break;
            }
        }
    }
}