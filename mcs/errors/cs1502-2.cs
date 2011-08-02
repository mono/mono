// CS1502: The best overloaded method match for `System.Console.WriteLine(bool)' has some invalid arguments
// Line: 10
using System;

public class MainClass
{
        public static void Main()
        {
		test MyBug = new test();
                Console.WriteLine (MyBug.mytest());
	}
}

public class   test
{
        public void mytest()
        {
                Console.WriteLine("test");
	}
}


