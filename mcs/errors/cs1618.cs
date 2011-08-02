// CS1618: Cannot create delegate with `TestClass.Show(int)' because it has a Conditional attribute
// Line: 13

class TestClass
{
        delegate void test_delegate (int arg);

        [System.Diagnostics.Conditional("DEBUG")]
        public void Show (int arg) {}
            
        public TestClass ()
        {
            test_delegate D = new test_delegate (Show);
        }
}

