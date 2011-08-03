// CS0557: Duplicate user-defined conversion in type `SampleClass'
// Line: 5

class SampleClass {
        public static implicit operator bool (SampleClass value) {
                return true;
        }
        
        public static implicit operator bool (SampleClass value) {
                return true;
        }
}
