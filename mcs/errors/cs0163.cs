// CS0163: Control cannot fall through from one case label to another
// Line: 17


public class Foo
{
	public static void Main()
	{
		int a=5;
		int b=10;
		int c;
		
		switch (a)
		{
			case 1: c=a+b;
				return;

			case 2: c=a-b;
				return;

			case 3: c=a*b;
		}
	}
}
