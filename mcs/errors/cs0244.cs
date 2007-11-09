// CS0244: The `is' operator cannot be applied to an operand of pointer type
// Line: 7
// Compiler options: -unsafe

class UnsafeClass {
        unsafe UnsafeClass (int* pointer) {
                if (pointer is string) {}
        }
}


