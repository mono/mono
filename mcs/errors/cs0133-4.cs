// cs0133-2.cs: The expression being assigned to `S.pathName' must be constant
// Line: 12
// Compiler options: -unsafe

class C
{
    public static int i = 4;
}

public unsafe struct S
{
    private fixed char pathName [C.i];
}
