public class Foo
{
	static void Bar (System.Threading.ThreadStart ts)
	{
	}

	static void Baz (int yy)
	{
	}

	public static void Main ()
	{
		Bar (delegate {
			foreach (string x in new string[] { "x" }) {
				int yy;
				switch (x) {
				case "x": yy = 1; break;
				default: continue;
				}
				Baz (yy);
			}
		});
	}
}
