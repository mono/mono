// CS0157: Control cannot leave the body of a finally clause
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
