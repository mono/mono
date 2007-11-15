using System;
using System.Data;
using System.Data.Common;

public class Test
{
	static void Main ()
	{
		DataTable dt = DbProviderFactories.GetFactoryClasses ();
		foreach (DataRow row in dt.Rows) {
			string name = row ["Name"] as string;
			if (name.IndexOf ("Odbc") >= 0)
				Console.WriteLine ("Provider=" + name);
		}
	}
}


