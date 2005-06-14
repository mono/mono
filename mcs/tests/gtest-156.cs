// Compiler options: -r:gtest-156-lib.dll

namespace FLMID.Bugs.Marshal15
{
	public class D : C
	{
		public D()
		{
			_layout = new X();
		}
		public static void Main(string[] args)
		{
			System.Console.WriteLine("OK");
		}
	}
}

