// CS0448: The return type for ++ or -- operator must be the containing type or derived from the containing type
// Line: 5
class SampleClass {
    public static int operator ++ (SampleClass value) {
        return new SampleClass();        
    }
}
