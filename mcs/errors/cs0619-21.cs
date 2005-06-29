// cs0619-21.cs: `Obsolete' is obsolete: `'
// Line: 12
// Compiler options: -unsafe

[System.Obsolete("", true)]
struct Obsolete {
}

class MainClass {
        unsafe public static void Main ()
        {
                System.Console.WriteLine (sizeof (Obsolete));
        }
}