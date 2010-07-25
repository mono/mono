// cs0469.cs: The `goto case' value is not implicitly convertible to type `char'
// Line: 16
// Compiler options: -warnaserror -warn:2

class Test
{
	static void Main()
	{
		char c = 'c';
		switch (c)
		{
			case 'A':
				break;

			case 'a': 
				goto case 65;
		}
	}
}