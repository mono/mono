class Ref {}

class Def : Ref {}

interface IFooRef {
    Ref Bar { get; }
}

interface IFooDef : IFooRef {
    new Def Bar { get; set; }
}

class FooProcessor<T> where T : IFooDef {
    public void Attach (T t, Def def)
    {
        t.Bar = def;
    }
}

class Program {
    public static void Main ()
    {
    }
}
