using System;
using System.Runtime.InteropServices;

[Obsolete ("Use Errno", true)]
public enum Error {
	EROFS,
	ERANGE = TestConst.C,
	EANOTHER = ERANGE,
}

[Obsolete ("Use Errno", true)]
public sealed class UnixMarshal {
	public static string GetDescription (Error e) {
		return null;
	}
}

public sealed class UnixMarshal2 {
	[Obsolete ("Use Errno", true)]
	public static string GetDescription (Error e) {
		return null;
	}
}

[Obsolete ("Use Native.Stdlib", true)]
public class Stdlib {
	internal const string LIBC = "msvcrt.dll";
	[DllImport (LIBC)]
	public static extern IntPtr signal (int signum, IntPtr handler);
}

class TestConst {
	[Obsolete ("B", true)]
	public const int C = 3;
}

class Test {
	public static void Main () {
	}
}
