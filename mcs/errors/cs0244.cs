// cs0244.cs: "is" or "as" are not valid on pointer types
// Line: 7
// Compiler options: -unsafe

class UnsafeClass {
        unsafe UnsafeClass (int* pointer) {
                if (pointer is string) {}
        }
}


