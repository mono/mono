// cs1642.cs: Fixed sized buffer of length '1073741825' and type 'long' is too big
// Line: 7
// Compiler options: -unsafe

public unsafe struct C
{
    private fixed long test_1 [1073741825];
}
