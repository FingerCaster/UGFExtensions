using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.ObjectPool;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UGFExtensions.Await;
using UGFExtensions.Timer;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions.SpriteCollection
{
    public partial class SpriteCollectionComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 散图集合对象池
        /// </summary>
        private IObjectPool<SpriteCollectionItemObject> m_SpriteCollectionPool;

        /// <summary>
        /// 自动释放时间间隔
        /// </summary>
        [SerializeField] private int m_AutoReleaseInterval = 60;

#if ODIN_INSPECTOR
        [ReadOnly] [ShowInInspector]
#endif
        private LinkedList<LoadSpriteObject> m_LoadSpriteObjectsLinkedList;
        
        private HashSet<string> m_SpriteCollectionBeingLoaded;
        private Dictionary<string, LinkedList<ISetSpriteObject>> m_WaitSetObjects;
#if UNITY_EDITOR
        public LinkedList<LoadSpriteObject> LoadSpriteObjectsLinkedList
        {
            get => m_LoadSpriteObjectsLinkedList;
            set => m_LoadSpriteObjectsLinkedList = value;
        }
#endif
        private void Start()
        {
            ObjectPoolComponent objectPoolComponent =
                UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            m_SpriteCollectionPool = objectPoolComponent.CreateMultiSpawnObjectPool<SpriteCollectionItemObject>(
                "SpriteCollection",
                60.0f, 16, 60, 0);
            TimerComponent timerComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
            timerComponent.AddRepeatedTimer(m_AutoReleaseInterval * 1000, -1, ReleaseUnused);
            m_LoadSpriteObjectsLinkedList = new LinkedList<LoadSpriteObject>();
            m_SpriteCollectionBeingLoaded = new HashSet<string>();
            m_WaitSetObjects = new Dictionary<string, LinkedList<ISetSpriteObject>>();
        }

        /// <summary>
        /// 回收无引用的 Image 对应图集。
        /// </summary>
#if ODIN_INSPECTOR
        [Button("Release Unused")]
#endif
        public void ReleaseUnused()
        {
            LinkedListNode<LoadSpriteObject> current = m_LoadSpriteObjectsLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.SpriteObject.IsCanRelease())
                {
                    m_SpriteCollectionPool.Unspawn(current.Value.Collection);
                    ReferencePool.Release(current.Value.SpriteObject);
                    m_LoadSpriteObjectsLinkedList.Remove(current);
                }

                current = next;
            }
        }

        /// <summary>
        /// 设置精灵
        /// </summary>
        /// <param name="setSpriteObject">需要设置精灵的对象</param>
        public async void SetSprite(ISetSpriteObject setSpriteObject)
        {
            if (m_SpriteCollectionPool.CanSpawn(setSpriteObject.CollectionPath))
            {
                SpriteCollection collectionItem =
                    (SpriteCollection)m_SpriteCollectionPool.Spawn(setSpriteObject.CollectionPath).Target;
                setSpriteObject.SetSprite(collectionItem.GetSprite(setSpriteObject.SpritePath));
                m_LoadSpriteObjectsLinkedList.AddLast(new LoadSpriteObject(setSpriteObject, collectionItem));
                return;
            }

            if (m_WaitSetObjects.ContainsKey(setSpriteObject.CollectionPath))
            {
                var loadSp = m_WaitSetObjects[setSpriteObject.CollectionPath];
                loadSp.AddLast(setSpriteObject);
            }
            else
            {
                var loadSp = new LinkedList<ISetSpriteObject>();
                loadSp.AddFirst(setSpriteObject);
                m_WaitSetObjects.Add(setSpriteObject.CollectionPath, loadSp);
            }

            if (m_SpriteCollectionBeingLoaded.Contains(setSpriteObject.CollectionPath))
            {
                return;
            }

            m_SpriteCollectionBeingLoaded.Add(setSpriteObject.CollectionPath);
            SpriteCollection collection =
                await GameEntry.Resource.LoadAssetAsync<SpriteCollection>(setSpriteObject.CollectionPath);
            m_SpriteCollectionPool.Register(
                SpriteCollectionItemObject.Create(setSpriteObject.CollectionPath, collection), false);
            m_SpriteCollectionBeingLoaded.Remove(setSpriteObject.CollectionPath);
            m_WaitSetObjects.TryGetValue(setSpriteObject.CollectionPath,
                out LinkedList<ISetSpriteObject> awaitSetImages);
            LinkedListNode<ISetSpriteObject> current = awaitSetImages?.First;
            while (current != null)
            {
                m_SpriteCollectionPool.Spawn(setSpriteObject.CollectionPath);
                current.Value.SetSprite(collection.GetSprite(current.Value.SpritePath));
                m_LoadSpriteObjectsLinkedList.AddLast(new LoadSpriteObject(current.Value, collection));
                current = current.Next;
            }
        }
    }
}