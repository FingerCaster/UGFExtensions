using System;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace UGFExtensions.SpriteCollection
{
    public partial class SpriteCollectionComponent
    {

        [Serializable]
        public class LoadSpriteObject
        {
#if ODIN_INSPECTOR
            [ShowInInspector] public ISetSpriteObject SpriteObject { get; }
            [ShowInInspector] public SpriteCollection Collection { get; }

            public LoadSpriteObject(ISetSpriteObject obj, SpriteCollection collection)
            {
                SpriteObject = obj;
                Collection = collection;
            }
#else
#if UNITY_EDITOR
            public bool IsSelect  { get; set; }
#endif
            public ISetSpriteObject SpriteObject { get; }
            public SpriteCollection Collection { get; }
            
            public LoadSpriteObject(ISetSpriteObject obj, SpriteCollection collection)
            {
                SpriteObject = obj;
                Collection = collection;
            }
#endif
        }
    }
}