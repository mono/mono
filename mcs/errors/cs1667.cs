// CS1667: Attribute `System.CLSCompliantAttribute' is not valid on property or event accessors. It is valid on `assembly, module, class, struct, enum, constructor, method, property, indexer, field, event, interface, parameter, delegate, return' declarations only
// GMCS1667: Attribute `System.CLSCompliantAttribute' is not valid on property or event accessors. It is valid on `assembly, module, class, struct, enum, constructor, method, property, indexer, field, event, interface, parameter, delegate, return, type parameter' declarations only
// Line: 15

class Test {
        public static bool Error {
            [System.CLSCompliant (true)] get {
                return false;
            }
        }
}

class MainClass {
        public static void Main () {
                System.Console.WriteLine (Test.Error);
        }
}