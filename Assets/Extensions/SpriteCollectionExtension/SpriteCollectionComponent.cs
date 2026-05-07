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
        /// 检查是否可以释放间隔
        /// </summary>
        [SerializeField] private float m_CheckCanReleaseInterval = 30f;

        private float m_CheckCanReleaseTime = 0.0f;

        /// <summary>
        /// 对象池自动释放时间间隔
        /// </summary>
        [SerializeField] private float m_AutoReleaseInterval = 60f;
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
        private void AddWaitSetObject(ISetSpriteObject setSpriteObject)
        {
            if (m_WaitSetObjects.TryGetValue(setSpriteObject.CollectionPath, out var setSpriteObjects))
            {
                setSpriteObjects.AddLast(setSpriteObject);
                return;
            }

            var loadSpriteObjects = new LinkedList<ISetSpriteObject>();
            loadSpriteObjects.AddLast(setSpriteObject);
            m_WaitSetObjects.Add(setSpriteObject.CollectionPath, loadSpriteObjects);
        }

        private void ClearWaitSetObjects(string collectionPath, bool releaseObjects)
        {
            if (!m_WaitSetObjects.TryGetValue(collectionPath, out var waitSetObjects))
            {
                return;
            }

            if (releaseObjects)
            {
                LinkedListNode<ISetSpriteObject> current = waitSetObjects.First;
                while (current != null)
                {
                    current.Value.SetSprite(null);
                    ReferencePool.Release(current.Value);
                    current = current.Next;
                }
            }

            waitSetObjects.Clear();
            m_WaitSetObjects.Remove(collectionPath);
        }

        private void ApplyLoadedSpriteCollection(string collectionPath, SpriteCollection collection)
        {
            m_SpriteCollectionBeingLoaded.Remove(collectionPath);
            if (!m_WaitSetObjects.TryGetValue(collectionPath, out LinkedList<ISetSpriteObject> awaitSetImages))
            {
                m_SpriteCollectionPool.Unspawn(collection);
                return;
            }

            LinkedListNode<ISetSpriteObject> current = awaitSetImages.First;
            while (current != null)
            {
                LinkedListNode<ISetSpriteObject> next = current.Next;
                SpriteCollection currentCollection = collection;
                if (current != awaitSetImages.First)
                {
                    SpriteCollectionItemObject itemObject = m_SpriteCollectionPool.Spawn(collectionPath);
                    if (itemObject == null)
                    {
                        Log.Error("Can not spawn SpriteCollection from '{0}'.", collectionPath);
                        ReleaseWaitSetObjectsFromNode(current);
                        break;
                    }

                    currentCollection = (SpriteCollection)itemObject.Target;
                }

                current.Value.SetSprite(currentCollection.GetSprite(current.Value.SpritePath));
                m_LoadSpriteObjectsLinkedList.AddLast(new LoadSpriteObject(current.Value, currentCollection));
                current = next;
            }

            ClearWaitSetObjects(collectionPath, false);
        }

        private void ReleaseWaitSetObjectsFromNode(LinkedListNode<ISetSpriteObject> node)
        {
            while (node != null)
            {
                LinkedListNode<ISetSpriteObject> next = node.Next;
                node.Value.SetSprite(null);
                ReferencePool.Release(node.Value);
                node = next;
            }
        }

        private void Start()
        {
            ObjectPoolComponent objectPoolComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            m_SpriteCollectionPool = objectPoolComponent.CreateMultiSpawnObjectPool<SpriteCollectionItemObject>(
                "SpriteCollection",
                m_AutoReleaseInterval, 16, 60, 0);
            m_LoadSpriteObjectsLinkedList = new LinkedList<LoadSpriteObject>();
            m_SpriteCollectionBeingLoaded = new HashSet<string>();
            m_WaitSetObjects = new Dictionary<string, LinkedList<ISetSpriteObject>>();

            InitializedResources();
        }
        
        private void Update()
        {
            m_CheckCanReleaseTime += Time.unscaledDeltaTime;
            if (m_CheckCanReleaseTime < (double)m_CheckCanReleaseInterval)
                return;
            ReleaseUnused();
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

            m_CheckCanReleaseTime = 0;
        }
    }
}
