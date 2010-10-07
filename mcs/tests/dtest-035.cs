using System;

public class Test
{
	public dynamic this[int i] {
		get { 
			return 0;
		}
		set {
		}
	}

	public dynamic[] Prop {
		get {
			return new dynamic [] { 0 };
		}
		set {
		}
	}
	
	public int Prop2 {
		get {
			return 5;
		}
		set {
		}
	}
	
	int prop = 500;
	public dynamic Prop3
	{
		get { return prop; }
		set { prop = value; }
	}

	public static int Main ()
	{
		int i = 0;
		var d = new dynamic[] { 1 };
		d[i++] += null;
		if (i != 1)
			return 1;
		
		i = 0;
		var t = new Test ();
		t [i++] += null;
		if (i != 1)
			return 2;
		
		i = 0;
		t.Prop [i++] += null;
		if (i != 1)
			return 3;
		
		i = 0;
		d [0] = 9;
		t.Prop2 += d [0];
		
		int v = 3;
		v *= t.Prop3 -= 15;
		if (v != 1455)
			return 4;

		Console.WriteLine ("ok");
		return 0;
	}
}
