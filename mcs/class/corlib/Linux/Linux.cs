
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
using System.Reflection;
using System.Runtime.CompilerServices;

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
		private IntPtr Stdin;
		private IntPtr Stdout;
		private IntPtr Stderr;

		//----------------------------------
		//		Class Fields
		//----------------------------------

		//----------------------------------
		//		Class Constructor
		//----------------------------------
		public OpSys()
		{
			Stdin=GetStdHandle(0);
			Stdout=GetStdHandle(1);
			Stderr=GetStdHandle(2);
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
				return new char[] { '\0' };
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
										//System.Diagnostics/.Debug.Assert(false);	// this	shouldn't happen
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
			//System.Diagnostics/.Debug.WriteLine("Linux:ChangeExtension(System.String, System.String): Stub Method");
			return null;
		}

		public string GetExtension(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetExtension(System.String): Stub Method");
			return null;
		}

		public string GetFileName(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetFileName(System.String): Stub Method");
			return null;
		}
	
		public string GetFileNameWithoutExtension(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetFileNameWithoutExtension(System.String): Stub Method");
			return null;
		}

		public string GetFullPath(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetFullPath(System.String): Stub Method");
			return null;
		}

		public string GetPathRoot(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetPathRoot(System.String): Stub Method");
			return null;

		}
	
		public string GetTempFileName()
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetTempFileName(): Stub Method");
			return null;
		}
	
		public string GetTempPath()
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetTempPath(): Stub Method");
			return null;
		}

		public bool HasExtension(string	path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:HasExtension(System.String): Stub Method");
			return false;
		}

		public bool IsPathRooted(string	path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:IsPathRooted(System.String): Stub Method");
			return false;
		}



		// System.Directory services

		public void DeleteDirectory(string path, bool recursive)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:DeleteDirectory(System.String, System.Boolean): Stub Method");
		}

		public bool ExistsDirectory(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:ExistsDirectory(System.String): Stub Method");
			return false;
		}

		public DateTime	GetCreationTimeDirectory(string	path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetCreationTimeDirectory(System.String): Stub	Method");
			return new DateTime(0);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public extern string GetCurrentDirectory();

		public string[]	GetDirectories(string path, string searchPattern)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetDirectories(System.String,System.String): Stub Method");
			return null;
		}

		public string[]	GetFiles(string	path, string searchPattern)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetFiles(System.String, System.String): Stub Method");
			return null;
		}

		public string[]	GetFileSystemEntries(string path, string searchPattern)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetFileSystemEntries(System.String, System.String): Stub Method");
			return null;
		}

		public DateTime	GetLastAccessTimeDirectory(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetLastAccessTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public DateTime	GetLastWriteTimeDirectory(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:GetLastWriteTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public void MoveDirectory(string sourceDirName,	string destDirName)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:MoveDirectory(System.String, System.String): Stub Method");
		}

		public void SetCreationTimeDirectory(string path, DateTime creationTime)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:SetCreationTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		public void SetCurrentDirectory(string path)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:SetCurrentDirectory(System.String): Stub Method");
		}

		public void SetLastAccessTimeDirectory(string path, DateTime lastAccessTime)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:SetLastAccessTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		public void SetLastWriteTimeDirectory(string path, DateTime lastWriteTime)
		{
			//System.Diagnostics/.Debug.WriteLine("Linux:SetLastWriteTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		//-----------------------------------
		//		I/O Services
		//-----------------------------------

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		private extern IntPtr GetStdHandle(int fd);

		public IntPtr StdinHandle {
			get {
				return(Stdin);
			}
		}

		public IntPtr StdoutHandle {
			get {
				return(Stdout);
			}
		}

		public IntPtr StderrHandle {
			get {
				return(Stderr);
			}
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public extern void DeleteFile(string path);
	
		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public extern bool ExistsFile(string path);

		/* The long time parameters in GetFileTime and
		 * SetFileTime correspond to Windows file times (ticks
		 * from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		private extern static bool GetFileTime(IntPtr handle, out long creat, out long lastaccess, out long lastwrite);

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		private extern static bool SetFileTime(IntPtr handle, long creat, long lastaccess, long lastwrite);
	
		public DateTime	GetCreationTimeFile(string path)
		{
			long creat, lastaccess, lastwrite;
			bool ret;
			FileStream s = new FileStream(path, FileMode.Open, FileAccess.Read);
			
			ret=GetFileTime(s.Handle, out creat, out lastaccess, out lastwrite);
			s.Close();
			
			return DateTime.FromFileTime(creat);
		}
	
		public DateTime	GetLastAccessTimeFile(string path)
		{
			long creat, lastaccess, lastwrite;
			bool ret;
			FileStream s = new FileStream(path, FileMode.Open, FileAccess.Read);
			
			ret=GetFileTime(s.Handle, out creat, out lastaccess, out lastwrite);
			s.Close();
			
			return DateTime.FromFileTime(lastaccess);
		}
	
		public DateTime	GetLastWriteTimeFile(string path)
		{
			long creat, lastaccess, lastwrite;
			bool ret;
			FileStream s = new FileStream(path, FileMode.Open, FileAccess.Read);
			
			ret=GetFileTime(s.Handle, out creat, out lastaccess, out lastwrite);
			s.Close();
			
			return DateTime.FromFileTime(lastwrite);
		}
	
		public void SetCreationTimeFile(string path, DateTime creationTime)
		{
			long creat, lastaccess, lastwrite;
			bool ret;
			FileStream s = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
			
			// Get the existing times first
			ret=GetFileTime(s.Handle, out creat, out lastaccess, out lastwrite);

			creat=creationTime.ToFileTime();
			
			ret=SetFileTime(s.Handle, creat, lastaccess, lastwrite);
			s.Close();
		}
	
		public void SetLastAccessTimeFile(string path, DateTime	lastAccessTime)
		{
			long creat, lastaccess, lastwrite;
			bool ret;
			FileStream s = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
			
			// Get the existing times first
			ret=GetFileTime(s.Handle, out creat, out lastaccess, out lastwrite);

			lastaccess=lastAccessTime.ToFileTime();
			
			ret=SetFileTime(s.Handle, creat, lastaccess, lastwrite);
			s.Close();
		}
	
		public void SetLastWriteTimeFile(string	path, DateTime lastWriteTime)
		{
			long creat, lastaccess, lastwrite;
			bool ret;
			FileStream s = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
			
			// Get the existing times first
			ret=GetFileTime(s.Handle, out creat, out lastaccess, out lastwrite);

			lastwrite=lastWriteTime.ToFileTime();
			
			ret=SetFileTime(s.Handle, creat, lastaccess, lastwrite);
			s.Close();
		}


		public long FileLength(string path)
		{
			return 0;
		}

		// Private implementation details
		[DllImport("monowrapper", EntryPoint="mono_wrapper_environ", CharSet=CharSet.Ansi)]
		private unsafe static extern IntPtr _getEnviron();

		[DllImport("libc", EntryPoint="getpid")]
		private unsafe static extern int _getPid();
	}
}
