using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map {
    public class TileShape {
        public List<List<Tile>> Tiles { get; }

        public TileShape(List<List<Tile>> tiles) {
            Tiles = tiles;
        }

        public static TileShape FromCsv(string csv, char sep = ',') {
            var tiles = csv
                .Replace("\r", "")
                .Split('\n')
                .Select(row => row
                    .Split(sep)
                    .Select(str => str.ToTile())
                    .ToList())
                .ToList();

            // todo: check dimensions
            // todo: rotate map

            return new TileShape(tiles);
        }

        public string ToCsv(char sep = ',') {
            return string.Join("\n", Tiles
                    .Select(row => 
                        string.Join(sep.ToString(), row
                        .Select(tile => tile.ToText())
                        .ToList()))
                    .ToList());
            
            // todo: rotate map back
        }

        public bool CanNestShape(TileShape other, Vector2Int coords) {
            throw new NotImplementedException();
        }

        public bool TryNestShape(TileShape other, Vector2Int coords) {
            throw new NotImplementedException();
        }

        public TileShape Rotate(int quatersClockwise) {
            throw new NotImplementedException();
        }
    }
}