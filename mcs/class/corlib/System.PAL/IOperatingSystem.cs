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
	}
}
