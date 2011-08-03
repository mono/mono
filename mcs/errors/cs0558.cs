// CS0558: User-defined operator `SampleClass.implicit operator bool(SampleClass)' must be declared static and public
// Line: 5

class SampleClass {
        static implicit operator bool (SampleClass value) {
                return true;
        }
}
