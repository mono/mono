// CS0123: A method or delegate `Program.method(A)' parameters do not match delegate `D(dynamic)' parameters
// Line: 19

delegate object D (dynamic b);

class A
{
}

class Program
{
	static string method (A a)
	{
		return "";
	}

	static void Main ()
	{
		var d = new D (method);
	}
}
