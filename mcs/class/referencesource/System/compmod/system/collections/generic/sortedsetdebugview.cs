using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace System.Collections.Generic {

    /// <summary>
    /// Debug view for SortedSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SortedSetDebugView<T> {
        private SortedSet<T> set;

        public SortedSetDebugView(SortedSet<T> set) {
            if (set == null) {
                throw new ArgumentNullException("set");
            }

            this.set = set;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items {
            get {
                return set.ToArray();
            }
        }
    }

}
