using GameFramework;
using GameFramework.ObjectPool;
using UnityGameFramework.Runtime;

namespace UGFExtensions.SpriteCollection
{
    public class SpriteCollectionItemObject : ObjectBase
    {
        public static SpriteCollectionItemObject Create(string collectionPath ,SpriteCollection target)
        {
            SpriteCollectionItemObject item = ReferencePool.Acquire<SpriteCollectionItemObject>();
            item.Initialize(collectionPath, target);
            return item;
        }
        protected override void Release(bool isShutdown)
        {
            SpriteCollection spriteCollection = (SpriteCollection) Target;
            if (spriteCollection == null)
            {
                return;
            }
            GameEntry.Resource.UnloadAsset(spriteCollection);
        }
    }
}