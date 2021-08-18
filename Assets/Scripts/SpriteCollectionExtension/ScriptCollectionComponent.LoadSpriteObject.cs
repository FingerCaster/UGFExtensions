using System;
using Sirenix.OdinInspector;

namespace UGFExtensions.SpriteCollection
{
    public partial class SpriteCollectionComponent
    {
        [Serializable]
        public class LoadSpriteObject
        {
            [ShowInInspector] public ISetSpriteObject SpriteObject { get; }
            [ShowInInspector] public SpriteCollection Collection { get; }

            public LoadSpriteObject(ISetSpriteObject obj, SpriteCollection collection)
            {
                SpriteObject = obj;
                Collection = collection;
            }
        }
    }
}