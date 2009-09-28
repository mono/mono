using System;

public class My
{
	bool ContentTransferEncoding {
		set { }
	}
}

public static class Test
{
	public static int Main ()
	{
		var test = new My ();

		int result = test.ContentTransferEncoding ();
		if (result != 1)
			return 1;

		result = test.ContentTransferEncoding<int> ();
		if (result != 2)
			return 2;

		return 0;
	}

	public static int ContentTransferEncoding<T> (this My email)
	{
		return 2;
	}

	public static int ContentTransferEncoding (this My email)
	{
		return 1;
	}
}
