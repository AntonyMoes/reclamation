using System.Collections.Generic;
using Generation;
using Map;
using UnityEngine;

namespace Utils {
    public static class Extensions {
        public static int Mod(this int a, int b) {
            var remainder = a % b;
            return remainder >= 0 ? remainder : remainder + b;
        }

        public static IEnumerable<Vector2Int> Iterate(this Vector2Int vector) {
            for (var i = 0; i < vector.x; i++) {
                for (var j = 0; j < vector.y; j++) {
                    yield return new Vector2Int(i, j);
                }
            }
        }

        public static TileShape ToTileShape(this TextAsset asset, TileDictionary dictionary) {
            return TileShape.FromCsv(asset.text, dictionary);
        }

        public static ConnectableShape ToConnectableShape(this TextAsset asset, TileDictionary dictionary) {
            return ConnectableShape.FromTileShape(TileShape.FromCsv(asset.text, dictionary));
        }
    
    
    }
}
