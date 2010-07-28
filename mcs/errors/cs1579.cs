// CS1579: foreach statement cannot operate on variables of type `X' because it does not contain a definition for `GetEnumerator' or is inaccessible
// Line: 11

class X {
}

class Y {
	void yy (X b)
	{
		
		foreach (object a in b)
			;
	}
}
