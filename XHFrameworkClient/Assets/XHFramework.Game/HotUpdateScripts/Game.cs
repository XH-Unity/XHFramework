using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;
using TMPro;
using XHFramework.Core;

namespace XHFramework.Game
{

    public class Game
    {
        /// <summary>
        /// 热更层入口（AOT 层调用）
        /// </summary>
        public static async UniTask Start()
        {
            //预加载配置表
            await PreloadDataTable();
            // 初始化热更层配置
            InitHotUpdateConfigs();
            //预加载语言
            PreloadLoadLocalization();
            //切换场景
            await FW.SceneManager.LoadScene<GameScene>(1);
            
            TableMap tableMap=  FW.DataTableManager.GetTable<TbMap>()[1001];
            GameObject map = await FW.ResourceManager.LoadGameObjectAsync(ResourceConfig.GetMapAsset(tableMap.AssetName));
            await FW.PathFindingManager.LoadMapAsync(ResourceConfig.GetMapJsonAsset(tableMap.JasonAssetName));
             
             TableRole tableRole=  FW.DataTableManager.GetTable<TbRole>()[1001];
             GameObject role = await FW.ResourceManager.LoadGameObjectAsync(ResourceConfig.GetEntityAsset(tableRole.AssetName));
        }


        /// 初始化热更层配置
        private static void InitHotUpdateConfigs()
        {
            // 初始化 UI 配置（UIGroup 配置）
            UIConfig.InitUI();
            // 初始化音频配置（AudioGroup 配置）
            AudioConfig.InitAudio();
            // 初始化实体配置（EntityGroup 配置）
            EntityConfig.InitEntity();
            // 初始化网络配置（TCP/UDP/WebSocket 通道配置）
            NetworkConfig.InitNetwork();
            // 初始化 HTTP 配置
            HttpConfig.InitHttp();
        }

        public static async UniTask PreloadDataTable()
        {
            // 加载所有配置表字节数据
            await FW.DataTableManager.LoadAllDataTablesByTagAsync(ResourceConfig.DataTableAssetPriority);

            // 解析所有配置表
            FW.DataTableManager.LoadTable<TbEntity>("tbentity");
            FW.DataTableManager.LoadTable<TbLocalizationText>("tblocalizationtext");
            FW.DataTableManager.LoadTable<TbMusic>("tbmusic");
            FW.DataTableManager.LoadTable<TbScene>("tbscene");
            FW.DataTableManager.LoadTable<TbSound>("tbsound");
            FW.DataTableManager.LoadTable<TbUISound>("tbuisound");
            FW.DataTableManager.LoadTable<TbMap>("tbmap");
            FW.DataTableManager.LoadTable<TbRole>("tbrole");
        }
        
        public static void PreloadLoadLocalization()
        {
            FW.LocalizationManager.SetLanguageTypes(LanguageConfig.LanguageTypes);
            FW.LocalizationManager.SetLanguageData((id, language) =>
            {
                TableLocalizationText tableLocalizationText=  FW.DataTableManager.GetTable<TbLocalizationText>()[id];
                if (tableLocalizationText==null) return null;
                return language switch
                {
                    "CN" => tableLocalizationText.CN,
                    "EN" => tableLocalizationText.EN,
                    _ => tableLocalizationText.CN
                };
            });
            FW.LocalizationManager.ResetLanguage();
        }
        
        // public static async UniTask PreloadFont()
        // {
        //     await FW.ResourceManager.PreloadGlobalResidentAssetAsync<TMP_FontAsset>("");
        // }
        
    }
}