// CS1664: Fixed size buffer `C.test_1' of length `1073741825' and type `long' exceeded 2^31 limit
// Line: 7
// Compiler options: -unsafe

public unsafe struct C
{
    private fixed long test_1 [1073741825];
}
