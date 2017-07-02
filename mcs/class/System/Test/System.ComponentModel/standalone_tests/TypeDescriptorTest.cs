using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Web.UI.WebControls;


class Test {
	static void DoTest () {
		for (int i=0; i<100000; i++) {
			TextBox tb = new TextBox ();
			TypeDescriptor.GetProperties (tb);
			tb.Dispose ();
		}
	}

	static void Main(string[] args) {
		Console.WriteLine ("This test should normally take less than a second");
		var sw = Stopwatch.StartNew ();
		DoTest ();
		TimeSpan ts = sw.Elapsed;
		Console.Write ("Time spent: ");
		Console.WriteLine (ts.ToString());
	}
}