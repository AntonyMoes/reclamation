using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;

namespace Generation {
    public class ConnectableShape : TileShape {
        public readonly Dictionary<Direction, Connector> Connectors;

        protected ConnectableShape(TileShape other, Dictionary<Direction, Connector> connectors) : base(other) {
            Connectors = connectors;
        }

        public static ConnectableShape FromTileShape(TileShape target) {
            // if (target.Size.x < 2 || target.Size.y < 2) {
            //     return null;
            // }

            var connectors = new List<Connector>();
            foreach (var direction in DirectionMethods.Clockwise()) {
                var rotation = Direction.North.RotationTo(direction);
                var rotatedTarget = target.Rotate(rotation);

                var multipleConnectors = false;
                var mismatchingConnectorTypes = false;
                var connectorStart = -1;
                var connectorEnd = -1;  // last + 1
                string type = null;
                for (var i = 0; i < rotatedTarget.Size.x; i++) {
                    var tile = rotatedTarget[new Vector2Int(i, rotatedTarget.Size.y - 1)];
                    if (tile != null && tile.MetaData.tags.Contains(Tags.Connector)) {
                        if (connectorStart == -1) {
                            connectorStart = i;
                            type = tile.MetaData.tags.FirstOrDefault(t => t.StartsWith(Tags.ConnectorType));
                        } else if (connectorEnd != -1) {
                            multipleConnectors = true;
                            break;
                        } else if (tile.MetaData.tags.FirstOrDefault(t => t.StartsWith(Tags.ConnectorType)) != type) {
                            mismatchingConnectorTypes = true;
                            break;
                        }
                    } else if (connectorStart != -1 && connectorEnd == -1) {
                        connectorEnd = i;
                    }
                }

                if (multipleConnectors || mismatchingConnectorTypes)
                    continue;

                if (connectorStart != -1) {
                    if (connectorEnd == -1)
                        connectorEnd = rotatedTarget.Size.x;

                    connectors.Add(new Connector(direction, connectorEnd - connectorStart, connectorStart, type));
                }
            }

            if (connectors.Count == 0)
                return null;

            return new ConnectableShape(target, connectors.ToDictionary(c => c.Direction, c => c));
        }

        public override TileShape Rotate(int quatersClockwise) {
            var rotatedBase = base.Rotate(quatersClockwise);
            var rotatedConnectors = Connectors
                .Select(pair => (pair.Key.Rotate(-quatersClockwise),
                    new Connector(pair.Value.Direction.Rotate(-quatersClockwise),
                        pair.Value.Size, pair.Value.Offset, pair.Value.Type)))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);
            return new ConnectableShape(rotatedBase, rotatedConnectors);
        }

        public bool TryConnect(ConnectableShape other, Direction direction, out Vector2Int otherOffset) {
            if (Connectors.TryGetValue(direction, out var connector) &&
                other.Connectors.TryGetValue(direction.Opposite(), out var otherConnector)) {
                if (connector.Size != otherConnector.Size || connector.Type != otherConnector.Type) {
                    otherOffset = Vector2Int.zero;
                    return false;
                }

                otherOffset = ConnectorPivot(connector) - other.ConnectorPivot(otherConnector) + ShapeOffset(direction);
                return true;
            }

            otherOffset = Vector2Int.zero;
            return false;
        }
        
        public bool TryConnectRotating(ConnectableShape other, Direction direction, out Vector2Int otherOffset, out ConnectableShape otherRotated, out int rotation) {
            foreach (var otherDirection in DirectionMethods.Clockwise()) {
                rotation = Direction.North.RotationTo(otherDirection);
                otherRotated = (ConnectableShape) other.Rotate(rotation);

                if (TryConnect(otherRotated, direction, out otherOffset)) {
                    return true;
                }
            }

            otherRotated = null;
            otherOffset = Vector2Int.zero;
            rotation = 0;
            return false;
        }

        private Vector2Int ConnectorPivot(Connector connector) {
            var sizeCoeff = connector.Direction switch {
                Direction.North => Vector2Int.zero,
                Direction.West => Vector2Int.zero,
                Direction.South => Vector2Int.left,
                Direction.East => Vector2Int.down,
                _ => throw new ArgumentOutOfRangeException()
            };

            var offset = connector.Direction switch {
                Direction.North => (Size.y - 1) * Vector2Int.up + connector.Offset * Vector2Int.right,
                Direction.West => connector.Offset * Vector2Int.up,
                Direction.South => (Size.x - connector.Offset) * Vector2Int.right,
                Direction.East => (Size.x - 1) * Vector2Int.right + (Size.y - connector.Offset) * Vector2Int.up,
                _ => throw new ArgumentOutOfRangeException()
            };

            return sizeCoeff * connector.Size + offset;
        }

        private static Vector2Int ShapeOffset(Direction direction) {
            return direction.ToVector();
        }

        public class Connector {
            public readonly Direction Direction;
            public readonly int Size;
            public readonly int Offset;
            public readonly string Type;

            public Connector(Direction direction, int size, int offset, string type) {
                Direction = direction;
                Size = size;
                Offset = offset;
                Type = type;
            }
        }
    }
}
