// CS1644: Feature `interpolated strings' cannot be used because it is not part of the C# 5.0 language specification
// Line: 9
// Compiler options: -langversion:5

public class Program
{
	public static void Main()
	{
		var x = $"I should not compile";
	}
}
