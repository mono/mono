// Compiler options: -unsafe

using System.Runtime.InteropServices;

public unsafe struct A
{
	fixed byte fileid [DbConst.DB_FILE_ID_LEN];
}

public static class DbConst
{
	public const int DB_FILE_ID_LEN = 20;
}

[StructLayout(LayoutKind.Sequential, Size=92)]
internal unsafe struct hci_dev_info {
	public fixed sbyte name[8];
	private fixed byte bdaddr[6];
	hci_dev_info* foo;
}

class M
{
	public static void Main ()
	{
	}
}
