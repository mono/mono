// gcs0146.cs: Circular base class dependency involving `B<T>' and `A<T>'
// Line: 8

class A<T> : B<int>
{
}

class B<T> : A<float>
{ }

class X
{
	static void Main ()
	{ }
}
