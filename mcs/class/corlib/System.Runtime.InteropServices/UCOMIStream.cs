//
// System.Runtime.InteropServices.UCOMIStream.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIStream
	{
		void Clone (ref UCOMIStream ppstm);
		void Commit (int grfCommitFlags);
		void CopyTo (UCOMIStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
		void LockRegion (long libOffset, long cb, int dwLockType);
		void Read (byte[] pv, int cb, IntPtr pcbRead);
		void Revert ();
		void Seek (long dlibMove, int dwOrigin, IntPtr plibNewPosition);
		void SetSize (long libNewSize);
		void Stat (ref STATSTG pstatstg, int grfStatFlag);
		void UnlockRegion (long libOffset, long cb, int dwLockType);
		void Write (byte[] pv, int cb, IntPtr pcbWritten);
	}
}
