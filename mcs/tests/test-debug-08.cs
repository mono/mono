// Compiler options: -optimize

class C
{
	public static void Main ()
	{
		return;
	}
	
	void Foo ()
	{
	}
	
	int Foo2 ()
	{
		return 7;
	}
	
	int Foo3 ()
	{
		{
			{
				return 2;
			}
		}
	}
}
