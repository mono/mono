interface IFoo {}
interface IBar {}

class C1<IFoo> where IFoo : IBar
{
}

abstract class C2
{
        public abstract C1<T> Hoge<T> (C1<T> c) where T : IBar;
}

class C3 : C2
{
        public override C1<T> Hoge<T> (C1<T> c) { return null; }
}

class X
{
	public static void Main ()
	{ }
}
