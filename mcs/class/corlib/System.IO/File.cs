//------------------------------------------------------------------------------
// 
// System.IO.File.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Monday, August 22, 2001 
//
// TODO: Research exceptions for all methods
//------------------------------------------------------------------------------

using System;

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class File : Object
	{ 
		/// <summary>
		/// Creates a StreamWriter that appends text to a file creating the file if needed
		/// </summary>
		public static StreamWriter AppendText(string path)
		{	// TODO: Implement
			return null;
		}
		 
		/// <summary>
		/// Copies a file overwriting existing if necessary
		/// </summary>
		public static void Copy(string sourceFilename, string destFilename)
		{
			Copy(sourceFilename, destFilename, true);
		}
		 
		/// <summary>
		/// Copies a file overwriting existing if specified
		/// </summary>
		public static void Copy(string sourceFilename, string destFilename, bool bOverwrite)
		{	// TODO: Implement
		}

		/// <summary>
		/// Creates a file given the fully qualified path
		/// </summary>
		public static FileStream Create(string path)
		{	// TODO: Research default buffersize
			return Create(path, 1024);
		}

		/// <summary>
		/// Creates a file given the fully qualified path using specified buffersize
		/// </summary>
		public static FileStream Create(string path, int buffersize)
		{	// TODO: Implement
			return null;
		}
		
		/// <summary>
		/// Delete a file
		/// </summary>
		public static void Delete(string path)
		{	// TODO: Implement
		}
		
		/// <summary>
		/// Returns true if file exists on disk
		/// </summary>
		public static bool Exists(string path)
		{	// TODO: Implement
			return false;
		}
		
		/// <summary>
		/// Returns the date and time the file specified by path was created
		/// </summary>
		public static FileAttributes GetAttributes(string path)
		{
			FileInfo fInfo = new FileInfo(path);
			return fInfo.Attributes;
		}

		/// <summary>
		/// Returns the date and time the directory specified by path was created
		/// </summary>
		public static DateTime GetCreationTime(string path)
		{
			return getInfo(path).CreationTime;
		}

		/// <summary>
		/// Returns the date and time the directory specified by path was last accessed
		/// </summary>
		public static DateTime GetLastAccessTime(string path)
		{
			return getInfo(path).LastAccessTime;
		}

		/// <summary>
		/// Returns the date and time the directory specified by path was last modified
		/// </summary>
		public static DateTime GetLastWriteTime(string path)
		{
			return getInfo(path).LastWriteTime;
		}
		
		/// <summary>
		/// Moves a file
		/// </summary>
		public static void Move(string srcFilename, string destFilename)
		{
			getInfo(srcFilename).MoveTo(destFilename);
		}
		
		/// <summary>
		/// Open a file for exclusive reading and writing
		/// </summary>
		public static FileStream Open(string path, FileMode mode)
		{	// TODO: research if exclusive is the correct default
			return getInfo(path).Open(mode, FileAccess.ReadWrite);
		}
		
		/// <summary>
		/// Open a file for exclusive access specified by mode
		/// </summary>
		public static FileStream Open(string path, FileMode mode, FileAccess access)
		{	// TODO: research if exclusive is the correct default
			return getInfo(path).Open(mode, access, FileShare.None);
		}
		
		/// <summary>
		/// Open a file access specified by mode, sharing specified by share
		/// </summary>
		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return getInfo(path).Open(mode, access, share);
		}
		
		/// <summary>
		/// Open a FileStream for reading and writing
		/// </summary>
		public static FileStream OpenRead(string path)
		{	// TODO: find out what default share should be
			return getInfo(path).OpenRead();
		}
		
		/// <summary>
		/// Open a StreamReader
		/// </summary>
		public static StreamReader OpenText(string path)
		{
			return getInfo(path).OpenText();
		}

		/// <summary>
		/// Open a FileStream for reading and writing
		/// </summary>
		public FileStream OpenWrite(string path)
		{
			return getInfo(path).OpenWrite();
		}
		
		/// <summary>
		/// Sets the attributes of file specified by path
		/// </summary>
		public static void SetAttributes(string path, FileAttributes attributes)
		{
			getInfo(path).Attributes = attributes;
		}
		
		/// <summary>
		/// Sets the creation time of the directory specified by path
		/// </summary>
		public static void SetCreationTime(string path, DateTime creationTime)
		{
			getInfo(path).CreationTime = creationTime;
		}

		/// <summary>
		/// Sets the last access time of the directory specified by path
		/// </summary>
		public static void SetLastAccessTime(string path, DateTime accessTime)
		{
			getInfo(path).LastAccessTime = accessTime;
		}
		
		/// <summary>
		/// Sets the last write time of the directory specified by path
		/// </summary>
		public static void SetLastWriteTime(string path, DateTime modifiedTime)
		{
			getInfo(path).LastWriteTime = modifiedTime;
		}
		
		private static FileInfo getInfo(string path)
		{
			return new FileInfo(path);
		}
	}
}
