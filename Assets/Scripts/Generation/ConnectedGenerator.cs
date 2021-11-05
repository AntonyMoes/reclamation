using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using Random;
using UnityEngine;
using Utils;

namespace Generation {
    public static class ConnectedGenerator {
        public static void GenerateDumb(Rng rng, TileShape map, List<(ConnectableShape, float)> pool, 
            Func<Rng, TileShape, ShapeInfo, Queue<ShapeInfo>, bool> onFirstCandidateFail = null) {
            var startingShape = rng.NextWeightedChoice(pool);
            var (startingPoint, startingRotation) = InsertShapeRandomly(rng, map, startingShape);
            var rotatedStartingShape = (ConnectableShape) startingShape.Rotate(startingRotation);
            map.TryNestShape(rotatedStartingShape, startingPoint, false);

            var added = new HashSet<TileShape>();
            var queue = new Queue<ShapeInfo>();
            added.Add(rotatedStartingShape);
            foreach (var connector in rotatedStartingShape.Connectors) {
                queue.Enqueue(new ShapeInfo(rotatedStartingShape, startingPoint, connector.Key));
            }

            while (queue.Count != 0) {
                var currentInfo = queue.Dequeue();
                foreach (var candidate in rng.NextWeightedShuffle(pool)) {
                    var rotatedCandidate = (ConnectableShape) candidate.Rotate(rng.NextInt(0, 3));
                    if (!(TryConnectAndNest(rng, map, currentInfo, rotatedCandidate) is { } info)) {
                        if (onFirstCandidateFail?.Invoke(rng, map, currentInfo, queue) is { } shouldBreak && shouldBreak)
                            break;

                        continue;
                    }

                    added.Add(info.Shape);
                    foreach (var connector in info.Shape.Connectors) {
                        if (connector.Key == info.Direction)
                            continue;

                        queue.Enqueue(new ShapeInfo(info.Shape, info.Position, connector.Key));
                    }

                    break;
                }
            }

            if (added.Count == 0)
                return;

            var holder = HolderShape.FromChildren(added, map, out var holderPosition);
            map.TryNestShape(holder, holderPosition);
        }

        public static void GenerateDumbRoadAndRiver(Rng rng, TileShape map, List<(ConnectableShape, float)> roadPool, 
            List<(ConnectableShape, float)> riverPool, ConnectableShape bridgeEdge, ConnectableShape bridgeSegment, string riverSymbol) {
            GenerateDumb(rng, map, riverPool);
            GenerateDumb(rng, map, roadPool, (innerRng, innerMap, info, queue) => {
                var connDir = bridgeEdge.Connectors.First().Key;
                var edgeLenght = connDir == Direction.West || connDir == Direction.East
                    ? bridgeEdge.Size.x
                    : bridgeEdge.Size.y;

                var riverCheckCoords = info.Direction.ToVector() * edgeLenght + info.Position;
                var riverCheckTile = innerMap.IsInShape(riverCheckCoords) ? innerMap.MergeWithNested()[riverCheckCoords] : null;
                var bridgeNeeded = riverCheckTile is { } rt && rt.Symbol == riverSymbol;
                if (!bridgeNeeded) {
                    return false;
                }

                var added = new List<TileShape>();
                var currentInfo = info;
                var bridgeStarted = false;
                using var exitStack = new ExitStack();
                exitStack.Add(() => {
                    if (added.Count == 0)
                        return;

                    var holder = HolderShape.FromChildren(added, map, out var holderPosition);
                    map.TryNestShape(holder, holderPosition);
                });

                while (true) {
                    if (!(TryConnectAndNest(innerRng, innerMap, currentInfo, bridgeEdge) is { } edgeInfo)) {
                        if (!bridgeStarted)
                            return false;

                        if (!(TryConnectAndNest(innerRng, innerMap, currentInfo, bridgeSegment) is { } segmentInfo))
                            return true;

                        added.Add(segmentInfo.Shape);
                        currentInfo = NewInfoFromConnected(segmentInfo);
                    } else {
                        added.Add(edgeInfo.Shape);
                        var newInfo = NewInfoFromConnected(edgeInfo);
                        if (!bridgeStarted) {
                            bridgeStarted = true;
                            currentInfo = newInfo;
                        } else {
                            queue.Enqueue(newInfo);
                            return true;
                        }
                    }
                }
            });

            ShapeInfo NewInfoFromConnected(ShapeInfo info) =>
                new ShapeInfo(info.Shape, info.Position, info.Direction.Opposite());
        }

        private static (Vector2Int position, int rotation) InsertShapeRandomly(Rng rng, TileShape map, TileShape shape) {
            Vector2Int RandomPosition() => new Vector2Int(rng.NextInt(0, map.Size.x), rng.NextInt(0, map.Size.y));

            while (true) {
                var position = RandomPosition();
                var rotationOffset = rng.NextInt(0, 3);
                foreach (var direction in DirectionMethods.Clockwise()) {
                    var rotation = Direction.North.RotationTo(direction) + rotationOffset;
                    var rotatedShape = shape.Rotate(rotation);

                    if (map.CanNestShape(rotatedShape, position, false)) {
                        return (position, rotation);
                    }
                }
            }
        }

        private static ShapeInfo TryConnectAndNest(Rng rng, TileShape map, ShapeInfo currentInfo, ConnectableShape candidate, bool recursively = false) {
            var (shape, position, direction) = currentInfo;
            var offsetCandidate = (ConnectableShape) candidate.Rotate(rng.NextInt(0, 3));
            if (!shape.TryConnectRotating(offsetCandidate, direction, out var offset, out var rotatedCandidate, out _))
                return null;

            var connectedPosition = position + offset;
            if (!map.TryNestShape(rotatedCandidate, connectedPosition, recursively))
                return null;

            return new ShapeInfo(rotatedCandidate, connectedPosition, direction.Opposite());
        }

        public class ShapeInfo {
            public readonly ConnectableShape Shape;
            public readonly Vector2Int Position;
            public readonly Direction Direction;

            public ShapeInfo(ConnectableShape shape, Vector2Int position, Direction direction) {
                Shape = shape;
                Position = position;
                Direction = direction;
            }

            public void Deconstruct(out ConnectableShape shape, out Vector2Int position, out Direction direction) {
                shape = Shape;
                position = Position;
                direction = Direction;
            }
        }
    }
}
