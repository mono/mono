// CS1956: The interface method `I<string>.M(out string)' implementation is ambiguous between following methods: `A<string,string>.M(out string)' and `A<string,string>.M(ref string)' in type `Test'
// Line: 17
// Compiler options: -warnaserror

interface I<T>
{
	void M (out T x);
}

class A<T, U>
{
	public virtual void M (out T t)
	{
		t = default (T);
	}

	public virtual void M (ref U u)
	{
	}
}

class Test : A<string, string>, I<string>
{
	static void Main ()
	{
		I<string> x = new Test ();
		string s;
		x.M (out y);
	}
}
