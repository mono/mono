// cs0173.cs: Type of conditional expression can't be determined because there is no implicit conversion between 'ClassA' and 'ClassB'
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
            
                Console.WriteLine(true ? a : b);
   }
}
