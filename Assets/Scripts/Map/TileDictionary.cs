using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map {
    public class TileDictionary : Dictionary<string, TileData> {
        public TileDictionary() { }
        public TileDictionary(IDictionary<string, TileData> copyFrom) : base(copyFrom) { }
        
        public static TileDictionary FromJson(string json) {
            var pairList = JsonUtility.FromJson<SerializedDict>(json).data;

            return new TileDictionary(pairList.ToDictionary(pair => pair.tile, pair => pair.tileData));
        }

        public string ToJson() {
            var pairList = this.Select(pair => new Pair {
                tile = pair.Key,
                tileData = pair.Value
            }).ToList();

            return JsonUtility.ToJson(new SerializedDict { data = pairList });
        }
    }

    [Serializable]
    public class Pair {
        public string tile;
        public TileData tileData;
    }

    [Serializable]
    public class SerializedDict {
        public List<Pair> data;
    }
}