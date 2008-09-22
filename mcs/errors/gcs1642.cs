// cs1642.cs: `C.test_1': Fixed size buffer fields may only be members of structs
// Line: 7
// Compiler options: -unsafe

public unsafe class C
{
    private fixed char test_1 [128];
}
