// CS0673: System.Void cannot be used from C#. Use typeof (void) to get the void type object
// Line: 8

public class X
{
	public static void Main()
	{
		System.Type t = typeof (System.Void);
	}
}
