/*---------------------------------------------------------------------

		 XX		   X		    XXX
				  XX		     XX
		XXX	XX XXX	 XXXXX		     XX
		 XX	XXX XX	  XX		     XX
		 XX	XX  XX	  XX	  XXXXX	     XX
		 XX	XX  XX	  XX XX	 XX    X     XX
		XXXX	XX  XX	   XXX	 XXXXXXX    XXXX
					 XX
					  XXXXX

Copyright (c) 2001 Intel Corporation.  All Rights Reserved.

CREATED: August	22, 2001
OWNER: Scott D Smith, Joel Marcey
VERSION: 1.0
---------------------------------------------------------------------*/
	    
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Collections;

namespace System.PAL
{
	/// <summary>
	///	Class that implements IOperatingSystem,	providing the requested	functionality through calls into APIs available	in Linux 
	/// </summary>
	internal class OpSys
	{
		private Hashtable _environment = null;

		//----------------------------------
		//	       Class Constants
		//----------------------------------
		private	const int EOF =	-1; // TODO: Linux: Is this true?
	

		// For StdInputStream and StdOutputStream
		private	const int STDOUT = 1; // TODO: Linux: Is this true?
		private	const int STDIN	= 0; //	TODO: Linux: Is	this true?


		//----------------------------------
		//		Class Fields
		//----------------------------------

		//----------------------------------
		//		Class Constructor
		//----------------------------------
		public OpSys()
		{
		}


		//-------------------------------------------------
		//		Environment Services 
		//-------------------------------------------------

		public string NewLineSequence
		{
			get
			{
				return "\n";
			}
		}

		public char DirectorySeparator
		{
			get
			{
				return '/';
			}
		}

		public char AltDirectorySeparator
		{
			get
			{
				return '\\';
			}
		}

		public char VolumeSeparator
		{
			get
			{
				return '/';
			}
		}

		public char PathSeparator
		{
			get
			{
				return ':';
			}
		}

		public char[] InvalidPathChars
		{
			get
			{
				return new char[] { '/', '\0' };
			}
		}

		public char[] DirVolSeparatorChars
		{
			get
			{
				return new char[] { this.DirectorySeparator, this.AltDirectorySeparator, this.VolumeSeparator};
			}
		}
		public char ExtensionCharacter
		{
			get
			{
				return '.';
			}
		}

		public string GetEnvironmentVariable(string eVar)
		{
			return EnvironmentVariables[eVar].ToString();
		}

		public IDictionary EnvironmentVariables
		{
			get
			{
				if (_environment == null) {
					IntPtr pp = _getEnviron(); // pointer to	an array of char*
					_environment = new Hashtable();
			
					if (pp != IntPtr.Zero) {
						IntPtr p;
						bool done = false;
						char[] delimiter = { '=' };
				
						while (!done) 
						{
							p = Marshal.ReadIntPtr(pp);
							if (p != IntPtr.Zero) 
							{
								string str = Marshal.PtrToStringAuto(p);
								string[] ar = str.Split(delimiter, 2);
								switch(ar.Length) 
								{
									case 1:
										_environment.Add(ar[0], "");
										break;
									case 2:
										_environment.Add(ar[0], ar[1]);
										break;
									default:
										System.Diagnostics.Debug.Assert(false);	// this	shouldn't happen
										break;
								}
							} 
							else 
							{
								done = true;
							}
						}
					} 
				}			
				return _environment;
			}
		}

		public string CommandLine
		{
			get
			{
				string path = Path.Combine(Path.Combine("/proc", _getPid().ToString()), "cmdline");
				StreamReader stream = File.OpenText(path);
				string res = stream.ReadToEnd();
				stream.Close();
				return res;
			}
		}

		public string MachineName
		{
			get
			{
				return GetEnvironmentVariable("HOSTNAME");
			}
		}

		public OperatingSystem OSVersion
		{
			get
			{
				return null;
			}
		}

		// System.Path services

		public string ChangeExtension(string path, string extension)
		{
			System.Diagnostics.Debug.WriteLine("Linux:ChangeExtension(System.String, System.String): Stub Method");
			return null;
		}

		public string GetExtension(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetExtension(System.String): Stub Method");
			return null;
		}

		public string GetFileName(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetFileName(System.String): Stub Method");
			return null;
		}
	
		public string GetFileNameWithoutExtension(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetFileNameWithoutExtension(System.String): Stub Method");
			return null;
		}

		public string GetFullPath(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetFullPath(System.String): Stub Method");
			return null;
		}

		public string GetPathRoot(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetPathRoot(System.String): Stub Method");
			return null;

		}
	
		public string GetTempFileName()
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetTempFileName(): Stub Method");
			return null;
		}
	
