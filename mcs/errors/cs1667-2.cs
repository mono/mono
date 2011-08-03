// CS1667: Attribute `System.ObsoleteAttribute' is not valid on property or event accessors. It is valid on `class, struct, enum, constructor, method, property, indexer, field, event, interface, delegate' declarations only
// Line: 14

class Test {
        public static bool Error {
            [System.Obsolete] set {
            }
        }
}

class MainClass {
        public static void Main () {
                Test.Error = false;
        }
}