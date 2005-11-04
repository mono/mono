using System;

public class Blah {
	
	public const int i = 5;

	public static int Main ()
	{
		const int aaa = 1, bbb = 2;
		const int foo = 10;
		
		int j = Blah.i;

		if (j != 5)
			return 1;

		if (foo != 10)
			return 1;

		for (int i = 0; i < 5; ++i){
			const int bar = 15;

			Console.WriteLine (bar);
			Console.WriteLine (foo);
		}
		if ((aaa + bbb) != 3)
			return 2;

		Console.WriteLine ("Constant emission test okay");

		return 0;
	}
    
    public static void Test_1 ()
    {
        const long lk = 1024;
        const long lM = 1024 * lk;
        const long lG = 1024 * lM;
        const long lT = 1024 * lG;
    }
}
