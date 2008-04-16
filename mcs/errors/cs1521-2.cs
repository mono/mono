// CS1521: Invalid base type `C*'
// Line: 9
// Compiler options: -unsafe

struct C
{
}

unsafe class C2: C*
{
}
