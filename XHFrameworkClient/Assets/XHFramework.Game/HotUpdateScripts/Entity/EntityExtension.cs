using System;
using Cysharp.Threading.Tasks;
using XHFramework.Core;

namespace XHFramework.Game
{
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId = 0;

        public static T GetGameEntity<T>(this EntityManager EntityManager, int entityId) where T : Entity
        {
            Entity entity = EntityManager.GetEntity(entityId);
            if (entity == null)
            {
                return null;
            }

            return entity as T;
        }

        public static void HideEntity(this EntityManager EntityManager, int entityId, object userData = null)
        {
            EntityManager.HideEntity(entityId, userData);
        }

        public static void AttachEntity(this EntityManager EntityManager, Entity entity, int ownerId,
            string parentTransformPath = null, object userData = null)
        {
            EntityManager.AttachEntity(entity, ownerId, parentTransformPath, userData);
        }

        //单机 id自己GenerateSerialId生成   网络版需要服务器下发
        private static async UniTask<Entity> ShowEntity<T>(this EntityManager EntityManager, string entityGroup, EntityData data) where T : Entity, new()
        {
            TableEntity table = FW.DataTableManager.GetTable<TbEntity>()[data.EntityTableID];
            Entity entity = await EntityManager.ShowEntity<T>(data.Id, ResourceConfig.GetEntityAsset(table.AssetName),
                entityGroup,ResourceConfig.EntityAssetPriority, data);
            return entity;
        }

        public static int GenerateSerialId(this EntityManager EntityManager)
        {
            return --s_SerialId;
        }
    }
}
