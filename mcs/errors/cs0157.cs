// cs0157.cs: Control can not leave the body of a finally clause
// Line: 9

class X {
	void A ()
	{
		try {
		} finally {
			return;
		}
	}
}
