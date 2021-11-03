using System;

namespace Map {
    [Serializable]
    public class TileData {
        public bool placeableOn = true;
        public MetaData metaData;
    }
}