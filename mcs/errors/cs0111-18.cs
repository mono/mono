// CS0111: A member `C.this[params int[]]' is already defined. Rename this member or use different parameter types
// Line: 6

class C
{
    bool this [int[] i] { get { return true; } }
    bool this [params int[] i] { get { return false; } }
}
