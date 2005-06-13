// cs0111-9.cs: Type `C' already defines a member called `this' with the same parameter types
// Line: 6

class C
{
    bool this [int i] { get { return false; } }
    bool this [int i] { get { return true; } }
}
