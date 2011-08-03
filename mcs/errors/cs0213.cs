// CS0213: You cannot use the fixed statement to take the address of an already fixed expression
// Line: 12
// Compiler options: -unsafe

class UnsafeClass {
        unsafe UnsafeClass () {
                int value = 5;
                Calculate(value);
        }
        
        unsafe void Calculate (int value) {
                fixed (int *x = &value) {}
        }
}


