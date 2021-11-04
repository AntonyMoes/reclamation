using System.Collections.Generic;
using Map;
using Random;
using UnityEngine;

namespace Generation {
    public static class ConnectedGenerator {
        public static void GenerateDumb(Rng rng, TileShape map, List<ConnectableShape> pool) {
            Vector2Int RandomPos() => new Vector2Int(rng.NextInt(0, map.Size.x), rng.NextInt(0, map.Size.y));
            ConnectableShape RandomShape() => rng.NextChoice(pool);

            var startingShape = RandomShape();
            Vector2Int startingPoint = Vector2Int.zero;
            var placed = false;
            while (!placed) {
                startingPoint = RandomPos();
                foreach (var direction in DirectionMethods.Clockwise()) {
                    var rotation = Direction.North.RotationTo(direction);
                    var rotatedShape = startingShape.Rotate(rotation);

                    if (map.TryNestShape(rotatedShape, startingPoint, false)) {
                        placed = true;
                        startingShape = (ConnectableShape) rotatedShape;
                        break;
                    }
                }
            }

            var queue = new Queue<(ConnectableShape shape, Vector2Int position, Direction direction)>();
            foreach (var connector in startingShape.Connectors) {
                queue.Enqueue((startingShape, startingPoint, connector.Key));
            }

            while (queue.Count != 0) {
                var (shape, position, direction) = queue.Dequeue();
                var rotationOffset = rng.NextInt(0, 3);
                foreach (var candidate in rng.NextShuffle(pool)) {
                    var connected = false;
                    foreach (var candidateDirection in DirectionMethods.Clockwise()) {
                        var rotation = Direction.North.RotationTo(candidateDirection) + rotationOffset;
                        var rotatedCandidate = (ConnectableShape) candidate.Rotate(rotation);

                        if (!shape.TryConnect(rotatedCandidate, direction, out var offset))
                            continue;

                        var connectedPosition = position + offset;
                        connected = map.TryNestShape(rotatedCandidate, connectedPosition, false);
                        if (!connected)
                            continue;

                        foreach (var connector in rotatedCandidate.Connectors) {
                            if (connector.Key == direction.Opposite())
                                continue;

                            queue.Enqueue((rotatedCandidate, connectedPosition, connector.Key));
                        }
                        break;
                    }

                    if (connected)
                        break;
                }
            }
        }
    }
}
