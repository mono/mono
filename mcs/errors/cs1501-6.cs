// cs1501.cs: No overload for method 'IndexerNameAttribute' takes '4' arguments
// Line: 5

class MainClass {
        [System.Runtime.CompilerServices.IndexerName("A", "", "", "")]
        int this [int index] {
                get {
                        return 0;
                }
        }
    
}

