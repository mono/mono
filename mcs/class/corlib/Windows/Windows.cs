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

CREATED: August	15, 2001
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
	///	Class that implements IOperatingSystem,	providing the requested	functionality through calls into APIs available	in Windows (this class is a work in progress). 
	/// </summary>
	internal class OpSys
	{
	
		//OS Calls (P/Invoke)
	
	
		// For StdInputStream
		[DllImport("msvcrt.dll")]
		private	static extern int _read(int handle, out	byte buffer, uint count);
		//private static extern	int _read(int handle, byte[] buffer, uint count);


		// For StdOutputStream
		[DllImport("msvcrt.dll")]
		private	static extern int _write(int handle, ref byte buffer, uint count);
		//private static extern	int _write(int handle, byte[] buffer, uint count);
	

		/*DWORD	GetFullPathName(
		    LPCTSTR lpFileName,	 // file name
		    DWORD nBufferLength, // size of path buffer
		    LPTSTR lpBuffer,	 // path buffer
		    LPTSTR *lpFilePart	 // address of file name in path
		    );*/
	
		[DllImport("kernel32.dll")]
		private	static extern uint GetFullPathName(string path,	uint bufLength,	System.Text.StringBuilder buffer, ref System.Text.StringBuilder	fNameAddr);
	
		/*UINT GetTempFileName(
		    LPCTSTR lpPathName,	     //	directory name
		    LPCTSTR lpPrefixString,  //	file name prefix
		    UINT uUnique,	     //	integer
		    LPTSTR lpTempFileName    //	file name buffer
		    );*/
	
		[DllImport("kernel32.dll")]
		private	static extern uint GetTempFileName(string path,	string prefix, uint unique, System.Text.StringBuilder buffer);

		/*DWORD	GetTempPath(
		    DWORD nBufferLength,  // size of buffer
		    LPTSTR lpBuffer	  // path buffer
		    );*/
	
		[DllImport("kernel32.dll")]
		private	static extern int GetTempPath(int bufferLength,	System.Text.StringBuilder buffer);



	
		// Class Constants
	
		private	const int EOF =	-1; // In stdio.h, EOF is defined as -1
	


		// For StdInputStream and StdOutputStream
	
		private	const int STDOUT = 1; // In stdio.h, the handle	to standard out	is 1
		private	const int STDIN	= 0; //	In stdio.h, the	standard input handle is defined as 0. Will this always	be true?

	
		// Class Fields
	
		private	static bool isNextCharacterResidualNewline = false;
		private	static byte residualNewlineByte	= 0;


	
		// Class Constructor
	
		public OpSys()
		{
		}

	
		//  System.Environment Services	
	
		public string NewLineSequence
		{
			get
			{
				return "\r\n";
			}
		}

		public char DirectorySeparator
		{
			get
			{
				return (char) 0x005C; // This is a \
			}
		}

		public char AltDirectorySeparator
		{
			get
			{
				return (char) 0x002F; // This is a /
			}
		}

		public char VolumeSeparator
		{
			get
			{
				return (char) 0x003A; // This is a :
			}
		}

		public char PathSeparator
		{
			get
			{
				return (char) 0x003B; // This is a ;
			}
		}

		public char[] InvalidPathChars
		{
			get
			{
				char[] ipc = {'"', '<',	'>', '|', '\0'};
				return ipc;
			}
		}

		public char[] DirVolSeparatorChars
		{
			get
			{
				char[] dsc = new char[]	{this.DirectorySeparator, this.AltDirectorySeparator, this.VolumeSeparator};
				return dsc;
			}
		}
		public char ExtensionCharacter
		{
			get
			{
				return (char) 0x002E; // This is a .
			}
		}

		public string GetEnvironmentVariable(string eVar)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetEnvironmentVariable(System.String): Stub	Method");
			// Call	Windows	API to get environment variable;
			return null;
		}

		public string CommandLine
		{
			get
			{
				return null;
			}
		}

		public IDictionary EnvironmentVariables
		{
			get
			{
				return null;
			}
		}

		public string MachineName
		{
			get
			{
				return null;
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
			//System.Diagnostics.Debug.WriteLine("Windows:ChangeExtension(System.String, System.String): Stub Method");
			if (path == null || path.Equals(string.Empty))
			{
				return path;
			}
			if (!this.HasExtension(path) &&	extension != null)
			{
				return string.Concat(path, extension);
			}
			string pathNoExt = path.Substring(0, path.LastIndexOf(this.ExtensionCharacter));
			// If extension	is null, concat	replaces it with string.Empty
			return string.Concat(pathNoExt,	extension);
		}

		public string GetExtension(string path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetExtension(System.String): Stub	Method");
			if (path == null)
			{
				return path;
			}
			if (!this.HasExtension(path))
			{
				return string.Empty;
			}
			// It has an extension
			return path.Substring(path.LastIndexOf(this.ExtensionCharacter));
		}

		public string GetFileName(string path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetFileName(System.String): Stub Method");
			if (path == null)
			{
				return null;
			}
			int dvLast = path.LastIndexOfAny(this.DirVolSeparatorChars);
			if (dvLast == -1)
			{
				return path;
			}
			return dvLast +	1 == path.Length ? string.Empty	: path.Substring(dvLast	+ 1);
		}
	
		public long FileLength(string path)
		{
			return 0;
		}

		public long FileLength(IntPtr handle)
		{
			return 0;
		}

		public string GetFileNameWithoutExtension(string path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetFileNameWithoutExtension(System.String): Stub Method");
			return this.ChangeExtension(this.GetFileName(path), null);
		}

		// TODO: Windows: GetFullPath: Verify logic here. This seems TOO simplistic
		public string GetFullPath(string path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetFullPath(System.String): Stub Method");
			if (path == null)
			{
				return null;
			}
			// TODO: GetFullPath: What should the size of the buffer be?
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(256);
			// Just	temporary to pass in as	final parameter
			System.Text.StringBuilder temp = new System.Text.StringBuilder();
			// TODO: GetFullPath: ECMA spec	says that an ArgumentException is thrown if system can not retrieve the	path. That does	not seem right.	Returning null for now
			return GetFullPathName(path, (uint) buffer.Capacity, buffer, ref temp) != 0 ? buffer.ToString()	: null;
		}

		public string GetPathRoot(string path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetPathRoot(System.String): Stub Method");
			if (path == null)
			{
				return null;
			}
			// TODO: Windows: GetPathRoot: Check logic. Assuming that if there is not dir or vol separators	in the first three characters, then relative.
			int dvFirst = path.IndexOfAny(this.DirVolSeparatorChars, 0, 3);
			if (dvFirst == -1)
			{
				return string.Empty;
			}
			if (dvFirst == 0 && path[dvFirst].Equals(this.VolumeSeparator))
			{
				return string.Empty;
			}
			return dvFirst == 1 && path[dvFirst].Equals(this.VolumeSeparator) && (path[dvFirst+1].Equals(this.DirectorySeparator) || path[dvFirst+1].Equals(this.AltDirectorySeparator)) ? path.Substring(0, dvFirst+2) : path.Substring(0,	dvFirst+1);

		}
	
		public string GetTempFileName()
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetTempFileName(): Stub Method");
			string tPath = this.GetTempPath();
			string prefix =	"tmp";
			// TODO: Windows: GetTempFileName: Remove System once our implementation of StringBuilder is done. Same	for parameter to P/Invoke
			// TODO: Windows: GetTempFileName: What	is the proper length?
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(256);
			// TODO: Windows: GetTempFileName: If an error is returned, what should	we do? Right now return	null;
			return GetTempFileName(tPath, prefix, 0, buffer) != 0 ?	buffer.ToString() : null;
		}

		public string GetTempPath()
		{
			//System.Diagnostics.Debug.WriteLine("Windows:GetTempPath(): Stub Method");
			// TODO: Windows: GetTempPath: Remove System once our implementation of	StringBuilder is done. Same for	parameter to P/Invoke
			// According to	docs, LPTSTR maps to StringBuilder for In/Out
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(256);
			// TODO: Windows: GetTempPath: What is the proper length?
			// TODO: Windows: GetTempPath: If an error is returned,	what should we do? Right now return null;
			return GetTempPath(buffer.Capacity, buffer) != 0 ? buffer.ToString() : null;
		}

		public bool HasExtension(string	path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:HasExtension(System.String): Stub	Method");
			int dvLast = path.LastIndexOfAny(this.DirVolSeparatorChars);
			int exLast = path.LastIndexOf(this.ExtensionCharacter);
			if (exLast > dvLast)
			{
				return true;
			}
			return false;
		}

		public bool IsPathRooted(string	path)
		{
			//System.Diagnostics.Debug.WriteLine("Windows:IsPathRooted(System.String): Stub	Method");
			return (this.GetPathRoot(path) == null)	|| (this.GetPathRoot(path).Equals(string.Empty)) ? false : true;
		}

       
		// System.Directory service  

		public void DeleteDirectory(string path, bool recursive)
		{
			System.Diagnostics.Debug.WriteLine("Windows:DeleteDirectory(System.String, System.Boolean): Stub Method");
		}

		public bool ExistsDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:ExistsDirectory(System.String): Stub Method");
			return false;
		}

		public DateTime	GetCreationTimeDirectory(string	path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetCreationTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public string GetCurrentDirectory()
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetCurrentDirectory(): Stub	Method");
			return null;
		}

		public string[]	GetDirectories(string path, string searchPattern)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetDirectories(System.String,System.String): Stub Method");
			return null;
		}

		public string[]	GetFiles(string	path, string searchPattern)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetFiles(System.String, System.String): Stub Method");
			return null;
		}

		public string[]	GetFileSystemEntries(string path, string searchPattern)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetFileSystemEntries(System.String,	System.String):	Stub Method");
			return null;
		}

		public DateTime	GetLastAccessTimeDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetLastAccessTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public DateTime	GetLastWriteTimeDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetLastWriteTimeDirectory(System.String): Stub Method");
			return new DateTime(0);
		}

		public void MoveDirectory(string sourceDirName,	string destDirName)
		{
			System.Diagnostics.Debug.WriteLine("Windows:MoveDirectory(System.String, System.String): Stub Method");
		}

		public void SetCreationTimeDirectory(string path, DateTime creationTime)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetCreationTimeDirectory(System.String, System.DateTime): Stub Method");
		}

		public void SetCurrentDirectory(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetCurrentDirectory(System.String):	Stub Method");
		}

		public void SetLastAccessTimeDirectory(string path, DateTime lastAccessTime)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetLastAccessTimeDirectory(System.String, System.DateTime):	Stub Method");
		}

		public void SetLastWriteTimeDirectory(string path, DateTime lastWriteTime)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetLastWriteTimeDirectory(System.String, System.DateTime): Stub Method");
		}

	
		// I/O Services
	

		public int ReadStdInput(byte[] buffer, int offset, int count)
		{
			/*
			* Note that a byte is being passed into	the native read
			* function. But	importantly is that a reference, via the out, is being passed,
			* so the the native read function pass the value back to this method. Doing it
			* this away to avoid GC	issues with the	VM as value types (byte) do not	get
			* moved	around like objects (byte[]) do. Downside is that the OS is being called
			* many times in	this loop. Should WE USE Standard C Library getChar()? Optimally, 
			* probably want	to call	the read function the second way (commented out) below...
			* but maybe not. Doing it this way could be better since we are	forced to do some
			* checks.
			*/
			byte b;
			int nativeReadRetVal;
			int charsRead =	0;

			for (int i = 0;	i < count; i++)
			{
				if (isNextCharacterResidualNewline)
				{
					isNextCharacterResidualNewline = false;
					buffer[i+offset] = residualNewlineByte;
					charsRead++;
					if (count == 1)
					{
						break;
					}
				}
				else
				{
					// Returns 0 if	at the end of the stream, -1 if	an error;
					nativeReadRetVal = _read(STDIN,	out b, 1);
					if (nativeReadRetVal ==	-1)
					{
						throw new System.IO.IOException();
					}
					// The MS implementation needs the end of stream character on a	line by
					// itself. So if end of	stream character (^Z, which is 0x0017) is reached, but
					// the previous	character was not a newline character (and since we break
					// once	a newline is hit, the only way this could be true is if	we are on
					// the first iteration of ths loop), then it is	not
					// technically the end of the stream. Is this a	correct	assumption?
					if (nativeReadRetVal ==	0 && i == 0)
					{
						// TODO: Windows: Read:	What is	the better way to do this? Should I do this? MS	implementation seems to.
						// Flush any characeters following the ^Z
						while (!IsPartOfNewlineSequence(b))
						{
							nativeReadRetVal = _read(STDIN,	out b, 1);
							if (nativeReadRetVal ==	-1)
							{
								throw new System.IO.IOException();
							}
						}
						break;
					}
					// If the next byte is part of the newline sequence, place all of the characters
					// of the newline in the the buffer, then break. The native read function
					// combines the	newline	characters into	one (e.g. CR-LF	into just LF)
					if (IsPartOfNewlineSequence(b))
					{
						char[] newlineArray = System.Environment.NewLine.ToCharArray();
						for (int j = i;	j < (i+newlineArray.Length); j++)
						{
							// If we are at	the end	of the buffer (i.e. at the limit of number
							// of characters to count) set static boolean to say next character
							// should be newline character
							// The loop is guaranteed to run to the	"else" successfully 
							// once	since we could not have	gotten to this point if
							// we were at count (the main for loop would have ended)
							if (j == count)
							{
								isNextCharacterResidualNewline = true;
								residualNewlineByte = (byte) newlineArray[j-i];
							}
							else
							{
								buffer[j+offset] = (byte) newlineArray[j-i];
								charsRead++; 
							}
			    
						}
						break;
					}
					charsRead++;
					buffer[i+offset] = b;
				}
			}
	    
			/*
			 * SECOND POSSIBLE WAY
			 * 
			// Not just using 'buffer' parameter because of	the offset. Deal with the offset 
			// below
			byte[] ba = new	byte[count];
			charsRead = _read(STDIN, ba, (uint) count);
			if (charsRead == -1)
			{
			    throw new System.IO.IOException();
			}
			Array.Copy(ba, 0, buffer, offset, charsRead);
			*/
	    
			return charsRead;	     
		}

		public void FlushStdOutput(byte[] byteBuf)
		{
			byte b;
			/*
			* Note that a byte is being passed into	the native write
			* function. Doing it
			* this away to avoid GC	issues with the	VM as value types (byte) do not	get
			* moved	around like objects (byte[]) do. Downside is that the OS is being called
			* many times in	this loop. Should WE USE Standard C Library putChar()? Optimally, 
			* probably want	to call	the write function the first way (commented out) below
			*/
			//if (_write(STDOUT, byteBuf, (uint) byteBuf.Length) ==	-1)
			for (int i = 0;	i < byteBuf.Length; i++)
			{
				b = byteBuf[i];
				if (_write(STDOUT, ref b, 1) ==	-1)
				{
					throw new System.IO.IOException();  // An I/O Exception	occurred");
				}
			}
			// TODO: Windows: Flush: Determine if a	call to	a native flush should be called	in StdOutputStream
		}
	
		public int ReadFile(IntPtr handle, byte[] buffer, int offset, int count)
		{
			System.Diagnostics.Debug.WriteLine("Windows:ReadFile(System.Int32, byte[], System.Int32, System.Int32):	Stub Method");
			return -1;
		}

		public int WriteFile(IntPtr handle, byte[] buffer, int offset, int count)
		{
			return -1;
		}

		public int SetLengthFile(IntPtr handle, long length)
		{
			return 0;		
		}

		public void FlushFile(IntPtr handle, byte[] byteBuf)
		{
			System.Diagnostics.Debug.WriteLine("Windows:FlushFile(System.Int32, byte[]): Stub Method");
		}

		public IntPtr OpenFile(string path, FileMode mode, FileAccess access, FileShare	share)
		{
			System.Diagnostics.Debug.WriteLine("Windows:OpenFile(System.String, System.IO.FileMode,	System.IO.FileAccess, System.IO.FileShare): Stub Method");
			return new IntPtr(-1);
		}
	    
		public void CloseFile(IntPtr handle)
		{
			System.Diagnostics.Debug.WriteLine("Windows:CloseFile(System.Int32): Stub Method");
		}
	
		public long SeekFile(IntPtr handle, long offset, SeekOrigin origin)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SeekFile(System.Int32, System.Int64, System.IO.SeekOrigin):	Stub Method");
			return -1;
		}
	
		public IntPtr CreateFile(string	path, FileMode mode, FileAccess	access,	FileShare share)
		{
			System.Diagnostics.Debug.WriteLine("Windows:CreateFile(System.String, System.IO.FileMode, System.IO.FileAccess,	System.IO.FileShare): Stub Method");
			return new IntPtr(-1);
		}
	
		public void DeleteFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:DeleteFile(System.String): Stub Method");
		}
	
		public bool ExistsFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:ExistsFile(System.String): Stub Method");
			return false;
		}
	
		public DateTime	GetCreationTimeFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetCreationTimeFile(System.String):	Stub Method");
			return new DateTime(0);
		}
	
		public DateTime	GetLastAccessTimeFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetLastAccessTimeFile(System.String): Stub Method");
			return new DateTime(0);
		}
	
		public DateTime	GetLastWriteTimeFile(string path)
		{
			System.Diagnostics.Debug.WriteLine("Windows:GetLastWriteFile(System.String): Stub Method");
			return new DateTime(0);
		}
	
		public void SetCreationTimeFile(string path, DateTime creationTime)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetCreationTimeFile(System.String, System.DateTime): Stub Method");
		}
	
		public void SetLastAccessTimeFile(string path, DateTime	lastAccessTime)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetLastAccessTimeFile(System.String, System.DateTime): Stub	Method");
		}
	
		public void SetLastWriteTimeFile(string	path, DateTime lastWriteTime)
		{
			System.Diagnostics.Debug.WriteLine("Windows:SetCLastWriteTimeFile(System.String, System.DateTime): Stub	Method");
		}

		// DONE: Determine if this should be in a utility class
		/// <summary>
		///     Determines if a byte is part of the newline sequence
		/// </summary>
		/// <param name="c">The byte to compare</param>
		/// <returns>A System.Boolean stating whether the byte is part of the newline</returns>
		private static bool IsPartOfNewlineSequence(byte c)
		{
			char[] newLineArray = System.Environment.NewLine.ToCharArray();
			for (int i = 0; i < newLineArray.Length; i++)
			{
				if (c == (byte) newLineArray[i]) // Or do I need Equals()
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		///     Determines if a character is a newline character
		/// </summary>
		/// <param name="c">The character to check to see if it is a newline character</param>
		/// <returns></returns>
		public static bool IsPartOfNewlineSequence(char c)
		{
			// DONE: Determine if this method can be moved into TextReader or maybe a utility class (a class with a bunch of static methods...System.IO.IOUtility)
			char[] newLineArray = System.Environment.NewLine.ToCharArray();
			for (int i = 0; i < newLineArray.Length; i++)
			{
				if (c == newLineArray[i]) // TODO: Determine if .Equals() should be used here.
				{
					return true;
				}
			}
			return false;
		}

		[ DllImport("msvcrt", EntryPoint="acos") ]
		public extern static double Acos(double d);

		[ DllImport("msvcrt", EntryPoint="asin") ]
		public extern static double Asin(double d);

		[ DllImport("msvcrt", EntryPoint="atan") ]
		public extern static double Atan(double d);

		[ DllImport("msvcrt", EntryPoint="atan2") ]
		public extern static double Atan2(double y, double x);

		[ DllImport("msvcrt", EntryPoint="cos") ]
		public extern static double Cos(double d);

		[ DllImport("msvcrt", EntryPoint="cosh") ]
		public extern static double Cosh(double d);

		[ DllImport("msvcrt", EntryPoint="exp") ]
		public extern static double Exp(double d);

		[ DllImport("msvcrt", EntryPoint="log") ]
		public extern static double Log(double d);

		[ DllImport("msvcrt", EntryPoint="log10") ]
		public extern static double Log10(double d);

		[ DllImport("msvcrt", EntryPoint="pow") ]
		public extern static double Pow(double x, double y);

		[ DllImport("msvcrt", EntryPoint="sin") ]
		public extern static double Sin(double d);

		[ DllImport("msvcrt", EntryPoint="sinh") ]
		public extern static double Sinh(double d);

		[ DllImport("msvcrt", EntryPoint="sqrt") ]
		public extern static double Sqrt(double d);

		[ DllImport("msvcrt", EntryPoint="tan") ]
		public extern static double Tan(double d);

		[ DllImport("msvcrt", EntryPoint="tanh") ]
		public extern static double Tanh(double d);

	}
}
