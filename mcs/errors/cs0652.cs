// cs0652.cs: Comparison to integral constant is useless; the constant is outside the range of type 'type'
// Line: 9

class X
{
	void b ()
	{
                byte b = 0;
                if (b == 500)
                    return;
	}

	static void Main () {}
}
