//------------------------------------------------------------------------------
// 
// System.IO.Directory.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Monday, August 13, 2001 
//
// TODO: Research exceptions for all methods
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
			DirectoryInfo dInfo = getInfo(path);
			if(!dInfo.Exists)
			{
				dInfo.Create();
			}
			return dInfo;
		}
		
		/// <summary>
		/// Delete an empty directory
		/// </summary>
		public static void Delete(string path)
		{	
			DirectoryInfo dInfo = getInfo(path);
			if(dInfo.Exists)
			{
				dInfo.Delete();
			}
		}
		
		/// <summary>
		/// Delete a directory, and contents if bRecurse is true
		/// </summary>
		public static void Delete(string path, bool bRecurse)
		{	
			DirectoryInfo dInfo = getInfo(path);
			if(dInfo.Exists)
			{
				dInfo.Delete(bRecurse);
			}
		}
		
		/// <summary>
		/// Returns true if directory exists on disk
		/// </summary>
		public static bool Exists(string path)
		{
			return getInfo(path).Exists;
		}
		/// <summary>
		/// Returns the date and time the directory specified by path was created
		/// </summary>
		public static DateTime GetCreationTime(string path)
		{
			return getInfo().GetCreationTime(path);
		}
		/// <summary>
		/// Returns the date and time the directory specified by path was created
		/// </summary>
		public static string GetCurrentDirectory()
		{	// TODO: Implement
			return null;
		}
		/// <summary>
		/// Returns an array of directories in the directory specified by path
		/// </summary>
		public static string[] GetDirectories(string path)
		{
			return getNames(getInfo(path).GetDirectories());
		}
		
		/// <summary>
		/// Returns an array of directories in the directory specified by path
		/// matching the filter specified by mask
		/// </summary>
		public static string[] GetDirectories(string path, string mask)
		{
			return getNames(getInfo(path).GetDirectories(mask));
		}
		
		/// <summary>
		/// Returns the root of the specified path
		/// </summary>
		public static string GetDirectoryRoot(string path)
		{
			return getInfo(path).Root.FullName();
		}
		
		/// <summary>
		/// Returns an array of files in the directory specified by path
		/// </summary>
		public static string[] GetFiles(string path)
		{
			return getNames(getInfo(path).GetFiles());
		}
		
		/// <summary>
		/// Returns an array of files in the directory specified by path
		/// matching the filter specified by mask
		/// </summary>
		public static string[] GetFiles(string path, string mask)
		{
			return getNames(getInfo(path).GetFiles());
		}
		/// <summary>
		/// Returns an array of filesystementries in the directory specified by path
		/// I think this is just files and directories
		/// </summary>
		public static string[] GetFileSystemEntries(string path)
		{	// TODO: Research to verify this is files + directories
			return getNames(getInfo(path).GetFileSystemInfos());
		}
		/// <summary>
		/// Returns an array of filesystementries in the directory specified by path
		/// matching the filter specified by mask
		/// </summary>
		public static string[] GetFileSystemEntries(string path, string mask)
		{	// TODO: Research to verify this is files + directories
			return getNames(getInfo(path).GetFileSystemInfos());		}
		
		/// <summary>
		/// Returns the date and time the directory specified by path was last accessed
		/// </summary>
		public static DateTime GetLastAccessTime(string path)
		{
			return getInfo().GetLastWriteTime(path);
		}
		
		/// <summary>
		/// Returns the date and time the directory specified by path was last modified
		/// </summary>
		public static DateTime GetLastWriteTime(string path)
		{
			return getInfo().GetLastWriteTime(path);
		}
		
		/// <summary>
		/// Returns an array of logical drives on this system
		/// </summary>
		public static string[] GetLogicalDrives()
		{	// TODO: Implement
			return null;
		}
		/// <summary>
		/// Returns the parent directory of the directory specified by path
		/// </summary>
		public static DirectoryInfo GetParent(string path)
		{	// TODO: Implement
			return null;
		}
		/// <summary>
		/// Moves a directory and its contents
		/// </summary>
		public static void Move(string src, string dst)
		{
			 getInfo(src).MoveTo(dst);
		}
		
		/// <summary>
		/// Sets the creation time of the directory specified by path
		/// </summary>
		public static void SetCreationTime(string path, DateTime creationTime)
		{
			getInfo().SetCreationTime(path, creationTime);
		}
		
		/// <summary>
		/// Sets the current directory to the directory specified by path
		/// </summary>
		public static void SetCurrentDirectory(string path)
		{	// TODO: Implement
		}
		
		/// <summary>
		/// Sets the last access time of the directory specified by path
		/// </summary>
		public static void SetLastAccessTime(string path, DateTime accessTime)
		{
			getInfo().SetLastAccessTime(path, accessTime);
		}
		
		/// <summary>
		/// Sets the last write time of the directory specified by path
		/// </summary>
		public static void SetLastWriteTime(string path, DateTime modifiedTime)
		{
			getInfo().SetLastWriteTime(path, modifiedTime);
		}
		
		private static DirectoryInfo getInfo(string path)
		{
			return new DirectoryInfo(path);
		}
		
		private static string[] getNames(FileSystemInfo[] arInfo)
		{
			int index = 0;
			string[] ar = new string[arInfo.Length];
						
			foreach(FileInfo fi in arInfo)
			{
				ar[index++] = fi.FullName;
			}
			return ar;
		}
	}
}
