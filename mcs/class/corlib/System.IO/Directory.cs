//------------------------------------------------------------------------------
// 
// System.IO.Directory.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

using System;

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Directory : Object
	{

		/// <summary>
		/// Creates all directories not existing in path
		/// </summary>
		public static DirectoryInfo CreateDirectory(string path)
		{
			return null;
		}
		/// <summary>
		/// Delete an empty directory
		/// </summary>
		public static void Delete(string path)
		{
		}
		/// <summary>
		/// Delete a directory, and contents if bRecurse is true
		/// </summary>
		public static void Delete(string path, bool bRecurse)
		{
		}
		/// <summary>
		/// Returns true if directory exists on disk
		/// </summary>
		public static bool Exists(string path)
		{
			return false;
		}
		/// <summary>
		/// Returns the date and time the directory specified by path was created
		/// </summary>
		public static DateTime GetCreationTime(string path)
		{
			return DateTime.MinValue;
		}
		/// <summary>
		/// Returns the date and time the directory specified by path was created
		/// </summary>
		public static string GetCurrentDirectory()
		{
			return null;
		}
		/// <summary>
		/// Returns an array of directories in the directory specified by path
		/// </summary>
		public static string[] GetDirectories(string path)
		{
			return null;
		}
		/// <summary>
		/// Returns an array of directories in the directory specified by path
		/// matching the filter specified by mask
		/// </summary>
		public static string[] GetDirectories(string path, string mask)
		{
			return null;
		}
		/// <summary>
		/// Returns the root of the specified path
		/// </summary>
		public static string GetDirectoryRoot(string path)
		{
			return null;
		}
		/// <summary>
		/// Returns an array of files in the directory specified by path
		/// </summary>
		public static string[] GetFiles(string path)
		{
			return null;
		}
		/// <summary>
		/// Returns an array of files in the directory specified by path
		/// matching the filter specified by mask
		/// </summary>
		public static string[] GetFiles(string path, string mask)
		{
			return null;
		}
		/// <summary>
		/// Returns an array of filesystementries in the directory specified by path
		/// </summary>
		public static string[] GetFileSystemEntries(string path)
		{
			return null;
		}
		/// <summary>
		/// Returns an array of filesystementries in the directory specified by path
		/// matching the filter specified by mask
		/// </summary>
		public static string[] GetFileSystemEntries(string path, string mask)
		{
			return null;
		}
		/// <summary>
		/// Returns the date and time the directory specified by path was last accessed
		/// </summary>
		public static DateTime GetLastAccessTime(string path)
		{
			return DateTime.MinValue;
		}
		/// <summary>
		/// Returns the date and time the directory specified by path was last modified
		/// </summary>
		public static DateTime GetLastWriteTime(string path)
		{
			return DateTime.MinValue;
		}
		/// <summary>
		/// Returns an array of logical drives on this system
		/// </summary>
		public static string[] GetLogicalDrives()
		{
			return null;
		}
		/// <summary>
		/// Returns the parent directory of the directory specified by path
		/// </summary>
		public static DirectoryInfo GetParent(string path)
		{
			return null;
		}
		/// <summary>
		/// Moves a directory and its contents
		/// </summary>
		public static void Move(string srcDirName, string destDirName)
		{
		}
		/// <summary>
		/// Sets the creation time of the directory specified by path
		/// </summary>
		public static void SetCreationTime(string path, DateTime creationTime)
		{
		}
		/// <summary>
		/// Sets the current directory to the directory specified by path
		/// </summary>
		public static void SetCurrentDirectory(string path)
		{
		}
		/// <summary>
		/// Sets the last access time of the directory specified by path
		/// </summary>
		public static void SetLastAccessTime(string path, DateTime accessTime)
		{
		}
		/// <summary>
		/// Sets the last write time of the directory specified by path
		/// </summary>
		public static void SetLastWriteTime(string path, DateTime modifiedTime)
		{
		}
	}
}
