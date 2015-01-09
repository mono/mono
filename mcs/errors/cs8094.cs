// CS8094: Alignment value has a magnitude greater than 32767 and may result in a large formatted string
// Line: 9
// Compiler options: -warnaserror

class Program
{
	static void Main ()
	{
		var s = $"{1, int.MaxValue }";
	}
}