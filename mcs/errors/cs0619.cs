// CS0619: `Obsolete.MethodError()' is obsolete: `Do not use it.'
// Line: 12

class Obsolete {
        [System.Obsolete("Do not use it.", true)]
        public static void MethodError() {
        }
}

class MainClass {
        public static void Main () {
                Obsolete.MethodError();
        }
}