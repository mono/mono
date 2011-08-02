// CS0210: You must provide an initializer in a fixed or using statement declaration
// Line: 7
// Compiler options: -unsafe

public class MainClass {
        unsafe static void Main () {
                fixed (int* p) {
                }
        }
}

