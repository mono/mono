public class C
{
	delegate void D ();
	
	static void Test (D d)
	{
	}
	
	public static void Main ()
	{
		int[] i_a = new int [] { 1,2,3 };
		
		Test (delegate () {
				foreach (int t in i_a) {
				}
			});
	}
}