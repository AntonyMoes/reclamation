using System.Collections.Generic;
using System.Linq;

namespace Random {
    public class Rng
    {
        private readonly System.Random _rnd;
        public int Seed { get; }
        
        public Rng(int seed) {
            Seed = seed;
            _rnd = new System.Random(seed);
        }

        public static int RandomSeed => UnityEngine.Random.Range(0, int.MaxValue);

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