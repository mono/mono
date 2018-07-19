// FileInfo.cs created with MonoDevelop
// User: alan at 14:47 13/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	[StructLayout (LayoutKind.Sequential)]
	struct UnzipFileInfo32
	{
		public uint version;              /* version made by                 2 bytes */
		public uint version_needed;       /* version needed to extract       2 bytes */
		public uint flag;                 /* general purpose bit flag        2 bytes */
		public uint compression_method;   /* compression method              2 bytes */
		public uint dosDate;              /* last mod file date in Dos fmt   4 bytes */
		public uint crc;                  /* crc-32                          4 bytes */
		public uint compressed_size;      /* compressed size                 4 bytes */
		public uint uncompressed_size;    /* uncompressed size               4 bytes */
		public uint size_filename;        /* filename length                 2 bytes */
		public uint size_file_extra;      /* extra field length              2 bytes */
		public uint size_file_comment;    /* file comment length             2 bytes */
	
		public uint disk_num_start;       /* disk number start               2 bytes */
		public uint internal_fa;          /* internal file attributes        2 bytes */
		public uint external_fa;          /* external file attributes        4 bytes */
	
	    ZipTime tmu_date;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct UnzipFileInfo64
	{
		public ulong version;              /* version made by                 2 bytes */
		public ulong version_needed;       /* version needed to extract       2 bytes */
		public ulong flag;                 /* general purpose bit flag        2 bytes */
		public ulong compression_method;   /* compression method              2 bytes */
		public ulong dosDate;              /* last mod file date in Dos fmt   4 bytes */
		public ulong crc;                  /* crc-32                          4 bytes */
		public ulong compressed_size;      /* compressed size                 4 bytes */
		public ulong uncompressed_size;    /* uncompressed size               4 bytes */
		public ulong size_filename;        /* filename length                 2 bytes */
		public ulong size_file_extra;      /* extra field length              2 bytes */
		public ulong size_file_comment;    /* file comment length             2 bytes */

		public ulong disk_num_start;       /* disk number start               2 bytes */
		public ulong internal_fa;          /* internal file attributes        2 bytes */
		public ulong external_fa;          /* external file attributes        4 bytes */

		ZipTime tmu_date;
	}
}
