// CS0254: The right hand side of a fixed statement assignment may not be a cast expression
// Line: 16
// Compiler options: -unsafe

class Box {
    public int value;
}

unsafe struct Obsolete {
}

class MainClass {
        unsafe public static void Main ()
        {
                Box b = new Box ();
                fixed (long* p = (long*)&b.value)
                {
                }
        }
}