// CS4009: `C.Main()': an entry point cannot be async method
// Line: 7
// Compiler options: -langversion:future

class C
{
	public static async void Main ()
	{
		await Call ();
	}
	
	static async void Call ()
	{
	}
}
