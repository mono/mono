public class T
{
	protected class Protected
	{
	}
}

public class D : T
{
	private class Private
	{
		public void Stuff (Protected p)
		{
		}
	}
}

public class D2 : T
{
	public class P
	{
		private class Private
		{
			public void Stuff (Protected p)
			{
			}
		}
	}
}

public class Z
{
	public static void Main ()
	{
	}
}
