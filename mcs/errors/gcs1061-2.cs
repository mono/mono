// CS1061: Type `string' does not contain a definition for `Length2' and no extension method `Length2' of type `string' could be found (are you missing a using directive or an assembly reference?)
// Line: 12


using System.Linq;

public class M
{
	public static void Main ()
	{
		var e = from values in new [] { "value" }
			let length = values.Length2
			select length;
	}
}
