using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace XHFramework.Core {

public class SceneManager : ManagerBase
    {
        public override int Priority => 30;

        #region 字段与属性
        /// <summary>
        /// 已加载场景
        /// </summary>
        private readonly List<string> _loadedSceneAssetNames = new();

        /// <summary>
        /// 加载中场景
        /// </summary>
        private readonly List<string> _loadingSceneAssetNames = new();

        /// <summary>
        /// 卸载中场景
        /// </summary>
        private readonly List<string> _unloadingSceneAssetNames = new();

        private SceneBase _currentScene;
        #endregion

        public override void Init()
        {
           
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _currentScene?.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理场景管理器
        /// </summary>
        public override void Shutdown()
        {
            if (_currentScene != null)
            {
                _currentScene.OnLeave();
                ReferencePool.Release(_currentScene);
                _currentScene = null;
            }

            // 清理状态列表
            _loadedSceneAssetNames.Clear();
            _loadingSceneAssetNames.Clear();
            _unloadingSceneAssetNames.Clear();
        }

        #region 场景检查
        /// <summary>
        /// 获取场景是否正在加载
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称</param>
        /// <returns>场景是否正在加载</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Debug.LogError("场景名称为空");
            }

            return _loadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取场景是否已加载
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称</param>
        /// <returns>场景是否已加载</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Debug.LogError("场景名称为空");
            }

            return _loadedSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取场景是否正在卸载
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称</param>
        /// <returns>场景是否正在卸载</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Debug.LogError("场景名称为空");
            }

            return _unloadingSceneAssetNames.Contains(sceneAssetName);
        }
        #endregion

        #region 场景名称获取
        /// <summary>
        /// 获取已加载场景的资源名称
        /// </summary>
        /// <returns>已加载场景的资源名称</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return _loadedSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称
        /// </summary>
        /// <returns>正在加载场景的资源名称</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return _loadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称
        /// </summary>
        /// <returns>正在卸载场景的资源名称</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return _unloadingSceneAssetNames.ToArray();
        }
        #endregion

        #region 场景加载
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称</param>
        /// <param name="userData">用户自定义数据</param>
        public async UniTask LoadScene<T>(int sceneID,string sceneAssetName,uint priority=100,object userData = null) where T:SceneBase  ,new()
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Debug.LogError("场景名称为空，无法加载场景");
                return;
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                Debug.LogError("场景正在卸载，无法加载场景");
                return;
            }

            if (SceneIsLoading(sceneAssetName))
            {
                Debug.LogError("场景正在加载，无法加载场景");
                return;
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                Debug.LogError("场景已加载，无法加载场景");
                return;
            }

            //关闭当前场景逻辑
            if (_currentScene!=null)
            {
                _currentScene?.OnLeave();
                ReferencePool.Release(_currentScene);
            }
            // 停止所有声音
            FW.AudioManager.StopAllLoadingAudios();
            FW.AudioManager.StopAllLoadedAudios();
            // 隐藏所有实体
            FW.EntityManager.HideAllLoadingEntities();
            FW.EntityManager.HideAllLoadedEntities();
            //关闭所有UI
            FW.UIManager.CloseAllLoadingUIForms();
            FW.UIManager.CloseAllLoadedUIForms();
            //卸载场景
            foreach (string loadedSceneAssetName in _loadedSceneAssetNames)
            {
               await UnloadScene(loadedSceneAssetName);
            }
            
            //清理资源
            await FW.ResourceManager.UnloadSceneAssetsAsync();
            // 还原游戏速度
            FW.SettingManager.ResetNormalGameSpeed();
            //创建新场景逻辑
            _currentScene = ReferencePool.Acquire<T>();
            _currentScene.OnInit(sceneID);
            //加载场景UI
            FW.EventManager.Fire(this, SceneLoadProgressEventArgs.Create(sceneAssetName, 0, userData));
            await _currentScene.OpenLoading();
            FW.EventManager.Fire(this, SceneLoadProgressEventArgs.Create(sceneAssetName, 0.3f, userData));
            //加载场景
            _loadingSceneAssetNames.Add(sceneAssetName);
            bool success = await FW.ResourceManager.LoadSceneAsync(sceneAssetName, UnityEngine.SceneManagement.LoadSceneMode.Additive, priority,(progress) =>
            {
                // 抛出场景加载进度事件
                FW.EventManager.Fire(this, SceneLoadProgressEventArgs.Create(sceneAssetName, 0.3f + progress * 0.3f, userData));
            });
            if (success)
            {
                _loadingSceneAssetNames.Remove(sceneAssetName);
                _loadedSceneAssetNames.Add(sceneAssetName);
                //预加载
                await _currentScene.OnPreload();
                FW.EventManager.Fire(this, SceneLoadProgressEventArgs.Create(sceneAssetName, 0.9f, userData));
                //进入场景逻辑
                _currentScene.OnEnter();
                FW.EventManager.Fire(this, SceneLoadProgressEventArgs.Create(sceneAssetName, 1f, userData));
                _currentScene.CloseLoading();
            }
            else
            {
                ReferencePool.Release(_currentScene);
                _loadingSceneAssetNames.Remove(sceneAssetName);
            }
        }
        #endregion

        #region 场景卸载

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public async UniTask UnloadScene(string sceneAssetName, object userData = null)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Debug.LogError("场景名称为空，无法卸载场景");
                return;
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                Debug.LogError("场景正在卸载，无法卸载场景");
                return;
            }

            if (SceneIsLoading(sceneAssetName))
            {
                Debug.LogError("场景正在加载，无法卸载场景");
                return;
            }

            if (!SceneIsLoaded(sceneAssetName))
            {
                Debug.LogError("场景未加载，无法卸载场景");
                return;
            }

            _unloadingSceneAssetNames.Add(sceneAssetName);
            bool success= await FW.ResourceManager.UnloadSceneAsync(sceneAssetName);
            if (success)
            {
                _unloadingSceneAssetNames.Remove(sceneAssetName);
                _loadedSceneAssetNames.Remove(sceneAssetName);
            }
            else
            {
                _unloadingSceneAssetNames.Remove(sceneAssetName);
            }
        }
        #endregion

        /// <summary>
        /// 获取场景名称
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称</param>
        /// <returns>场景名称</returns>
        public static string GetSceneName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Debug.LogError("场景资源名称为空，无法获取场景名称");
                return null;
            }

            //检查场景资源名称是否合法（即是否是Assets下的完整路径）
            int sceneNamePosition = sceneAssetName.LastIndexOf('/');
            if (sceneNamePosition + 1 >= sceneAssetName.Length)
            {
                Debug.LogError("场景资源名称不合法：" + sceneAssetName);
                return null;
            }

            //分割出场景名
            string sceneName = sceneAssetName.Substring(sceneNamePosition + 1);
            sceneNamePosition = sceneName.LastIndexOf(".unity");
            if (sceneNamePosition > 0)
            {
                sceneName = sceneName.Substring(0, sceneNamePosition);
            }

            return sceneName;
        }

}

}


