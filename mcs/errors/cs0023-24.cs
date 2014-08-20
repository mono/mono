// CS0023: The `?' operator cannot be applied to operand of type `int'
// Line: 9

public class C
{
	static void Main()
	{
		string s = null;
		var x = s?.Length?.ToString ();
	}
}