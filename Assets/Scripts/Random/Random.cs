using System.Collections.Generic;
using System.Linq;

namespace Random {
    public class Random
    {
        private readonly System.Random _rnd;
        public int Seed { get; }
        
        public Random(int seed) {
            Seed = seed;
            _rnd = new System.Random(seed);
        }

        public int NextInt(int min, int max) {
            return _rnd.Next(min, max);
        }

        public float NextFloat(float min, float max) {
            return min + (float) _rnd.NextDouble() * (max - min);
        }

        public T NextChoice<T>(IReadOnlyList<T> collection) {
            var idx = NextInt(0, collection.Count);
            return collection[idx];
        }

        public List<T> NextShuffle<T>(IEnumerable<T> collection) {
            return collection.OrderBy(_ => NextFloat(0, 1)).ToList();
        }
    }
}