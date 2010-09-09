using System;

class X {
	~X ()
	{
		int id = 1;
		Console.WriteLine ("DESTRUCTOR!" + id);
	}

        public static int Test1()
	{
                try {
                        return 8;
                } catch (Exception) {}  
                System.Console.WriteLine("Shouldn't get here");
		return 9;
        }

	public static void Test2()
	{
		int[] vars = { 3, 4, 5 };

		foreach (int a in vars) {
			try {
				continue;
			} catch (Exception) {
				break;
			}
		}
	}

        public static void Main() {
		Test1 ();
		Test2 ();

                try {
                        return;
                } catch (Exception) {}  
                System.Console.WriteLine("Shouldn't get here");
		return;
        }
}
