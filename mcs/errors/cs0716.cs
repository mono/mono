// CS0716: Cannot convert to static type `StaticClass'
// Line: 10

static class StaticClass {
}

class MainClass {
    public static void Method (object arg)
    {
        ((StaticClass)arg).ToString ();
    }
}
