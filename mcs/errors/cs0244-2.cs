// CS0244: The `is' operator cannot be applied to an operand of pointer type
// Line: 9
// Compiler options: -unsafe

class C
{
    static unsafe void Main()
    {
        bool p = null is int*;
    }
}
