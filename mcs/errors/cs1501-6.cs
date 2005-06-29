// cs1501-6.cs: No overload for method `System.Runtime.CompilerServices.IndexerNameAttribute' takes `4' arguments
// Line: 5

class MainClass {
        [System.Runtime.CompilerServices.IndexerName("A", "", "", "")]
        int this [int index] {
                get {
                        return 0;
                }
        }
    
}

