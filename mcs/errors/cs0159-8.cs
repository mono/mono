// CS0159: The label `a:' could not be found within the scope of the goto statement
// Line: 9

public class A
{
	public static void Main ()
	{
		int i = 9;
		goto a;
		switch (i) {
		case 9:
		a:
			break;
		}
	}
}

