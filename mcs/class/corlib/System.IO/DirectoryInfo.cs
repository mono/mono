//------------------------------------------------------------------------------
// 
// System.IO.DirectoryInfo.cs 
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
	public sealed class DirectoryInfo : FileSystemInfo
	{

		public DirectoryInfo(string path)
		{
			if(path == null) throw new ArgumentNullException();
			if(path.IndexOfAny(Path.InvalidPathChars) > -1)
				throw new ArgumentException("Invalid path characters");
			//LAMESPEC: If we throw here how could we ever Create or check existence?	
			//if(!Directory.Exists(path)) throw new DirectoryNotFoundException();
		}

		public override bool Exists
		{
			get
			{	//TODO: Implement
				return false;
			}
		}

		public override string Name
		{
			get
			{	//TODO: Implement this as per the documenation
				return FullPath;
			}
		}

		public DirectoryInfo Root
		{
			get
			{	//TODO: Implement
				return null;
			}
		}

		public void Create()
		{
			//TODO: Implement
		}

		DirectoryInfo CreateSubdirectory(string path)
		{
			return null;	//TODO: Implement
		}

		public override void Delete()
		{
			Directory.Delete(FullPath);
		}

		public void Delete(bool bRecurse)
		{
			Directory.Delete(FullPath, bRecurse);
		}

		/// <summary>
		/// Returns an array of DirectoryInfos for subdirectories
		/// </summary>
		public DirectoryInfo[] GetDirectories()
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of DirectoryInfos
		/// matching the filter specified by mask
		/// </summary>
		public static DirectoryInfo[] GetDirectories(string mask)
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of FileInfo for subdirectories
		/// </summary>
		public FileInfo[] GetFiles()
		{
			return null;
		}
		/// <summary>
		/// Returns an array of FileInfo
		/// matching the filter specified by mask
		/// </summary>
		public static FileInfo[] GetFiles(string mask)
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of FileSystemInfo for subdirectories
		/// </summary>
		public FileSystemInfo[] GetFileSystemInfos()
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of FileSystemInfo
		/// matching the filter specified by mask
		/// </summary>
		public static FileSystemInfo[] GetFileSystemInfos(string mask)
		{
			return null;	//TODO: Implement
		}

		public void MoveTo(string destDirName)
		{
			Directory.Move(FullName, destDirName);
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
