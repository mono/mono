// cs0135.cs: 'test' conflicts with the declaration 'ClassMain.test'
// Line: 11

class ClassMain {
        static bool test = true;
    
        public static void Main() {
                if (true) {
                        bool test = false;
                }
                test = false;
        }
}

