// csc 1.x has a bug

class SampleClass
{
        public static SuperClass operator ++ (SampleClass value) {
                return new SuperClass();
        }
}

class SuperClass: SampleClass
{
        public static int Main ()
        {
            return 0;
        }
}
