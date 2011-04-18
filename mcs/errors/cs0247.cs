// CS0247: Cannot use a negative size with stackalloc
// Line: 7
// Compiler options: -unsafe

public class MainClass {
        static unsafe void Main () {
                int* ptr = stackalloc int[-1];
        }
}


