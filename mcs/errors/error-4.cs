// This should not compile as void can't be converted to bool

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


