using System;

delegate bool D ();

class Data
{
	public D d;
}

public class Test
{
	int value;
	D change;
	
	static void Foo (int i, D d)
	{
	}
	
	public static void Main ()
	{
	}
	
	void TestMe ()
	{
		if (true) {
			Data data = null;
			if (data != null) {
				D d2 = delegate { return true; };
				change += d2;		
		
				data.d += delegate {
					change -= d2;
					Foo (10, delegate { 
								data = null;
								return false;
							}
						);
					return true;
				};
			}
		}
	}
}

