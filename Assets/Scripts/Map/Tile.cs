using System.Collections.Generic;
using System.Linq;

namespace Map {
    public class Tile {
        public string Symbol { get; }
        public bool PlaceableOn { get; }
        public MetaData MetaData { get; }

        public Tile(string symbol, TileData tileData) {
            tileData ??= new TileData();
            Symbol = symbol;
            PlaceableOn = tileData.placeableOn;
            MetaData = tileData.metaData;
        }
    }

    public static class TileMethods {
        private static readonly List<(string text, Tile tile)> Mapping = new List<(string text, Tile tile)> {
            (" ", null),
            ("", null),  // additional mapping for parsing purposes
        };
        
        public static string ToText(this Tile tile) {
            var mappingIndex = Mapping.FindIndex(p => p.tile == tile);
            if (mappingIndex != -1)
                return Mapping[mappingIndex].text;

            return tile.Symbol;
        }

        
        public static Tile ToTile(this string textTile, TileDictionary tileDictionary) {
            var mappingIndex = Mapping.FindIndex(p => p.text == textTile);
            if (mappingIndex != -1)
                return Mapping[mappingIndex].tile;
            
            tileDictionary.TryGetValue(textTile, out var tileData);
            return new Tile(textTile, tileData);
        }
    }
}