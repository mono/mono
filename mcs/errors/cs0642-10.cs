// CS0642: Possible mistaken empty statement
// Line: 9
// Compiler options: -warnaserror -warn:3 -unsafe -nowarn:0219

public class C
{
    public unsafe void Test ()
    {
        fixed (char *p = str);
            { }
    }

    static readonly char [] str = new char [] {'A'};
}

