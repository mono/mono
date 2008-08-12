// CS0719: Array elements cannot be of static type `StaticClass'
// Line: 10

static class StaticClass {
}

class MainClass {
    public static object Method ()
    {
        return new StaticClass [3];
    }
}
