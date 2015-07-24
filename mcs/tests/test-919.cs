// Compiler options: -unsafe

class Test
{
	public static void Main (string[] args)
	{
		string s = "hello, world!";
		Outer (s);
	}

	static void Outer (string s)
	{
		unsafe {
			fixed (char* sp = s) {
				char* p = sp;
				Inner (ref p, sp);
			}
		}
	}

	static unsafe void Inner (ref char* p, char* sp)
	{
		++sp;
		p = sp;
	}
}