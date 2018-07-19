// CS8312: Use of default literal is not valid in this context
// Line: 9
// Compiler options: -langversion:latest

class C
{
	static void Main ()
	{
		foreach (var x in default) {
		}
	}
}