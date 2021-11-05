using System;
using System.Collections.Generic;

namespace Utils {
    public class ExitStack : IDisposable {
        private bool _disposed;
        private readonly List<Action> _stack = new List<Action>();

        public void Add(Action action) {
            _stack.Add(action);
        }

        public void Dispose() {
            if (_disposed)
                return;

            _disposed = true;
            _stack.Reverse();
            foreach (var action in _stack) {
                action?.Invoke();
            }
        }
    }
}
