// CS0251: Indexing an array with a negative index (array indices always start at zero)
// Line: 10
// Compiler options: -warn:2 -warnaserror

class Main
{
    public int Method (int[] array)
    {
       const int index = 5;
       return array [index - 10];
    }
}
