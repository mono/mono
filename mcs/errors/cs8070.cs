// CS8070: Control cannot fall out of switch statement through final case label `case 3:'
// Line: 20

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
