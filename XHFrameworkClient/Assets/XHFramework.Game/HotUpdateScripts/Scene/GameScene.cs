using Cysharp.Threading.Tasks;
using XHFramework.Game;

namespace XHFramework.Core
{
    public class GameScene:SceneBase
    {
        private TableScene tableScene = null;
        public override void OnInit(int sceneID)
        { 
            tableScene = FW.DataTableManager.GetTable<TbScene>()[sceneID];
        }

        public override async UniTask OpenLoading()
        {
            await FW.AudioManager.PlayMusic(tableScene.BackgroundMusicId);
           // await FW.UIManager.OpenUIForm<LoadingForm>();
        }

        public override void CloseLoading()
        {
         
        }

        public override void OnEnter()
        {
           
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
                
        }

        public override void OnLeave()
        {
      
        }
    }
}