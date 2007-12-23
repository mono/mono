// Compiler options: -unsafe

public unsafe struct A
{
	fixed byte fileid[DbConst.DB_FILE_ID_LEN];
}

public static class DbConst
{
	public const int DB_FILE_ID_LEN = 20;
}

class M
{
	public static void Main ()
	{
	}
}
