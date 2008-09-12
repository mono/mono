// CS0455: Type parameter `V' inherits conflicting constraints `B' and `A'
// Line: 13

class A
{ }

class B
{ }

class Foo<T,U, V>
	where T : A
	where U : B
	where V : U, T
{
}
