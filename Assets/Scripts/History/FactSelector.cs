using System;

namespace History {
    public class FactSelector<T> {
        private readonly string _path;
        private readonly bool _allowNull;

        public FactSelector(string path, bool allowNull = false) {
            _path = path;
            _allowNull = allowNull;
        }

        public Fact<T> GetFact(FactCollection collection) {
            var facts = _path.Split('.');
            if (facts.Length == 0)
                throw new ArgumentException();

            var currentCollection = collection;
            foreach (var collName in new ArraySegment<string>(facts, 0, facts.Length - 1)) {
                var newCollection = currentCollection[collName];
                if (newCollection == null) {
                    if (!_allowNull) {
                        throw new ArgumentException($"Null encountered on {collName} from {_path}");
                    } else {
                        newCollection = new Fact<FactCollection>(collName);
                        currentCollection[collName] = newCollection;
                    }
                }

                if (!(newCollection is Fact<FactCollection> subCollection)) {
                    throw new ArgumentException($"Expected {newCollection.Name} from {_path} to be a FactCollection!");
                }

                currentCollection = subCollection.Value;
            }
            
            // get fact and check type
            throw new NotImplementedException();
        }
    }
}
