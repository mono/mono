// cs0719.cs: 'StaticClass': array elements cannot be of static type
// Line: 10

static class StaticClass {
}

class MainClass {
    public static object Method ()
    {
        return new StaticClass [3];
    }
}
