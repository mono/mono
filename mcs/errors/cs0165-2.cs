// CS0165: Use of unassigned local variable `x'
// Line: 16

class T {
	void fun (ref int a)
	{
		if (a == 3)
		a = 2;
	}

	void x ()
	{
		int x;

		if (System.Console.Read () == 1)
			x = 1;
		fun (ref x);
	}
}
