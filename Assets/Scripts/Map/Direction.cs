using System;
using UnityEngine;
using Utils;

namespace Map {
    public enum Direction {
        North = 0,
        West = 1,
        South = 2,
        East = 3,
    }

    public static class DirectionMethods {
        public static Direction Rotate(this Direction direction, int rotation) {
            return (Direction) ((int) (direction + rotation)).Mod(4);
        }

        public static Direction Opposite(this Direction direction) {
            return direction.Rotate(2);
        }

        public static int RotationTo(this Direction direction, Direction other) {
            return (other - direction).Mod(4);
        }

        // if we rotate something clockwise from North, the side facing North now is West e.t.c.
        public static Direction[] Clockwise() {
            return new[] {Direction.North, Direction.West, Direction.South, Direction.East};
        }

        public static Vector2Int ToVector(this Direction direction) {
            return direction switch {
                Direction.North => Vector2Int.up,
                Direction.West => Vector2Int.left,
                Direction.South => Vector2Int.down,
                Direction.East => Vector2Int.right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}
