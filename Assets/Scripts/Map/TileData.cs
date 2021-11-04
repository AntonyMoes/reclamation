using System;
using System.Collections.Generic;

namespace Map {
    [Serializable]
    public class TileData {
        public bool placeableOn = true;
        public List<string> placingOverrides = new List<string>();
        public MetaData metaData = new MetaData();
    }
}