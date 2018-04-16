// Compiler options: -langversion:latest

class C
{
	static void Main()
	{
		M (default, 1);

		M2 (default);
		M2 (null);

		var res = Test (default);
	}


	static void M<T> (T x, T y)
	{
	}

	static void M2 (params object[] x)
	{        
	}

	static byte[] Test (S<byte> x)
	{
		return null;
	}
}

struct S<T>
{

}