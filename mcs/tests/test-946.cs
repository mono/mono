using System;

class X
{
	public static void Main ()
	{

	}

	int ImportScope (int scope)
	{
		switch (scope) {
		case 200:
			throw new NotImplementedException ();
		}

		throw new NotSupportedException ();
	}	
}