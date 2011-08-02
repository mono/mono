// CS0712: Cannot create an instance of the static class `StaticClass'
// Line: 10

static class StaticClass {
}

public class MainClass {
    public static void Main ()
    {
        new StaticClass ();
    }
}
