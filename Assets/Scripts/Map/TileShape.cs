using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map {
    public class TileShape {  // todo: metadata
        private readonly List<List<Tile>> _tiles;
        public Vector2Int Size => new Vector2Int(_tiles.Count, _tiles[0].Count);
        public Tile this[Vector2Int idx] {
            get => _tiles[idx.x][idx.y];
            private set => _tiles[idx.x][idx.y] = value;
        }
        public List<(TileShape shape, Vector2Int pivot)> NestedShapes { get; }

        private TileShape(List<List<Tile>> tiles, List<(TileShape, Vector2Int)> nestedShapes = null) {
            _tiles = tiles;
            NestedShapes = nestedShapes ?? new List<(TileShape, Vector2Int)>();
        }

        #region Parsing

        public static TileShape FromCsv(string csv, char sep = ',') {
            var tiles = csv.Trim()
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

        public string ToCsv(char sep = ',', bool withNested = true) {
            var shapeToExport = Rotate(-1);
            var tilesToExport = withNested ? shapeToExport.MergeWithNested()._tiles : shapeToExport._tiles;
            return string.Join("\n", tilesToExport
                .Select(row => 
                    string.Join(sep.ToString(), row
                        .Select(tile => tile.ToText())
                        .ToList()))
                .ToList());
        }

        #endregion

        #region Nesting

        public TileShape MergeWithNested() {
            // todo: merge recursively from the deepest level of nesting
            var tilesCopy = Copy();  // le kek

            foreach (var (shape, pivot) in NestedShapes) {
                var shapeSize = shape.Size;
                foreach (var idx in shapeSize.Iterate()) {
                    try {
                        tilesCopy[idx + pivot] = shape[idx];
                    }
                    catch (Exception e) {
                        Debug.Log($"Idx: {idx}, pivot: {pivot}, shapeSize: {shapeSize}, size: {Size}");
                        throw;
                    }
                }
            }

            return new TileShape(tilesCopy._tiles);
        }

        public bool CanNestShape(TileShape other, Vector2Int coords) {
            var nestedShape = MergeWithNested();

            var maxDimensions = coords + other.Size;
            if (maxDimensions.x > Size.x || maxDimensions.y > Size.y)
                return false;

            foreach (var otherIdx in other.Size.Iterate()) {
                if (other[otherIdx] == Tile.None)
                    continue;

                var idx = coords + otherIdx;
                var tile = nestedShape[idx];
                if (tile == Tile.None || tile != Tile.Empty)
                    return false;
            }

            return true;
        }

        public bool TryNestShape(TileShape other, Vector2Int coords) {
            // todo: add to the deepest level of nesting
            if (!CanNestShape(other, coords))
                return false;

            NestedShapes.Add((other.Copy(), coords));
            return true;
        }

        #endregion

        #region Utils

        public TileShape Copy() {
            return Rotate(0);  // le kek
        }

        public TileShape Rotate(int quatersClockwise) {
            var rotations = quatersClockwise.Mod(4);

            var oldRows = _tiles.Count;
            var oldCols = _tiles[0].Count;
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
                    newTiles[iNew][jNew] = _tiles[i][j];
                }
            }

            Vector2Int NestedIndexSelector(Vector2Int idx, Vector2Int rotatedSize) {
                var (xNew, yNew) = IndexSelector(idx.x, idx.y);
                var newIdx = new Vector2Int(xNew, yNew);

                return rotations switch {
                    0 => newIdx,
                    1 => new Vector2Int(xNew, yNew - rotatedSize.y + 1),
                    2 => newIdx - rotatedSize + Vector2Int.one,
                    3 => new Vector2Int(xNew - rotatedSize.x + 1, yNew),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            var newNested = NestedShapes.Select(t => {
                var rotated = t.shape.Rotate(rotations);
                return (rotated, NestedIndexSelector(t.pivot, rotated.Size));
            }).ToList();

            return new TileShape(newTiles, newNested);
        }

        #endregion
    }
}