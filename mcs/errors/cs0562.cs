// cs0562.cs: The parameter of a unary operator must be the containing type
// Line: 5

class SampleClass {
        public static SampleClass operator - (int value) {
                return new SampleClass();
        }
}
