// CS8154: The body of `TestClass.TestFunction()' cannot be an iterator block because the method returns by reference
// Line: 10

class TestClass
{
    int x;

    ref int TestFunction()
    {
        yield return x;
    }
}