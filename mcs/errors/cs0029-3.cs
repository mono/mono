// cs0029.cs: Cannot implicitly convert type 'Helper*' to 'Obsolete*'
// Line: 18
// Compiler options: -unsafe

class Box {
        public Helper o;
}

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
