// CS0184: The given expression is never of the provided (`byte') type
// Line: 13
// Compiler options: -warnaserror -warn:1

class S {}
	
class X
{
	static void Main ()
	{
		const S x = null;
		
		if (x is byte) {
		}
	}
}
