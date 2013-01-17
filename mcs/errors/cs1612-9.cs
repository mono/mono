// CS1612: Cannot modify a value type return value of `R.Size'. Consider storing the value in a temporary variable
// Line: 19

struct R
{
	public S Size { get; set; }
}

struct S
{
	public float Height { get; set; }
}

public class Test
{
	public static void Main ()
	{
		var r = new R ();
		r.Size.Height = 3;
	}
}
