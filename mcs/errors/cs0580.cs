// cs0580.cs: Too many unnamed arguments to attribute 'System.Runtime.CompilerServices.IndexerName'
// Line: 5

class MainClass {
        [System.Runtime.CompilerServices.IndexerName("Index", "", "", "", "")]
        int this [int index] {
                get {
                        return 0;
                }
        }
    
        public static void Main () {}
}

