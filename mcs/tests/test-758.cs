// Compiler options: -warn:4 -warnaserror

public class C
{
	public int Finalize;
	
	public static void Main ()
	{
	}
}

public class D : C
{
	~D ()
	{
	}
}
