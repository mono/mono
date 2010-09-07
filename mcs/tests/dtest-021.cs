public class T
{
	class B
	{
		public dynamic this [int i] {
			set {
				int v = (dynamic) value;
			}
			get {
				return i;
			}
		}
	}

	public class Program 
	{
		public static int Main ()
		{
			B b = new B ();
			b [4] = 1;
			dynamic d = b [9];
			return 0;
		}
	}
}