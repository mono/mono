// cs0619-29.cs: `Obsolete.Error' is obsolete: `Do not use it.'
// Line: 15

class Obsolete {
        [System.Obsolete("Do not use it.", true)]
        public static bool Error {
            get {
                return false;
            }
        }
}

class MainClass {
        public static void Main () {
                System.Console.WriteLine (Obsolete.Error);
        }
}