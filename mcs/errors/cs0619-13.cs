// cs0619-13.cs: `Obsolete' is obsolete: `'
// Line: 11

[System.Obsolete("", true)]
class Obsolete {
}

class MainClass {
        public static void Main ()
        {
                System.Type t = typeof (Obsolete);
        }
}