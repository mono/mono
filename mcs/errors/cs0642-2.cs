// CS0642: Possible mistaken empty statement
// Line: 10
// Compiler options: -warnaserror -warn:3

public class C
{
    public static void Main ()
    {
        int i= 5;
        while (i++ < 100);
            { }
    }
}

