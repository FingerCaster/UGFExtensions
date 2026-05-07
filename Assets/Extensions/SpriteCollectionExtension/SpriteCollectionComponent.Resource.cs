using System.Collections.Generic;
using GameFramework.Resource;
using UnityGameFramework.Runtime;

namespace UGFExtensions.SpriteCollection
{
    public partial class SpriteCollectionComponent
    {
        /// <summary>
        /// 资源组件
        /// </summary>
        private ResourceComponent m_ResourceComponent;
        private LoadAssetCallbacks m_LoadAssetCallbacks;
        
        private void InitializedResources()
        {
            m_ResourceComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();
            m_LoadAssetCallbacks = new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFailure);
        }
        
        private void OnLoadAssetFailure(string assetName, LoadResourceStatus status, string errormessage, object userdata)
        {
            if (userdata is ISetSpriteObject setSpriteObject)
            {
                m_SpriteCollectionBeingLoaded.Remove(setSpriteObject.CollectionPath);
                ClearWaitSetObjects(setSpriteObject.CollectionPath, true);
            }

            Log.Error("Can not load SpriteCollection from '{0}' with error message '{1}'.",assetName,errormessage);
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            ISetSpriteObject setSpriteObject = (ISetSpriteObject)userdata;
            SpriteCollection collection = (SpriteCollection)asset;
            m_SpriteCollectionPool.Register(SpriteCollectionItemObject.Create(setSpriteObject.CollectionPath, collection,m_ResourceComponent), true);
            ApplyLoadedSpriteCollection(setSpriteObject.CollectionPath, collection);
        }
        
        /// <summary>
        /// 设置精灵
        /// </summary>
        /// <param name="setSpriteObject">需要设置精灵的对象</param>
        public void SetSprite(ISetSpriteObject setSpriteObject)
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
            m_ResourceComponent.LoadAsset(setSpriteObject.CollectionPath,typeof(SpriteCollection),m_LoadAssetCallbacks,setSpriteObject);
        }
    }
}
