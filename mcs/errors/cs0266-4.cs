// cs0266.cs: Cannot implicitly convert type `long' to `int'. An explicit conversion exists (are you missing a cast?)
// Line: 7
// Compiler options: -unsafe

public unsafe struct C
{
    private fixed long test_1 [200000000000];
}

