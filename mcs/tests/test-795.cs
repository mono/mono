using System;
using System.Reflection;

public class Test : MarshalByRefObject
{
	public DateTime Stamp = new DateTime (1968, 1, 2);

	public static int Main ()
	{
		var setup = new AppDomainSetup();
		setup.ApplicationBase = System.Environment.CurrentDirectory;

		AppDomain d = AppDomain.CreateDomain ("foo", AppDomain.CurrentDomain.Evidence, setup);

		Test t = (Test) d.CreateInstanceAndUnwrap (Assembly.GetExecutingAssembly().FullName, typeof (Test).FullName);
		t.Stamp = new DateTime (1968, 1, 3);
		Console.WriteLine (t.Stamp);
		return 0;
	}
}
