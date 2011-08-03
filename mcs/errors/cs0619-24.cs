// CS0619: `Obsolete' is obsolete: `'
// Line: 19
// Compiler options: -unsafe

class Box {
        public Helper o;
}

[System.Obsolete("", true)]
unsafe struct Obsolete {
}

unsafe struct Helper {}

class MainClass {
        unsafe public static void Main ()
        {
                Box b = new Box ();
                fixed (Obsolete* p = &b.o)
                {
                }
        }
}