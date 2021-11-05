using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;

namespace Generation {
    public class HolderShape : TileShape {
        protected HolderShape(List<List<Tile>> tiles, List<(TileShape, Vector2Int)> nestedShapes) : base(tiles, nestedShapes) { }

        public static HolderShape FromShapes(List<(TileShape shape, Vector2Int position)> shapes, out Vector2Int position) {
            if (shapes == null || shapes.Count == 0) {
                position = Vector2Int.zero;
                return null;
            }

            var min = shapes[0].position;
            var max = min + shapes[0].shape.Size;
            foreach (var (shape, shapePosition) in shapes) {
                min.x = Mathf.Min(min.x, shapePosition.x);
                min.y = Mathf.Min(min.y, shapePosition.y);
                max.x = Mathf.Max(max.x, shapePosition.x + shape.Size.x);
                max.y = Mathf.Max(max.y, shapePosition.y + shape.Size.y);
            }

            var size = max - min;
            var offset = min;
            var tiles = CreateTiles(size, new Lazy<Tile>(() => new Tile(null, new TileData {placeableOn = true})));

            var nestingCheck = FromTiles(tiles);
            foreach (var (shape, shapePosition) in shapes) {
                if (nestingCheck.TryNestShape(shape, shapePosition - offset))
                    continue;

                position = Vector2Int.zero;
                return null;
            }

            position = offset;
            return new HolderShape(CreateTiles(size), nestingCheck.NestedShapes);
        }

        public static HolderShape FromChildren(IEnumerable<TileShape> children, TileShape parent, out Vector2Int position, bool removeFromParent = true) {
            if (removeFromParent) {
                var removedShapes = children.Select(child => {
                    var removed = parent.TryRemoveNested(child, out var pivot);
                    if (!removed)
                        Debug.LogWarning($"Could not remove child {child.GetHashCode()} from array of children: " +
                                         string.Join(", ", parent.NestedShapes.Select(t => t.shape.GetHashCode())));
                    return (child, pivot);
                }).ToList();

                return FromShapes(removedShapes, out position);
            }

            var shapes = children.Select(child =>
                    (child, parent.NestedShapes.First(t => t.shape == child).pivot))
                .ToList();
            return FromShapes(shapes, out position);
        }

        public override bool CanNestShape(TileShape other, Vector2Int coords, out TileShape parent, bool recursively = true) {if (recursively) {
                foreach (var (shape, pivot) in NestedShapes) {
                    var canNest = shape.CanNestShape(other, coords - pivot, out var nestedParent);
                    if (canNest) {
                        parent = nestedParent;
                        return true;
                    }
                }
            }

            parent = null;
            return false;
        }

    }
}
