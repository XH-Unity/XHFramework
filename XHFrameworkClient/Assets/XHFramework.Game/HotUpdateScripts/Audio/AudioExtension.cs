
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using XHFramework.Core;

    namespace XHFramework.Game
    {
        public static class AudioExtension
        {
            private static int? s_musicSerialId = null;

            //音乐
            public static async UniTask<int?> PlayMusic(this AudioManager audioManager, int musicId)
            {
                audioManager.StopMusic();
                TableMusic tbMusic = FW.DataTableManager.GetTable<TbMusic>()[musicId];
                PlayAudioParams playAudioParams = PlayAudioParams.Create();
                playAudioParams.Priority = 64;
                playAudioParams.Loop = true;
                playAudioParams.VolumeInAudioGroup = 1f;
                playAudioParams.SpatialBlend = 0f;
                s_musicSerialId = await audioManager.PlayAudio(ResourceConfig.GetMusicAsset(tbMusic.AssetName), "Music",
                    ResourceConfig.MusicAssetPriority, playAudioParams);
                return s_musicSerialId;
            }

            public static void StopMusic(this AudioManager audioManager)
            {
                if (!s_musicSerialId.HasValue)
                {
                    return;
                }

                audioManager.StopAudio(s_musicSerialId.Value);
                s_musicSerialId = null;
            }

            //音效
            public static async UniTask<int?> PlayAudio(this AudioManager audioManager, int audioId)
            {
                TableSound tbAudio = FW.DataTableManager.GetTable<TbSound>()[audioId];
                PlayAudioParams playAudioParams = PlayAudioParams.Create();
                playAudioParams.Priority = tbAudio.Priority;
                playAudioParams.Loop = tbAudio.Loop;
                playAudioParams.VolumeInAudioGroup = tbAudio.Volume;
                playAudioParams.SpatialBlend = tbAudio.SpatialBlend;
                int? seriaId = await audioManager.PlayAudio(ResourceConfig.GetSoundAsset(tbAudio.AssetName), "Audio",
                    ResourceConfig.SoundAssetPriority, playAudioParams);
                return seriaId;
            }

            //UI音效
            public static async UniTask<int?> PlayUIAudio(this AudioManager audioManager, int uiAudioId)
            {
                TableUISound tbUIAudio = FW.DataTableManager.GetTable<TbUISound>()[uiAudioId];
                PlayAudioParams playAudioParams = PlayAudioParams.Create();
                playAudioParams.Priority = tbUIAudio.Priority;
                playAudioParams.Loop = false;
                playAudioParams.VolumeInAudioGroup = tbUIAudio.Volume;
                playAudioParams.SpatialBlend = 0f;
                int? seriaId = await audioManager.PlayAudio(ResourceConfig.GetUISoundAsset(tbUIAudio.AssetName),
                    "UIAudio", ResourceConfig.UISoundAssetPriority, playAudioParams);
                return seriaId;
            }

            //3D音效
            public static async UniTask<int?> PlayAudio(this AudioManager audioManager, int audioId,
                Vector3 worldPosition)
            {
                TableSound tbAudio = FW.DataTableManager.GetTable<TbSound>()[audioId];
                PlayAudioParams playAudioParams = PlayAudioParams.Create();
                playAudioParams.Priority = tbAudio.Priority;
                playAudioParams.Loop = tbAudio.Loop;
                playAudioParams.VolumeInAudioGroup = tbAudio.Volume;
                playAudioParams.SpatialBlend = tbAudio.SpatialBlend;
                playAudioParams.WorldPosition = worldPosition;
                int? seriaId = await audioManager.PlayAudio(ResourceConfig.GetSoundAsset(tbAudio.AssetName), "Audio",
                    ResourceConfig.SoundAssetPriority, playAudioParams);
                return seriaId;
            }

            //3D跟随音效
            public static async UniTask<int?> PlayAudio(this AudioManager audioManager, int audioId,
                Transform m_BindingEntity)
            {
                TableSound tbAudio = FW.DataTableManager.GetTable<TbSound>()[audioId];
                PlayAudioParams playAudioParams = PlayAudioParams.Create();
                playAudioParams.Priority = tbAudio.Priority;
                playAudioParams.Loop = tbAudio.Loop;
                playAudioParams.VolumeInAudioGroup = tbAudio.Volume;
                playAudioParams.SpatialBlend = tbAudio.SpatialBlend;
                playAudioParams.BindingEntity = m_BindingEntity;
                int? seriaId = await audioManager.PlayAudio(ResourceConfig.GetSoundAsset(tbAudio.AssetName), "Audio",
                    ResourceConfig.SoundAssetPriority, playAudioParams);
                return seriaId;
            }

            //组静音
            public static bool IsMuted(this AudioManager audioManager, string audioGroupName)
            {
                return audioManager.GetAudioGroup(audioGroupName).Mute;
            }

            public static void Mute(this AudioManager audioManager, string audioGroupName, bool mute)
            {
                audioManager.GetAudioGroup(audioGroupName).Mute = mute;
            }

            //组音量
            public static float GetVolume(this AudioManager audioManager, string audioGroupName)
            {
                return audioManager.GetAudioGroup(audioGroupName).Volume;
            }

            public static void SetVolume(this AudioManager audioManager, string audioGroupName, float volume)
            {
                audioManager.GetAudioGroup(audioGroupName).Volume = volume;
            }
        }
    }
