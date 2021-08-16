using System;
using System.Collections.Generic;
using GameFramework.ObjectPool;
using Sirenix.OdinInspector;
using UGFExtensions.Await;
using UGFExtensions.Timer;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace UGFExtensions.SpriteCollection
{
    public class ScriptCollectionComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 散图集合对象池
        /// </summary>
        private IObjectPool<SpriteCollectionItemObject> m_SpriteCollectionPool;

        /// <summary>
        /// 自动释放时间间隔
        /// </summary>
        [SerializeField] private int m_AutoReleaseInterval = 60;
        

        [ReadOnly] private LinkedList<LoadSpriteImage> m_LoadSpriteImagesLinkedList;
        private HashSet<string> m_SpriteCollectionBeingLoaded;
        private Dictionary<string, LinkedList<AwaitSetImage>> m_AwaitSetImages;

        private void Start()
        {
            ObjectPoolComponent objectPoolComponent =
                UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            m_SpriteCollectionPool = objectPoolComponent.CreateMultiSpawnObjectPool<SpriteCollectionItemObject>(
                "SpriteCollection",
                60.0f, 16, 60, 0);
            TimerComponent timerComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
            timerComponent.AddRepeatedTimer(m_AutoReleaseInterval*1000, -1, Release);
            m_LoadSpriteImagesLinkedList = new LinkedList<LoadSpriteImage>();
            m_SpriteCollectionBeingLoaded = new HashSet<string>();
            m_AwaitSetImages = new Dictionary<string, LinkedList<AwaitSetImage>>();
        }

        /// <summary>
        /// 回收无引用的 Image 对应图集。
        /// </summary>
        private void Release()
        {
            LinkedListNode<LoadSpriteImage> current = m_LoadSpriteImagesLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.Image == null)
                {
                    m_SpriteCollectionPool.Unspawn(current.Value.Collection);
                    m_LoadSpriteImagesLinkedList.Remove(current);
                }

                current = next;
            }
        }

        /// <summary>
        /// 为图像设置精灵
        /// </summary>
        /// <param name="image">图像</param>
        /// <param name="collectionPath">精灵收集组件的地址</param>
        /// <param name="spriteName">精灵名称</param>
        public async void SetSprite(Image image, string collectionPath, string spriteName)
        {
            Log.Info($"collectionPath:{collectionPath}   spriteName:{spriteName}");

            if (m_SpriteCollectionPool.CanSpawn(collectionPath))
            {
                SpriteCollection collectionItem = (SpriteCollection) m_SpriteCollectionPool.Spawn(collectionPath).Target;
                image.sprite = collectionItem.GetSprite(spriteName);
                m_LoadSpriteImagesLinkedList.AddLast(new LoadSpriteImage(image, collectionItem));
                return;
            }

            if (m_AwaitSetImages.ContainsKey(collectionPath))
            {
                var loadSp = m_AwaitSetImages[collectionPath];
                loadSp.AddLast(new AwaitSetImage(spriteName, image));
            }
            else
            {
                var loadSp = new LinkedList<AwaitSetImage>();
                loadSp.AddFirst(new AwaitSetImage(spriteName, image));
                m_AwaitSetImages.Add(collectionPath, loadSp);
            }

            if (m_SpriteCollectionBeingLoaded.Contains(collectionPath))
            {
                return;
            }

            m_SpriteCollectionBeingLoaded.Add(collectionPath);
            SpriteCollection collection = await GameEntry.Resource.LoadAssetAsync<SpriteCollection>(collectionPath);
            m_SpriteCollectionPool.Register(SpriteCollectionItemObject.Create(collectionPath, collection), false);
            m_SpriteCollectionBeingLoaded.Remove(collectionPath);
            m_AwaitSetImages.TryGetValue(collectionPath, out LinkedList<AwaitSetImage> awaitSetImages);
            LinkedListNode<AwaitSetImage> current = awaitSetImages?.First;
            while (current != null)
            {
                m_SpriteCollectionPool.Spawn(collectionPath);
                current.Value.Image.sprite = collection.GetSprite(current.Value.SpriteName);
                m_LoadSpriteImagesLinkedList.AddLast(new LoadSpriteImage(current.Value.Image, collection));
                current = current.Next;
            }
        }

        /// <summary>
        /// 等待赋值Image
        /// </summary>
        private class AwaitSetImage
        {
            public AwaitSetImage(string spriteName, Image image)
            {
                SpriteName = spriteName;
                Image = image;
            }

            /// <summary>
            /// 精灵名称
            /// </summary>
            public string SpriteName { get; private set; }

            /// <summary>
            /// 图像组件
            /// </summary>
            public Image Image { get; private set; }
        }
        
        [Serializable]
        private class LoadSpriteImage
        {
            [ShowInInspector] public Image Image { get; }
            [ShowInInspector] public SpriteCollection Collection { get; }

            public LoadSpriteImage(Image image, SpriteCollection collection)
            {
                Image = image;
                Collection = collection;
            }
        }
    }

    public static class ImageExtensions
    {
        /// <summary>
        /// 设置精灵
        /// </summary>
        /// <param name="image"></param>
        /// <param name="collectionName">精灵所在收集器名称</param>
        /// <param name="spriteName">精灵名称</param>
        public static void SetSprite(this Image image, string collectionName, string spriteName)
        {
            ScriptCollectionComponent scriptCollectionComponent =
                UnityGameFramework.Runtime.GameEntry.GetComponent<ScriptCollectionComponent>();
            scriptCollectionComponent.SetSprite(image, "", spriteName);
        }
    }
}