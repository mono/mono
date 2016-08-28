// CS0619: `MainClass.Error' is obsolete: `Do not use it.'
// Line: 6

class MainClass {
        public static void Main () {
                System.Console.WriteLine (Error.DoesNotExist);
        }
        [System.Obsolete("Do not use it.", true)]
        public static object Error {
            get {
                return null;
            }
        }
}