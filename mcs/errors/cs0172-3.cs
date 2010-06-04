// CS0172: Type of conditional expression cannot be determined as `ClassA' and `ClassB' convert implicitly to each other
// Line: 29

class ClassA {
        public static implicit operator ClassB (ClassA value) {
                return null;
        }
        
        public static implicit operator ClassA (ClassB value) {
                return null;
        }
}

class ClassB {
        public static implicit operator ClassA (ClassB value) {
                return null;
        }
        
        public static implicit operator ClassB (ClassA value) {
                return null;
        }
}

public class MainClass {
        public static void Main() {
                ClassA a = new ClassA();
                ClassB b = new ClassB();
            
                System.Console.WriteLine(true ? a : b);
   }
}
