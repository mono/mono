// CS4009: `C.Main()': an entry point cannot be async method
// Line: 8

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
