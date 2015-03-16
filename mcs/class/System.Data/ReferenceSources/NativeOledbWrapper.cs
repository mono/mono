using System;

class NativeOledbWrapper
{
	public static int SizeOfPROPVARIANT {
		get { throw new NotSupportedException (msg); }
	}
	
	const string msg = "It is native method used by Microsoft System.Data.OleDb implementation that Mono or non-Windows platform does not support.";
	
	public static int ITransactionAbort (IntPtr handle)
	{
		throw new NotSupportedException (msg);
	}
	
	public static int ITransactionCommit (IntPtr handle)
	{
		throw new NotSupportedException (msg);
	}
	
	public static int MemoryCopy (IntPtr dst, IntPtr src, int bytes)
	{
		throw new NotSupportedException (msg);
	}
	
	public static bool MemoryCompare (IntPtr dst, IntPtr src, int bytes)
	{
		throw new NotSupportedException (msg);
	}
	
	public static IntPtr IChapteredRowsetReleaseChapter (IntPtr handle, IntPtr chapter)
	{
		throw new NotSupportedException (msg);
	}
}
