using System.Runtime.InteropServices;

public struct CONNECTDATA
{
	[MarshalAs (10)]
	public object pUnk;
	[MarshalAs (UnmanagedType.BStr)]
	public int dwCookie;
}

public class C
{
    public static void Main () {}
}
