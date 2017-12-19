// CS8311: Cannot use a default literal as an argument to a dynamically dispatched operation
// Line: 10
// Compiler options: -langversion:latest

class C
{
    static void Main ()
    {
        dynamic d = null;
        d.M2 (default);
    }
}