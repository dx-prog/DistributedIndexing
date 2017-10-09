using System;
using System.Collections.Generic;

namespace Sprockets.LargeGraph.Serialization {
    [Serializable]
    public class WorkBacklog<TContext, TData> {
        private readonly Stack<TData> _backlog = new Stack<TData>();

        public WorkBacklog(TContext context) {
            Context = context;
        }

        public TContext Context { get; }
        public bool HasWork => _backlog.Count > 0;

        public void AddWorkFor(TData obj) {
            _backlog.Push(obj);
        }

        public bool Execute(Action<TContext, TData> work) {
            if (_backlog.Count == 0)
                return false;

            var pop = _backlog.Pop();
            work(Context, pop);
            return true;
        }
    }
}