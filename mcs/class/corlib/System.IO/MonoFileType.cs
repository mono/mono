//
// System.IO.MonoFileType.cs:  enum for GetFileType return
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

namespace System.IO
{
	internal enum MonoFileType {
		Unknown=0x0000,
		Disk=0x0001,
		Char=0x0002,
		Pipe=0x0003,
		Remote=0x8000,
	}
}

