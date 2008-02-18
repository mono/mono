

public static class Gen
{
	public static T Test_1<T> (this T t)
	{
		return default (T);
	}
	
	public static string Test_1<T> (this string s)
	{
		return ":";
	}
}

namespace B
{
	public class M
	{
		public static void Main ()
		{
			"".Test_1();
			4.Test_1();
			new M().Test_1();
			
			//null.Test_1();
		}
	}
}