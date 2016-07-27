// CS0163: Control cannot fall through from one case label `case 1:' to another
// Line: 9

public class Program
{
	public static void Main ()
	{
		switch (1) {
			case 1: {}
			default: {}
		}
	}
}