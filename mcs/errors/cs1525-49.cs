// CS1525: Unexpected symbol `/'
// Line: 10

namespace Test674
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var a = new A(another: something, sth: /without/quotes);
		}
	}

	public class A
	{
		public A(string sth, string another)
		{
		}
	}
}
