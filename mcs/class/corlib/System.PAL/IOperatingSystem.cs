
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

CREATED: August	08, 2001
OWNER: Scott D Smith, Joel Marcey
VERSION: 1.0
---------------------------------------------------------------------*/
	    

using System;
using System.IO;
using System.Collections;

namespace System.PlatformAbstractionLayer
{
	/// <summary>
	///	Definition of functionality needed by the library that can only	be provided by the underlying OS. 
	/// </summary>
	internal interface IOperatingSystem
	{
	
		// System.IO services
	
		int ReadStdInput(byte[]	buffer,	int offset, int	count);

		void FlushStdOutput(byte[] byteBuf);



		//  System.File	services

		int ReadFile(IntPtr handle, byte[] buffer, int offset, int count);

		int WriteFile(IntPtr handle, byte[] buffer, int offset, int count);
	
		void FlushFile(IntPtr handle, byte[] byteBuf);
	
		int SetLengthFile(IntPtr handle, long length);

		IntPtr OpenFile(string path, FileMode mode, FileAccess access, FileShare share);
	
		void CloseFile(IntPtr handle);
	
		long SeekFile(IntPtr handle, long offset, SeekOrigin origin);
	
		IntPtr CreateFile(string path, FileMode	mode, FileAccess access, FileShare share);
	
		void DeleteFile(string path);
	
		bool ExistsFile(string path);
	
		DateTime GetCreationTimeFile(string path);
	
		DateTime GetLastAccessTimeFile(string path);
	
		DateTime GetLastWriteTimeFile(string path);
	
		void SetCreationTimeFile(string	path, DateTime creationTime);
	
		void SetLastAccessTimeFile(string path,	DateTime lastAccessTime);
	
		void SetLastWriteTimeFile(string path, DateTime	lastWriteTime);

		long FileLength(string path);

		long FileLength(IntPtr handle);
	

		//  System.Environment services
		
		string NewLineSequence {get;}
	
		char DirectorySeparator	{get;}
	
		char AltDirectorySeparator {get;}
	
		char PathSeparator {get;}
	
		char VolumeSeparator {get;}
	
		char[] DirVolSeparatorChars {get;}
	
		char[] InvalidPathChars	{get;}
	
		string GetEnvironmentVariable(string eVar);
	
		char ExtensionCharacter	{get;}

		string CommandLine {get;}

		IDictionary EnvironmentVariables {get;}

		string MachineName {get;}
	
		OperatingSystem	OSVersion {get;}

		//  System.Path	services
		// Note: Although some of these	do not require direct acccess to the OS,
		// some	platforms don't	support	some of	these methods
	
		string ChangeExtension(string path, string extension);
	
		string GetExtension(string path);
       
		string GetFileName(string path);
	
		string GetFileNameWithoutExtension(string path);
	
		string GetPathRoot(string path);
	
		string GetTempFileName();
	
		string GetTempPath();
	
		bool HasExtension(string path);
	
		bool IsPathRooted(string path);
	
		string GetFullPath(string path);
	
	
		//  System.Directory services
	
		void DeleteDirectory(string path, bool recursive);
	
		bool ExistsDirectory(string path);
	
		DateTime GetCreationTimeDirectory(string path);
	
		string GetCurrentDirectory();
	
		string[] GetDirectories(string path, string searchPattern);
	
		string[] GetFiles(string path, string searchPattern);
	
		string[] GetFileSystemEntries(string path, string searchPattern);
	
		DateTime GetLastAccessTimeDirectory(string path);
	
		DateTime GetLastWriteTimeDirectory(string path);
	
		void MoveDirectory(string sourceDirName, string	destDirName);
	
		void SetCreationTimeDirectory(string path, DateTime creationTime);
	
		void SetCurrentDirectory(string	path);
	
		void SetLastAccessTimeDirectory(string path, DateTime lastAccessTime);
	
		void SetLastWriteTimeDirectory(string path, DateTime lastWriteTime);

		double Acos(double d);

		double Asin(double d);

		double Atan(double d);

		double Atan2(double y, double x);

		double Cos(double d);

		double Cosh(double value);

		double Exp(dobule d);

		double Log(double d);

		double Log10(double d);

		double Pow(double x, double y);

		double Sin(double d);

		double Sinh(double d);

		double Sqrt(double d);

		double Tan(double d);

		double Tanh(double d);
		
	}
}
