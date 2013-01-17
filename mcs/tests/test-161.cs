using System;

public class ZipEntry
{
	DateTime cal = DateTime.Now;

	public ZipEntry(string name)
	{
	}

	public ZipEntry(ZipEntry e)
	{
	}

	public DateTime DateTime {
		get {
			return cal;
		}
	}

	public static int Main () {
		// Compilation only test.
		return 0;
	}
}
