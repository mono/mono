// CS0246: The type or namespace name `Uri' could not be found. Are you missing `System' using directive?
// Line: 7

public interface IFoo
{
        string Heh { get; } // significant to cause the error.
        Uri Hoge (); // note that it cannot be resolved here.
}

public class Foo : IFoo
{
        string IFoo.Heh { get { return null; } }
        public System.Uri Hoge () { return null; }
}

