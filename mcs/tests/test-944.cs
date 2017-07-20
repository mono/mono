// Compiler options: -r:test-944-lib.dll

public class Class2
{
	public static void Main ()
	{
		var writer = new Class1();
		byte[] bytes = writer.Finalize();
	}
}