class A { }
class B { }

interface I<T>
{
	T Prop { get; set; }
}

class C : I<A>, I<B>
{
	B I<B>.Prop { get; set; }
	A I<A>.Prop { get; set; }
}

class Program
{
	public static void Main (string[] args)
	{
		C c = new C ();
	}
}
