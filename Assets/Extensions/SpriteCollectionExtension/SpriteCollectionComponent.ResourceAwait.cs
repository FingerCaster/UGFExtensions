using System.Collections.Generic;
using UGFExtensions.Await;

namespace UGFExtensions.SpriteCollection
{
    public partial class SpriteCollectionComponent
    {
        /// <summary>
        /// 设置精灵
        /// </summary>
        /// <param name="setSpriteObject">需要设置精灵的对象</param>
        public async void SetSpriteAsync(ISetSpriteObject setSpriteObject)
        {
            if (m_SpriteCollectionPool.CanSpawn(setSpriteObject.CollectionPath))
            {
                SpriteCollection collectionItem = (SpriteCollection)m_SpriteCollectionPool.Spawn(setSpriteObject.CollectionPath).Target;
                setSpriteObject.SetSprite(collectionItem.GetSprite(setSpriteObject.SpritePath));
                m_LoadSpriteObjectsLinkedList.AddLast(new LoadSpriteObject(setSpriteObject, collectionItem));
                return;
            }

            AddWaitSetObject(setSpriteObject);

            if (m_SpriteCollectionBeingLoaded.Contains(setSpriteObject.CollectionPath))
            {
                return;
            }

            m_SpriteCollectionBeingLoaded.Add(setSpriteObject.CollectionPath);
            try
            {
                SpriteCollection collection = await m_ResourceComponent.LoadAssetAsync<SpriteCollection>(setSpriteObject.CollectionPath);
                m_SpriteCollectionPool.Register(SpriteCollectionItemObject.Create(setSpriteObject.CollectionPath, collection,m_ResourceComponent), true);
                ApplyLoadedSpriteCollection(setSpriteObject.CollectionPath, collection);
            }
            catch
            {
                m_SpriteCollectionBeingLoaded.Remove(setSpriteObject.CollectionPath);
                ClearWaitSetObjects(setSpriteObject.CollectionPath, true);
                throw;
            }
        }
    }
}
