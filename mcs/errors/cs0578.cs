// CS0578: Conditional not valid on `MainClass.Foo()' because its return type is not void
// Line: 10

class MainClass {
        [System.Diagnostics.Conditional("DEBUG")] bool Foo() { 
                return false;
        }
            
        public static void Main () {}
}

