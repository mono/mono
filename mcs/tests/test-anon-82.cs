//
// Tests different anonymous method caching scenarios
//

public delegate void StringSender (string str);
public delegate void VoidDelegate ();

public class MainClass
{
	public static void Main()
	{
		MainClass mc = new MainClass ();
		VoidDelegate del = new VoidDelegate (
			delegate {
				StringSender ss = delegate (string s) {
					SimpleCallback(mc, s);
				};
				ss("Yo!");
			}
		);
		del();
		
		mc.Test2 (10);
		mc.Test3 (20);
		mc.Test4 ();
		mc.Test5 (50);
	}
	
	void Test2 (int a)
	{
		StringSender d = delegate (string s) {
			VoidDelegate d2 = delegate {
				s = "10";
			};
		};
	}
	
	void Test3 (int a)
	{
		int u = 8;
		VoidDelegate d = delegate () { u = 9; };
		VoidDelegate d2 = delegate () { };
	}

	void Test4 ()
	{
		VoidDelegate d = delegate () {
			VoidDelegate d2 = delegate () {
				int a = 9;
				VoidDelegate d3 = delegate () {
					VoidDelegate d4 = delegate () {
						a = 3;
					};
				};
			};
		};
	}
	
	int a;
	int b;
	
	delegate int D (int a);
	
	void Test5 (int arg)
	{
		D d2 = delegate (int i) {
			D d1 = delegate (int a) {
				return a;
			};

			return d1 (9) + arg;
		};
	}
	
	static void SimpleCallback (MainClass mc, string str)
	{
		System.Console.WriteLine(str);
	}
}
