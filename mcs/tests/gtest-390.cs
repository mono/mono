using System;

class Program
{
	public static int Main ()
	{
		Error error = Error.FILE_NOT_FOUND;
		return (error == null) ? 1 : 0;
	}
}

enum Error
{
	FILE_NOT_FOUND
}

