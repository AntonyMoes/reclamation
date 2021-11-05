using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map {
    public class TileShape {
        private readonly List<List<Tile>> _tiles;
        public Vector2Int Size => new Vector2Int(_tiles.Count, _tiles[0].Count);
        public Tile this[Vector2Int idx] {
            get => _tiles[idx.x][idx.y];
            private set => _tiles[idx.x][idx.y] = value;
        }
        public List<(TileShape shape, Vector2Int pivot)> NestedShapes { get; }

        protected TileShape(List<List<Tile>> tiles, List<(TileShape, Vector2Int)> nestedShapes = null) {
            _tiles = tiles;
            NestedShapes = nestedShapes ?? new List<(TileShape, Vector2Int)>();
        }

        protected TileShape(TileShape other) : this(other.Copy()._tiles, other.Copy().NestedShapes) { }

        #region Parsing

        public static TileShape FromCsv(string csv, TileDictionary tileDictionary, char sep = ',') {
            var tiles = csv.Trim()
                .Replace("\r", "")
                .Split('\n')
                .Select(row => row
                    .Split(sep)
                    .Select(str => str.ToTile(tileDictionary))
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
            var tilesCopy = Copy();

            foreach (var (shape, pivot) in NestedShapes) {
                var mergedShape = shape.MergeWithNested();
                var shapeSize = mergedShape.Size;
                foreach (var idx in shapeSize.Iterate()) {
                    if (!IsInShape(idx + pivot))  // only possible when the corresponding child tile is null
                        continue;

                    tilesCopy[idx + pivot] = mergedShape[idx] ?? tilesCopy[idx + pivot];
                }
            }

            return new TileShape(tilesCopy._tiles);
        }

        public virtual bool CanNestShape(TileShape other, Vector2Int coords, out TileShape parent, bool recursively = true) {
            if (recursively) {
                foreach (var (shape, pivot) in NestedShapes) {
                    var canNest = shape.CanNestShape(other, coords - pivot, out var nestedParent);
                    if (canNest) {
                        parent = nestedParent;
                        return true;
                    }
                }
            }

            var nestedShape = MergeWithNested();
            foreach (var otherIdx in other.Size.Iterate()) {
                var otherTile = other[otherIdx];
                if (otherTile == null)
                    continue;

                var idx = coords + otherIdx;
                if (!IsInShape(idx)) {  // when the other's tile is not null but doesn't fit in the current shape
                    parent = null;
                    return false;
                }

                var tile = nestedShape[idx];
                if (tile != null && tile.CanPlaceOther(otherTile))
                    continue;

                parent = null;
                return false;
            }

            parent = this;
            return true;
        }

        public bool CanNestShape(TileShape other, Vector2Int coords, bool recursively = true) {
            return CanNestShape(other, coords, out _, recursively);
        }

        public bool TryNestShape(TileShape other, Vector2Int coords, out TileShape parent, bool recursively = true) {
            if (!CanNestShape(other, coords, out parent, recursively))
                return false;

            parent.NestedShapes.Add((other.Copy(), coords));
            return true;
        }
        
        public bool TryNestShape(TileShape other, Vector2Int coords, bool recursively = true) {
            return TryNestShape(other, coords, out _, recursively);
        }

        #endregion

        #region Utils

        public TileShape Copy() {
            return Rotate(0);  // le kek
        }

        public virtual TileShape Rotate(int quatersClockwise) {
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

        private bool IsInShape(Vector2Int localCoords) {
            return !(localCoords.x < 0 || localCoords.x >= Size.x || localCoords.y < 0 || localCoords.y >= Size.y);
        }

        // protected bool IsInShape(Vector2Int localCoords, Vector2Int shapeSize) {
        //     return IsInShape(localCoords) && IsInShape(localCoords + shapeSize - Vector2Int.one);  // TODO: check tiles instead of dimensions
        // }

        #endregion
    }
}