// CS0609: Cannot set the `IndexerName' attribute on an indexer marked override
// Line: 15

using System.Runtime.CompilerServices;

class BaseClass {
        protected virtual bool this[int index] {
                get {
                        return true;
                }
        }
}

class DerivedClass: BaseClass {
        [IndexerName("Error")]
        protected override bool this[int index] {
                get {
                        return false;
                }
        }
}

