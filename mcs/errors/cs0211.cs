// cs0211.cs: Cannot take the address of the given expression
// Line: 7
// Compiler options: -unsafe

class UnsafeClass {
        unsafe UnsafeClass () {
                fixed (int* a = &(2)) {}
        }
}

