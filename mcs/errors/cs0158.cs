// CS0158: The label `start' shadows another label by the same name in a contained scope
// Line: 9

class ClassMain {
        public static void Main() {
                start:
                {
                        start:  
                        goto start;
                }
        }
}

