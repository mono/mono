namespace Test
{
	public interface IInterface
	{
		string Get (string key, string v);
		int Get (string key, int v);
	}

	public class BaseClass
	{
		public string Get (string key, string v)
		{
			return v;
		}

		public int Get (string key, int v)
		{
			return 0;
		}
	}

	public class Subclass : BaseClass, IInterface
	{
		public static void Main ()
		{
			new Subclass ();
		}
	}
}