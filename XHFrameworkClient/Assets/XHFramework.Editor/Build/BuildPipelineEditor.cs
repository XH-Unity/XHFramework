using XHFramework.Core;

namespace XHFramework.Editor
{
    public partial class BuildPipelineEditor
    {
        private const string OfflineModeSymbol = "RESOURCE_OFFLINE"; // offline
        private const string AssetbundleModeSymbol = "RESOURCE_ASSETBUNDLE"; // ab热更
        private const string EnableLOGSymbol = "EnableLog"; //日志

        #region Android构建

        // 菜单：构建内部测试全量包
        //[MenuItem("YooAsset/构建/全量包(离线)")]
        public static void BuildOfflineAPK()
        {
            Log.Info("开始构建Android内部测试全量包...");
            Log.Info("步骤1/7: 设置离线宏");
            SetEnableLogSymbol(OfflineModeSymbol, true); // 设置编译符号为离线模式
            SetEnableLogSymbol(AssetbundleModeSymbol, false); // 设置编译符号为离线模式
            SetEnableLogSymbol(EnableLOGSymbol, BuildToolPanel.EnableLog); // 离线包需要日志功能
            Log.Info("步骤2/7: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog); // 根据日志设置管理Reporter组件
            Log.Info("步骤3/7: 初始化清空资源输出目录");
            BuildInit(true);
            Log.Info("步骤4/7: 构建DLL");
            BuildDLL();
            Log.Info("步骤5/7: 构建资源包");
            BuildAB(true, true); // 直接递增版本
            Log.Info("步骤6/7: 构建APK");
            BuildPlayer("Offline"); // 构建包含所有资源的APK
            Log.Info("步骤7/8: 恢复Editor状态，删除日志Reporter，恢复日志");
            ManageReporter(false);
            SetEnableLogSymbol(EnableLOGSymbol, true);
            Log.Info("========== Android全量包(离线)完成 ==========");
        }

        // 构建支持热更的全量包
        //[MenuItem("YooAsset/构建/全量包APK(热更)")]
        public static void BuildFullPackageAPK()
        {
            Log.Info("开始构建Android全量包APK(热更)...");
            Log.Info("步骤1/8: 设置热更宏");
            SetEnableLogSymbol(OfflineModeSymbol, false); // 设置编译符号为离线模式
            SetEnableLogSymbol(AssetbundleModeSymbol, true); // 设置编译符号为离线模式
            SetEnableLogSymbol(EnableLOGSymbol, BuildToolPanel.EnableLog); // 离线包需要日志功能
            Log.Info("步骤2/8: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog); // 根据日志设置管理Reporter组件
            Log.Info("步骤3/8: 初始化清空资源输出目录");
            BuildInit(true);
            Log.Info("步骤4/8: 构建DLL");
            BuildDLL();
            Log.Info("步骤5/8: 构建资源包");
            BuildAB(true, true); // 直接递增版本
            Log.Info("步骤6/8: 服务器资源同步");
            BuildSeverSync();
            Log.Info("步骤7/8: 构建APK");
            BuildPlayer("Release"); // 构建包含所有资源的APK
            Log.Info("步骤7/8: 恢复Editor状态，删除日志Reporter，恢复日志");
            ManageReporter(false);
            SetEnableLogSymbol(EnableLOGSymbol, true);
            Log.Info("========== Android全量包APK(热更)完成 ==========");
        }

        // 构建支持热更的空包
        //[MenuItem("YooAsset/构建/空包APK(热更)")]
        public static void BuildNulllPackageAPK()
        {
            Log.Info("开始构建Android空包APK(热更)...");
            Log.Info("步骤1/7: 设置热更宏");
            SetEnableLogSymbol(OfflineModeSymbol, false); // 设置编译符号为离线模式
            SetEnableLogSymbol(AssetbundleModeSymbol, true); // 设置编译符号为离线模式
            SetEnableLogSymbol(EnableLOGSymbol, BuildToolPanel.EnableLog); // 离线包需要日志功能
            Log.Info("步骤2/7: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog); // 根据日志设置管理Reporter组件
            Log.Info("步骤3/7: 初始化清空资源输出目录");
            BuildInit(true);
            Log.Info("步骤4/7: 构建DLL");
            BuildDLL();
            Log.Info("步骤5/7: 构建资源包");
            BuildAB(true, false); // 直接递增版本
            Log.Info("步骤6/7: 服务器资源同步");
            BuildSeverSync();
            Log.Info("步骤7/7: 构建APK");
            BuildPlayer("Release"); // 构建包含所有资源的APK
            Log.Info("步骤7/8: 恢复Editor状态，删除日志Reporter，恢复日志");
            ManageReporter(false);
            SetEnableLogSymbol(EnableLOGSymbol, true);
            Log.Info("========== Android空包APK(热更)完成 ==========");
        }

        #endregion

        #region iOS构建

        /// <summary>
        /// 构建iOS离线全量包（导出Xcode工程）
        /// </summary>
        public static void BuildOfflineiOS()
        {
            Log.Info("开始构建iOS内部测试全量包...");
            Log.Info("步骤1/7: 设置离线宏");
            SetEnableLogSymbol(OfflineModeSymbol, true);
            SetEnableLogSymbol(AssetbundleModeSymbol, false);
            SetEnableLogSymbol(EnableLOGSymbol, BuildToolPanel.EnableLog);
            Log.Info("步骤2/7: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog);
            Log.Info("步骤3/7: 初始化清空资源输出目录");
            BuildInit(true);
            Log.Info("步骤4/7: 构建DLL");
            BuildDLL();
            Log.Info("步骤5/7: 构建资源包");
            BuildAB(true, true);
            Log.Info("步骤6/7: 导出iOS Xcode工程");
            BuildiOSPlayer("Offline");
            Log.Info("步骤7/7: 恢复Editor状态");
            ManageReporter(false);
            SetEnableLogSymbol(EnableLOGSymbol, true);
            Log.Info("========== iOS全量包(离线)导出完成 ==========");
            Log.Info("📱 请将Xcode工程复制到Mac电脑进行最终构建");
        }

        /// <summary>
        /// 构建iOS热更全量包（导出Xcode工程）
        /// </summary>
        public static void BuildFullPackageiOS()
        {
            Log.Info("开始构建iOS全量包(热更)...");
            Log.Info("步骤1/8: 设置热更宏");
            SetEnableLogSymbol(OfflineModeSymbol, false);
            SetEnableLogSymbol(AssetbundleModeSymbol, true);
            SetEnableLogSymbol(EnableLOGSymbol, BuildToolPanel.EnableLog);
            Log.Info("步骤2/8: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog);
            Log.Info("步骤3/8: 初始化清空资源输出目录");
            BuildInit(true);
            Log.Info("步骤4/8: 构建DLL");
            BuildDLL();
            Log.Info("步骤5/8: 构建资源包");
            BuildAB(true, true);
            Log.Info("步骤6/8: 服务器资源同步");
            BuildSeverSync();
            Log.Info("步骤7/8: 导出iOS Xcode工程");
            BuildiOSPlayer("Release");
            Log.Info("步骤8/8: 恢复Editor状态");
            ManageReporter(false);
            SetEnableLogSymbol(EnableLOGSymbol, true);
            Log.Info("========== iOS全量包(热更)导出完成 ==========");
            Log.Info("📱 请将Xcode工程复制到Mac电脑进行最终构建");
        }

        /// <summary>
        /// 构建iOS热更空包（导出Xcode工程）
        /// </summary>
        public static void BuildNullPackageiOS()
        {
            Log.Info("开始构建iOS空包(热更)...");
            Log.Info("步骤1/7: 设置热更宏");
            SetEnableLogSymbol(OfflineModeSymbol, false);
            SetEnableLogSymbol(AssetbundleModeSymbol, true);
            SetEnableLogSymbol(EnableLOGSymbol, BuildToolPanel.EnableLog);
            Log.Info("步骤2/7: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog);
            Log.Info("步骤3/7: 初始化清空资源输出目录");
            BuildInit(true);
            Log.Info("步骤4/7: 构建DLL");
            BuildDLL();
            Log.Info("步骤5/7: 构建资源包");
            BuildAB(true, false);
            Log.Info("步骤6/7: 服务器资源同步");
            BuildSeverSync();
            Log.Info("步骤7/7: 导出iOS Xcode工程");
            BuildiOSPlayer("Release");
            Log.Info("步骤7/8: 恢复Editor状态");
            ManageReporter(false);
            SetEnableLogSymbol(EnableLOGSymbol, true);
            Log.Info("========== iOS空包(热更)导出完成 ==========");
            Log.Info("📱 请将Xcode工程复制到Mac电脑进行最终构建");
        }

        #endregion

        #region 通用构建

        // 菜单：构建热更新资源包
        //[MenuItem("YooAsset/构建/增量资源包(热更)")]
        public static void BuildIncrementalPackageNoAPK()
        {
            Log.Info("开始构建增量资源包(热更)...");
            Log.Info("步骤1/6: 管理GUI日志Reporter");
            ManageReporter(BuildToolPanel.EnableLog); // 根据日志设置管理Reporter组件
            Log.Info("步骤2/6: 初始化清空资源输出目录");
            BuildInit(false);
            Log.Info("步骤3/6: 构建DLL");
            BuildDLL();
            Log.Info("步骤4/6: 构建资源包");
            BuildAB(false, false); // 直接递增版本
            Log.Info("步骤5/6: 服务器资源同步");
            BuildSeverSync();
            Log.Info("步骤7/8: 恢复Editor状态，删除日志Reporter，恢复日志");
            ManageReporter(false);
            Log.Info("========== 增量资源包(热更)完成 ==========");
        }

        #endregion
    }
}