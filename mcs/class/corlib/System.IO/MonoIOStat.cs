//
// System.IO.MonoIOStat.cs: Idealized structure for file information.
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

using System;

namespace System.IO
{
	internal struct MonoIOStat {
		public string Name;
		public FileAttributes Attributes;
		public long Length;
		public long CreationTime;
		public long LastAccessTime;
		public long LastWriteTime;
	}
}
