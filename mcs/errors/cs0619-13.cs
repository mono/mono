// CS0619: `Obsolete' is obsolete: `'
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