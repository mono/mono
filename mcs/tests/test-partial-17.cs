public class Test
{
	public static bool Check (string name, string [] names)
	{
		foreach (string partial in names) {
			if (name.StartsWith (partial))
				return true;
		}

		return false;
	}

	public static void Main ()
	{
	}
}
