// cs0559.cs: The parameter and return type for ++ or — operator must be the containing type
// Line: 5

class SampleClass {
        public static SampleClass operator ++ (int value) {
                return new SampleClass();
        }
}
