public delegate void StringSender (string str);
public delegate void VoidDelegate ();

public class MainClass
{
	public static void Main()
	{
		MainClass mc = new MainClass ();
		VoidDelegate del = new VoidDelegate (delegate {
			StringSender ss = delegate (string s) {
				SimpleCallback(mc, s);
			};
			ss("Yo!");
		});
		del();
	}

	static void SimpleCallback (MainClass mc, string str)
	{
		System.Console.WriteLine(str);
	}
}
