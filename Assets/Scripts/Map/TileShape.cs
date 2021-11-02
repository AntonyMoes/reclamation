using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map {
    public class TileShape {
        private List<List<Tile>> Tiles { get; }
        public Vector2Int Size => new Vector2Int(Tiles.Count, Tiles[0].Count);
        public Tile this[Vector2Int idx] => Tiles[idx.x][idx.y];

        private TileShape(List<List<Tile>> tiles) {
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

            if (tiles.Count == 0)
                throw new ApplicationException("Empty shape");

            if (tiles.FirstOrDefault(row => row.Count != tiles[0].Count) is { } unevenRow)
                throw new ApplicationException($"Uneven column count: {tiles[0].Count} in topmost row," +
                                               $"but {unevenRow.Count} in the row number {tiles.IndexOf(unevenRow)}");

            return new TileShape(tiles).Rotate(1);
        }

        public string ToCsv(char sep = ',') {
            var shapeToExport = Rotate(-1);
            return string.Join("\n", shapeToExport.Tiles
                    .Select(row => 
                        string.Join(sep.ToString(), row
                        .Select(tile => tile.ToText())
                        .ToList()))
                    .ToList());
        }

        public bool CanNestShape(TileShape other, Vector2Int coords) {
            throw new NotImplementedException();
        }

        public bool TryNestShape(TileShape other, Vector2Int coords) {
            throw new NotImplementedException();
        }

        public TileShape Rotate(int quatersClockwise) {
            var rotations = quatersClockwise.Mod(4);

            var oldRows = Tiles.Count;
            var oldCols = Tiles[0].Count;
            var rows = rotations % 2 == 0 ? oldRows : oldCols;
            var cols = rotations % 2 == 0 ? oldCols : oldRows;
            List<Tile> RowInstantiator(int _) => new Tile[cols].ToList();
            var newTiles = Enumerable.Range(0, rows).Select(RowInstantiator).ToList();

            (int, int) IndexSelector(int i, int j) {
                return rotations switch {
                    0 => (i, j),
                    1 => (j, cols - i - 1),
                    2 => (rows - i - 1, cols - j - 1),
                    3 => (rows - j - 1, i),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            for (var i = 0; i < oldRows; i++) {
                for (var j = 0; j < oldCols; j++) {
                    var (iNew, jNew) = IndexSelector(i, j);
                    newTiles[iNew][jNew] = Tiles[i][j];
                }
            }

            return new TileShape(newTiles);
        }
    }
}