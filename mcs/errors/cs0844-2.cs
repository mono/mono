// CS0844: A local variable `y' cannot be used before it is declared. Consider renaming the local variable when it hides the member `X.y'
// Line: 8

class X
{
	static int y;
	static void Main () {
		y = 10;
		int y = 5;
	}
}
