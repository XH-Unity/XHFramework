using System.Collections.Generic;
using XHFramework.Core;

namespace XHFramework.Game
{
    /// <summary>
    /// 音频配置（热更层）
    /// AudioGroupConfig 类定义在 AOT 层
    /// </summary>
    public static class AudioConfig
    {
        /// <summary>
        /// 音频组配置列表
        /// </summary>
        public static readonly List<AudioGroupConfig> AudioGroupConfigs = new List<AudioGroupConfig>()
        {
            new AudioGroupConfig("Music", false, false, 1f, 2),
            new AudioGroupConfig("Sound", false, false, 1f, 4),
            new AudioGroupConfig("UISound", false, false, 1f, 4),
        };

        /// <summary>
        /// 初始化音频系统（热更层入口调用）
        /// </summary>
        public static void InitAudio()
        {
            FW.AudioManager.InitAudioGroups(AudioGroupConfigs);
            Log.Info("AudioConfig 初始化完成");
        }
    }
}