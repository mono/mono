using System;
using System.Runtime.InteropServices;

[Obsolete ("Use Errno", true)]
public enum Error {
	EROFS,
	ERANGE = TestConst.C,
	EANOTHER = ERANGE,
}

public enum Error_2 {
	[Obsolete ("Use A", true)]
	ERANGE,
	[Obsolete ("Use B", true)]
	EANOTHER = ERANGE,
}


[Obsolete ("Use Native.SignalHandler", true)]
public delegate void SignalHandler (int signal);

[Obsolete ("Use Errno", true)]
public sealed class UnixMarshal {

	public static readonly SignalHandler SIG_DFL = new SignalHandler(Default);

	static UnixMarshal ()
	{
		Stdlib s = new Stdlib ();
	}
	
	private static void Default (int signal)
	{
	}

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
	
	enum E {
		val1 = TestConst.C
	}
	
	internal const string LIBC = "msvcrt.dll";
	[DllImport (LIBC)]
	public static extern IntPtr signal (int signum, IntPtr handler);
}

class TestConst {
	[Obsolete ("B", true)]
	public const int C = 3;
}


[Obsolete ("Use Native.Stdlib", true)]
public class XX {
	private static readonly SignalHandler[] registered_signals;
}

[Obsolete ("Use Native.Pollfd", true)]
public struct Pollfd {
}

[Obsolete ("Use Native.Syscall", true)]
public class Syscall : XX {
	public static int poll (Pollfd [] fds, uint nfds, int timeout) {
		return -1;
	}
}


[Obsolete ("test me", true)]
partial struct PS
{
}

partial struct PS
{
	[Obsolete ("Use Errno", true)]
	public static void GetDescription (Error e) {}
}


[Obsolete ("Replaced by direct enum type casts to/from GLib.Value", true)]
public class EnumWrapper {
	public EnumWrapper (int val)
	{
	}
}	

public struct Value 
{
	[Obsolete ("Replaced by Enum cast", true)]
	public static explicit operator EnumWrapper (Value val)
	{
		return new EnumWrapper (334455);
	}
}

class Test {
	public static void Main () {
	}
}
