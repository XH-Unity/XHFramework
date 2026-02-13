
    using Cysharp.Threading.Tasks;
    using XHFramework.Core;

    namespace XHFramework.Game
    {
        public static class UIExtension
        {
            public static async UniTask<int?> OpenUIForm<T>(this UIManager UIManager, object userData = null)
                where T : UIForm, new()
            {
                UIFormConfig uiFormConfig = UIConfig.GetConfig<T>();
                return await UIManager.OpenUIForm<T>(ResourceConfig.GetUIFormAsset(uiFormConfig.AssetName), uiFormConfig.UIGroupName,
                    uiFormConfig.PauseCoveredUIForm, uiFormConfig.AllowMultiInstance,ResourceConfig.UIFormAssetPriority, userData);
            }

            public static UIForm[] GetUIForms<T>(this UIManager UIManager) where T : UIForm
            {
                UIFormConfig uiFormConfig = UIConfig.GetConfig<T>();
                return UIManager.GetUIForms<T>(uiFormConfig.AssetName);
            }

            public static UIForm GetUIForm<T>(this UIManager UIManager) where T : UIForm
            {
                UIFormConfig uiFormConfig = UIConfig.GetConfig<T>();
                return UIManager.GetUIForm<T>(uiFormConfig.AssetName);
            }

            //关闭单开界面
            public static void CloseUIForm<T>(this UIManager UIManager, object userData = null) where T : UIForm
            {
                UIFormConfig uiFormConfig = UIConfig.GetConfig<T>();
                if (uiFormConfig.AllowMultiInstance)
                {
                    Log.Error("可以多开界面，请使用serialId关闭");
                    return;
                }

                UIManager.CloseUIForm<T>(uiFormConfig.UIGroupName, userData);
            }

            //关闭多开界面（通过group，通过SerialId的可以用CloseUIForm(int serialId, object userData = null)）
            public static void CloseUIForm<T>(this UIManager UIManager, string uiGroupName, object userData = null)
                where T : UIForm
            {
                UIFormConfig uiFormConfig = UIConfig.GetConfig<T>();
                if (uiFormConfig.AllowMultiInstance)
                {
                    Log.Error("可以多开界面，请使用serialId关闭");
                    return;
                }

                UIGroup uiGroup = UIManager.GetUIGroup(uiGroupName);
                if (uiGroup == null)
                {
                    Log.Error("没有找到界面组");
                    return;
                }

                UIForm uiForm = (UIForm)uiGroup.GetUIForm(uiFormConfig.AssetName);
                if (uiForm == null)
                {
                    Log.Error("没有找到界面");
                    return;
                }

                UIManager.CloseUIForm(uiForm.SerialId, userData);
            }
        }
    }