// Compiler options: -t:library

public interface IFoo
{
	void Test<T> ();

	void Test<U,V> ();
}

public interface IBar<T>
{
	void Test ();
}

public interface IBar<U,V>
{
	void Test ();
}
