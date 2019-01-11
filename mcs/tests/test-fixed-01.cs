// Compiler options: -unsafe -langversion:latest

unsafe class C
{
	public static void Main ()
	{
		fixed (int* p = new Fixable ()) {
			System.Console.WriteLine (*p);
			System.Console.WriteLine (p [2]);
		}
	}

	struct Fixable
	{
		public ref int GetPinnableReference ()
		{
			return ref (new int[] { 1, 2, 3 })[0];
		}
	}
}