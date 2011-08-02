// CS0199: A static readonly field `ClassMain.index' cannot be passed ref or out (except in a static constructor)
// Line: 19

class ClassMain {
        static readonly int index;

        static ClassMain ()
        {
                GetMaxIndex (ref index);
        }

        static void GetMaxIndex (ref int value)
        {
                value = 5;
        }

        public static void Main ()
        {
                GetMaxIndex (ref index);
        }
}

