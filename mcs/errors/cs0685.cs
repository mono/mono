// CS0685: Conditional method `MainClass.Method(out int)' cannot have an out parameter
// Line: 6

class MainClass {
        [System.Diagnostics.Conditional("DEBUG")]
        public void Method (out int o)
        {
            o = 3;
        }
}
