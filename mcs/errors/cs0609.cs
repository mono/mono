// cs0609.cs: Cannot set the name attribute on an indexer marked override
// Line: 14
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
        protected override int this[int index] {
                get {
                        return false;
                }
        }
            
        static void Main() {}
}

