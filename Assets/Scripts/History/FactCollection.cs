using System;
using System.Collections.Generic;
using System.Linq;

namespace History {
    public class FactCollection {
        private readonly Dictionary<string, IFact> _facts;

        public FactCollection(IEnumerable<IFact> facts) {
            _facts = facts.ToDictionary(f => f.Name, f => f);
        }

        public IFact this[string name] {
            get => _facts.TryGetValue(name, out var result) ? result : null;
            set => _facts[name] = value;
        }
    }
}
