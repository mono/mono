// cs0619.cs: 'Obsolete.MethodError()' is obsolete: 'Do not use it'
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