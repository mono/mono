using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.Collections.Generic {

    /// <summary>
    /// Debug view for HashSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class HashSetDebugView<T> {
        private HashSet<T> set;

        public HashSetDebugView(HashSet<T> set) {
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
