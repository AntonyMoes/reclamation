using System.Linq;

namespace Map {
    public enum Tile {
        None,
        Empty,
        Occupied
    }

    public static class TileMethods {
        private static readonly (string text, Tile tile)[] Mapping = {
            (" ", Tile.None),
            ("", Tile.None),  // additional mapping for parsing purposes
            ("o", Tile.Empty),
            ("x", Tile.Occupied),
        };
        
        public static string ToText(this Tile tile) {
            return Mapping.First(m => m.tile == tile).text;
        }

        public static Tile ToTile(this string textTile) {
            return Mapping.First(m => m.text == textTile).tile;
        }
    }
}