// cs0111.cs: Class 'C' already defines a member called 'this' with the same parameter types
// Line: 6

class C
{
    bool this [int i] { get { return false; } }
    bool this [int i] { get { return true; } }
}
