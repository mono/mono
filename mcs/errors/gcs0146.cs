// CS0146: Class definition is circular: `A`1'
// Line: 3
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
