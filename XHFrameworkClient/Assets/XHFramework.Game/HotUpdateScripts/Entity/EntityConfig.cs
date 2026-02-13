using System.Collections.Generic;
using XHFramework.Core;

namespace XHFramework.Game
{
    /// <summary>
    /// 实体配置（热更层）
    /// EntityGroupConfig 类定义在 AOT 层
    /// </summary>
    public static class EntityConfig
    {
        /// <summary>
        /// 实体组配置列表
        /// </summary>
        public static readonly List<EntityGroupConfig> EntityGroupConfigs = new List<EntityGroupConfig>()
        {
            new EntityGroupConfig("Aircraft", 60f, 16, 60f),
            // 添加更多实体组配置...
        };

        /// <summary>
        /// 初始化实体系统（热更层入口调用）
        /// </summary>
        public static void InitEntity()
        {
            FW.EntityManager.InitEntityGroups(EntityGroupConfigs);
            Log.Info("EntityConfig 初始化完成");
        }
    }
}