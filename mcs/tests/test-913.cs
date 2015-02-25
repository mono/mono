namespace N 
{
	using X = A.B;

	class A
	{
		public class B {}

		public class C : X {}

		public static void Main ()
		{
			new C ();
		}
	}
}