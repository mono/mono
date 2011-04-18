// CS1665: `S.test20': Fixed size buffers must have a length greater than zero
// Line: 7
// Compiler options: -unsafe

public unsafe struct S
{
    public fixed bool test20 [-4];
}
