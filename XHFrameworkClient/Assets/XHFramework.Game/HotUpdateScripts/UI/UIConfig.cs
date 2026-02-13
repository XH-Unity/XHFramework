using System;
using System.Collections.Generic;
using XHFramework.Core;

/// <summary>
/// UI 配置（热更层）
/// UIFormConfig 和 UIGroupConfig 类定义在 AOT 层
/// </summary>
namespace XHFramework.Game
{
    public static class UIConfig
    {
        /// <summary>
        /// UI 窗口配置字典
        /// </summary>
        private static readonly Dictionary<Type, UIFormConfig> s_uiFormConfigs = new Dictionary<Type, UIFormConfig>()
        {
            // LoadingForm: 加载界面，Background组，不允许多实例，不暂停被覆盖界面
            { typeof(LoadingForm), new UIFormConfig("LoadingForm", "Background", false, false) },
        };

        /// <summary>
        /// UI 组配置列表
        /// </summary>
        public static readonly List<UIGroupConfig> UIGroupConfigs = new List<UIGroupConfig>()
        {
            // 示例：
             new UIGroupConfig("Background", 0),
             new UIGroupConfig("Normal", 100),
            // new UIGroupConfig("Fixed", 200),
            // new UIGroupConfig("Popup", 300),
            // new UIGroupConfig("Tips", 400),
        };

        /// <summary>
        /// 获取 UI 窗口配置
        /// </summary>
        public static UIFormConfig GetConfig<T>() where T : UIForm
        {
            if (s_uiFormConfigs.TryGetValue(typeof(T), out var config))
            {
                return config;
            }

            Log.Error($"UIFormConfig not found for type: {typeof(T).Name}");
            return null;
        }

        /// <summary>
        /// 初始化 UI 系统（热更层入口调用）
        /// </summary>
        public static void InitUI()
        {
            // 初始化 UI 组
            FW.UIManager.InitUIGroups(UIGroupConfigs);
            Log.Info("UIConfig 初始化完成");
        }
    }
}