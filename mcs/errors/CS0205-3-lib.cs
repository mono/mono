// CS0205: Cannot call an abstract base member `A.Foobar'
// Line: 15

public abstract class A
{
        protected abstract int Foobar { get; }
}

public abstract class A1 : A { }
