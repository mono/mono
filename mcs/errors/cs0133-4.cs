// CS0133: The expression being assigned to `S.pathName' must be a constant or default value
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
