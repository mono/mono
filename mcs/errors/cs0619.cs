// cs0619.cs: 'MethodError()' is obsolete: 'Do not use it'
// Line: 17

class Obsolete {
        [System.Obsolete]
        public static void MethodWarning() {
        }
    
        [System.Obsolete("Do not use it.", true)]
        public static void MethodError() {
        }
}

class MainClass {
        public static void Main () {
                Obsolete.MethodWarning();
                Obsolete.MethodError();
        }
}

