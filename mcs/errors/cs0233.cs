// cs0233.cs: sizeof can only be used in an unsafe context (consider using System.Runtime.InteropServices.Marshal.SizeOf)
// Line: 7
// Compiler options: -unsafe

public class MainClass {
        static void Main () {
                const int size = sizeof(int);
        }
}


