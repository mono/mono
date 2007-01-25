// CS0652: A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `byte'
// Line: 9
// Compiler options: -warnaserror -warn:2

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
