// cs0136: 'y' has a different meaning later in the block
// Line: 8

class X
{
	static int y;
	static void Main () {
		y = 10;
		int y = 5;
	}
}
