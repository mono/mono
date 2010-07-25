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
	struct UnzipFileInfo
	{
	    IntPtr version;              /* version made by                 2 bytes */
	    IntPtr version_needed;       /* version needed to extract       2 bytes */
	    IntPtr flag;                 /* general purpose bit flag        2 bytes */
	    IntPtr compression_method;   /* compression method              2 bytes */
	    IntPtr dosDate;              /* last mod file date in Dos fmt   4 bytes */
	    IntPtr crc;                  /* crc-32                          4 bytes */
	    IntPtr compressed_size;      /* compressed size                 4 bytes */
	    IntPtr uncompressed_size;    /* uncompressed size               4 bytes */
	    IntPtr size_filename;        /* filename length                 2 bytes */
	    IntPtr size_file_extra;      /* extra field length              2 bytes */
	    IntPtr size_file_comment;    /* file comment length             2 bytes */
	
	    IntPtr disk_num_start;       /* disk number start               2 bytes */
	    IntPtr internal_fa;          /* internal file attributes        2 bytes */
	    IntPtr external_fa;          /* external file attributes        4 bytes */
	
	    ZipTime tmu_date;
	    
	    public ulong VersionNeeded {
	    	get { return (ulong)version_needed.ToInt64 (); }
	    	set { version_needed = new IntPtr ((int)value); }
	    }
	    
	    public ulong Version {
	    	get { return (ulong)version.ToInt64 (); }
	    	set { version = new IntPtr ((int)value); }
	    }
	    
	    public ulong UncompressedSize {
	    	get { return (ulong)uncompressed_size.ToInt64 (); }
			set { uncompressed_size = new IntPtr ((int)value); }
	    }
	    
	    public ZipTime TmuDate {
	    	get { return tmu_date; }
	    	set { tmu_date = value; }
	    }
	    
	    public ulong SizeFilename {
	    	get { return (ulong)size_filename.ToInt64 (); }
	    	set { size_filename = new IntPtr ((int)value); }
	    }
	    
	    public ulong SizeFileExtra {
	    	get { return (ulong)size_file_extra.ToInt64 (); }
	    	set { size_file_extra = new IntPtr ((int)value); }
	    }
	    
	    public ulong SizeFileComment {
	    	get {
	    		return (ulong)size_file_comment.ToInt64 ();
	    	}
	    	set {
	    		size_file_comment = new IntPtr ((int)value);
	    	}
	    }
	    
	    public ulong InternalFa {
	    	get { return (ulong)internal_fa.ToInt64 (); }
	    	set { internal_fa = new IntPtr ((int)value); }
	    }
	    
	    public ulong Flag {
	    	get { return (ulong)flag.ToInt64 (); }
	    	set { flag = new IntPtr ((int)value); }
	    }
	    
	    public ulong ExternalFa {
	    	get { return (ulong)external_fa.ToInt64 (); }
	    	set { external_fa = new IntPtr ((int)value); }
	    }
	    
	    public ulong DosDate {
	    	get { return (ulong)dosDate.ToInt64 (); }
	    	set { dosDate = new IntPtr ((int)value); }
	    }
	    
	    public ulong DiskNumStart {
	    	get { return (ulong)disk_num_start.ToInt64 (); }
	    	set { disk_num_start = new IntPtr ((int)value); }
	    }
	    
	    public ulong Crc {
	    	get { return (ulong)crc.ToInt64 (); }
	    	set { crc = new IntPtr ((int)value); }
	    }
	    
	    public ulong CompressionMethod {
	    	get { return (ulong)compression_method.ToInt64 (); }
	    	set { compression_method = new IntPtr ((int)value); }
	    }
	    
	    public ulong CompressedSize {
	    	get { return (ulong)compressed_size.ToInt64 (); }
	    	set { compressed_size = new IntPtr ((int)value); }
	    }
	}
}
