// cs1579.cs: foreach statement cannot operate on variables of type `X' because it does not contain a definition for `GetEnumerator' or is not accessible
// Line: 10
class X {
}

class Y {
	void yy (X b)
	{
		
		foreach (object a in b)
			;
	}
}
