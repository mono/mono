// cs1642.cs: Fixed buffer fields may only be members of structs
// Line: 7
// Compiler options: -unsafe

public unsafe class C
{
    private fixed char test_1 [128];
}
