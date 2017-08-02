using System;
using System.Threading;

[AttributeUsage(AttributeTargets.Field)]
public sealed class Weak2Attribute : Attribute
{
}

public class Finalizable {

	~Finalizable () {
		Console.WriteLine ("Finalized.");
	}
}

public class Tests
{
	[Weak]
	public object Obj;
	[Weak2]
	public object Obj3;
	[Weak]
	public object Obj2;

	public static int Main (String[] args) {
		var t = new Tests ();
		var thread = new Thread (delegate () {
				t.Obj = new Finalizable ();
				t.Obj2 = new Finalizable ();
				t.Obj3 = new Finalizable ();
			});
		thread.Start ();
		thread.Join ();
		GC.Collect (0);
		GC.Collect ();
		GC.WaitForPendingFinalizers ();
		GC.WaitForPendingFinalizers ();
		if (t.Obj != null)
			return 1;
		if (t.Obj2 != null)
			return 2;
		if (t.Obj3 == null)
			return 3;
		return 0;
	}
	
}
