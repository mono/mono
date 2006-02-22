// cs0592-3.cs: The attribute `System.Runtime.CompilerServices.IndexerNameAttribute' is not valid on this declaration type. It is valid on `property, indexer' declarations only
// Line : 6

using System.Runtime.CompilerServices;

[IndexerName("XXX")]
class A {
        public static void Main () { }
        
}
