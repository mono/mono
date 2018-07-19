// CS0122: `A.Y' is inaccessible due to its protection level
// Line: 8

public class Test
{
	public static void Main ()
	{
		var x = nameof (A.Y);
	}
}
	 
public class A
{
	private int Y { get; set; }
}