		public string GetTempPath()
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetTempPath(): Stub Method");
			return null;
		}

		public bool HasExtension(string	path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:HasExtension(System.String): Stub Method");
			return false;
		}

		public bool IsPathRooted(string	path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:IsPathRooted(System.String): Stub Method");
			return false;
		}



		// System.Directory services

		public void DeleteDirectory(string path, bool recursive)
		{
			System.Diagnostics.Debug.WriteLine("Linux:DeleteDirectory(System.String, System.Boolean): Stub Method");
		}

		public bool ExistsDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:ExistsDirectory(System.String): Stub Method");
			return false;
		}

		public DateTime	GetCreationTimeDirectory(string	path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetCreationTimeDirectory(System.String): Stub	Method");
			return new DateTime(0);
		}

		public string GetCurrentDirectory()
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetCurrentDirectory(): Stub Method");
			return null;
		}

		public string[]	GetDirectories(string path, string searchPattern)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetDirectories(System.String,System.String): Stub Method");
			return null;
		}

		public string[]	GetFiles(string	path, string searchPattern)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetFiles(System.String, System.String): Stub Method");
			return null;
		}

		public string[]	GetFileSystemEntries(string path, string searchPattern)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetFileSystemEntries(System.String, System.String): Stub Method");
			return null;
		}

		public DateTime	GetLastAccessTimeDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetLastAccessTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public DateTime	GetLastWriteTimeDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetLastWriteTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public void MoveDirectory(string sourceDirName,	string destDirName)
		{
			System.Diagnostics.Debug.WriteLine("Linux:MoveDirectory(System.String, System.String): Stub Method");
		}

		public void SetCreationTimeDirectory(string path, DateTime creationTime)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetCreationTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		public void SetCurrentDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetCurrentDirectory(System.String): Stub Method");
		}

		public void SetLastAccessTimeDirectory(string path, DateTime lastAccessTime)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetLastAccessTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		public void SetLastWriteTimeDirectory(string path, DateTime lastWriteTime)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetLastWriteTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		//-----------------------------------
		//		I/O Services
		//-----------------------------------

		// For StdInputStream
		public int ReadStdInput(byte[] buffer, int offset, int count)
		{
			return ReadFile(new IntPtr(STDIN), buffer, offset, count);
		}

		// For StdOutputStream
		public void FlushStdOutput(byte[] byteBuf)
		{
			FlushFile(new IntPtr(STDOUT), byteBuf);
		}

		public unsafe int ReadFile(IntPtr handle, byte[] buffer, int offset, int count)
		{
			int res;

			fixed (void *p = &buffer [offset]) {
				res = _read(handle, p, count);
			}
			
			return res;
		}

		public unsafe int WriteFile(IntPtr handle, byte[] buffer, int offset, int count)
		{
			int res;

			fixed (void *p = &buffer [offset]) {
				res = _write(handle, p, count);
			}

			return res;
		}

		public int SetLengthFile(IntPtr handle, long length)
		{
			return _ftruncate (handle, (int)length);
		}

		public void FlushFile(IntPtr handle, byte[] byteBuf)
		{
			WriteFile(handle, byteBuf, 0, byteBuf.Length);
		}

		public IntPtr OpenFile(string path, FileMode mode, FileAccess access, FileShare	share)
		{
			int flags = _getUnixFlags (mode, access);

			return _open (path, flags, 0x1a4);
		}
	    
		public void CloseFile(IntPtr handle)
		{
			_close (handle);
		}
	
		public long SeekFile(IntPtr handle, long offset, SeekOrigin origin)
		{
			switch (origin) {
				case SeekOrigin.End:
					return _lseek (handle, (int)offset, SEEK_END);
				case SeekOrigin.Current:
					return _lseek (handle, (int)offset, SEEK_CUR);
				default:
					return _lseek (handle, (int)offset, SEEK_SET);
			}
			
		}
	
		public IntPtr CreateFile(string	path, FileMode mode, FileAccess	access,	FileShare share)
		{
			return OpenFile(path, FileMode.CreateNew, access, share);
		}
	
		public void DeleteFile(string path)
		{
			_unlink(path);
		}
	
		public bool ExistsFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:ExistsFile(System.String): Stub Method");
			return false;
		}
	
		public DateTime	GetCreationTimeFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetCreationTimeFile(System.String): Stub Method");
			return new DateTime(0);
		}
	
		public DateTime	GetLastAccessTimeFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetLastAccessTimeFile(System.String):	Stub Method");
			return new DateTime(0);
		}
	
		public DateTime	GetLastWriteTimeFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Linux:GetLastWriteFile(System.String): Stub	Method");
			return new DateTime(0);
		}
	
		public void SetCreationTimeFile(string path, DateTime creationTime)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetCreationTimeFile(System.String, System.DateTime): Stub Method");
		}
	
		public void SetLastAccessTimeFile(string path, DateTime	lastAccessTime)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetLastAccessTimeFile(System.String, System.DateTime): Stub Method");
		}
	
		public void SetLastWriteTimeFile(string	path, DateTime lastWriteTime)
		{
			System.Diagnostics.Debug.WriteLine("Linux:SetCLastWriteTimeFile(System.String, System.DateTime): Stub Method");
		}


		public long FileLength(string path)
		{
			return 0;
		}

		public long FileLength(IntPtr handle)
		{
			return 0;
		}

		// Private implementation details
		[DllImport("monowrapper", EntryPoint="mono_wrapper_environ", CharSet=CharSet.Ansi)]
		private unsafe static extern IntPtr _getEnviron();

		[DllImport("libc", EntryPoint="getpid")]
		private unsafe static extern int _getPid();

		[DllImport("libc", EntryPoint="read", CharSet=CharSet.Ansi)]
		private unsafe static extern int _read(IntPtr fd, void * buf, int count);

		[DllImport("libc", EntryPoint="write", CharSet=CharSet.Ansi)]
		private unsafe static extern int _write(IntPtr fd, void * buf, int count);

		[DllImport("libc", EntryPoint="ftruncate", CharSet=CharSet.Ansi)]
		private unsafe static extern int _ftruncate(IntPtr fd, int count);

		[DllImport("libc", EntryPoint="lseek", CharSet=CharSet.Ansi)]
		private unsafe static extern int _lseek(IntPtr fd, int offset, int whence);

		[DllImport("libc", EntryPoint="fflush", CharSet=CharSet.Ansi)]
		private unsafe static extern int _fflush(IntPtr fd);

		[DllImport("libc", EntryPoint="close", CharSet=CharSet.Ansi)]
		private unsafe static extern int _close(IntPtr fd);

		[DllImport("libc", EntryPoint="open", CharSet=CharSet.Ansi)]
		private unsafe static extern IntPtr _open(string path, int flags, int mode);

		[DllImport("libc", EntryPoint="unlink", CharSet=CharSet.Ansi)]
		private unsafe static extern int _unlink(string path);

		private const int O_RDONLY             = 0x00000000;
		private const int O_WRONLY             = 0x00000001;
		private const int O_RDWR               = 0x00000002;
		private const int O_CREAT              = 0x00000040;
		private const int O_EXCL               = 0x00000080;
		private const int O_TRUNC              = 0x00000200;
		private const int O_APPEND             = 0x00000400;

		private const int SEEK_SET             = 0;
		private const int SEEK_CUR             = 1;
		private const int SEEK_END             = 2;

		private int _getUnixFlags (FileMode mode, FileAccess access)
		{
			int flags = 0;
			switch (access) {
				case FileAccess.Read:
					flags = O_RDONLY;
					break;
				case FileAccess.Write:
					flags = O_WRONLY;
					break;
				case FileAccess.ReadWrite:
					flags = O_RDWR;
					break;
			}

			switch (mode) {
				case FileMode.Append:
					flags |= O_APPEND;
					break;
				case FileMode.Create:
					flags |= O_CREAT;
					break;
				case FileMode.CreateNew:
					flags |= O_CREAT | O_EXCL;
					break;
				case FileMode.Open:
					break;
				case FileMode.OpenOrCreate:
					flags |= O_CREAT;
					break;
				case FileMode.Truncate:
					flags |= O_TRUNC;
					break;
			}

			return flags;
		}

		[ DllImport("libm", EntryPoint="acos") ]
		public extern static double Acos(double d);

		[ DllImport("libm", EntryPoint="asin") ]
		public extern static double Asin(double d);

		[ DllImport("libm", EntryPoint="atan") ]
		public extern static double Atan(double d);

		[ DllImport("libm", EntryPoint="atan2") ]
		public extern static double Atan2(double y, double x);

		[ DllImport("libm", EntryPoint="cos") ]
		public extern static double Cos(double d);

		[ DllImport("libm", EntryPoint="cosh") ]
		public extern static double Cosh(double d);

		[ DllImport("libm", EntryPoint="exp") ]
		public extern static double Exp(double d);

		[ DllImport("libm", EntryPoint="log") ]
		public extern static double Log(double d);

		[ DllImport("libm", EntryPoint="log10") ]
		public extern static double Log10(double d);

		[ DllImport("libm", EntryPoint="pow") ]
		public extern static double Pow(double x, double y);

		[ DllImport("libm", EntryPoint="sin") ]
		public extern static double Sin(double d);

		[ DllImport("libm", EntryPoint="sinh") ]
		public extern static double Sinh(double d);

		[ DllImport("libm", EntryPoint="sqrt") ]
		public extern static double Sqrt(double d);

		[ DllImport("libm", EntryPoint="tan") ]
		public extern static double Tan(double d);

		[ DllImport("libm", EntryPoint="tanh") ]
		public extern static double Tanh(double d);


	}
}
