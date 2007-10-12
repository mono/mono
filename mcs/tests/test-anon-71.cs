// Compiler options: -unsafe

using System;

class Program
{
	private delegate int FdCb (int fd);
	private static Errno ProcessFile (string path, FdCb cb)
	{
		return Errno.Ok;
	}

	protected unsafe Errno OnReadHandle (string path, byte [] buf, long offset)
	{
		Errno e = ProcessFile (path, delegate (int fd) {
			fixed (byte* pb = buf) {
				return 5;
			}
		});
		return e;
	}

	public enum Errno
	{
		Ok = 1
	}
	
	public static void Main ()
	{
	}
}

