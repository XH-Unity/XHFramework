using Cysharp.Threading.Tasks;
using  XHFramework.Game;

namespace XHFramework.Core
{
    public static class SceneExtension
    {
        public static async UniTask LoadScene<T>(this SceneManager SceneManager,int sceneID, object userData = null)
            where T : SceneBase, new()
        {
           TableScene tableScene=  FW.DataTableManager.GetTable<TbScene>()[sceneID];
           await SceneManager.LoadScene<T>(sceneID,ResourceConfig.GetSceneAsset(tableScene.AssetName),ResourceConfig.SceneAssetPriority, userData);
        }

    }
}