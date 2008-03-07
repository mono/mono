// Compiler options: -unsafe

public class aClass
{
	public unsafe struct foo_t
	{
		public fixed char b[16];
	}
	
	public static unsafe void Main(string[] args)
	{
		foo_t bar;
		char* oo = bar.b;
	}
}
