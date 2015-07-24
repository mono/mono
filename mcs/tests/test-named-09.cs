using System.Collections.Generic;

class X
{
	public static int Main ()
	{
		switch (nameof (Dictionary<int,int>.Add)) {
			case nameof (List<int>.Equals):
				return 1;
			case nameof(List<int>.Add):
				return 0;
			default:
				return 2;
		}		
	}
}