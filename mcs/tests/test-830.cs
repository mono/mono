using System;
using System.Collections.Generic;

public class MC
{
	public struct ObjectInfo
	{
		public long Code;
	}

	public static int Main ()
	{
		var objects = new List<ObjectInfo> ();
		long a = 1;
		long b = 2;

		ObjectInfo aa = new ObjectInfo ();
		aa.Code = a;

		ObjectInfo bb = new ObjectInfo ();
		bb.Code = b;

		objects.Add (aa);
		objects.Add (bb);

		int r1 = objects[0].Code.CompareTo (objects[1].Code);
		int r2 = a.CompareTo (b);
		if (r1 != r2) {
			Console.WriteLine ("FAIL!");
			return 1;
		}

		Console.WriteLine ("OK!");
		return 0;
	}

}