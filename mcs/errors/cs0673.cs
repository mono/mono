//
// cs0673.cs: System.Void cannot be used from C# -- use typeof (void) to get the void type object.
// 

public class X
{
	public static void Main()
	{
		Type t = typeof (System.Void);
	}
}
