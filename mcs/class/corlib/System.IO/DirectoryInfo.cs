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
using System.Diagnostics;

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class DirectoryInfo : FileSystemInfo
	{

		public DirectoryInfo(string path)
		{
			CheckArgument.Path(path, false);
			//LAMESPEC: Does not throw directory not found exception
			//          Does not throw security exception in constructor
			OriginalPath = path;	
		}

		public override bool Exists
		{
			get
			{
				bool bRetCode;
				
				try
				{
					Refresh();
					bRetCode = ((Attributes & FileAttributes.Directory) != 0);
				}
				catch(ArgumentException ex)				
				{
					Debug.WriteLine(ex); // eliminates not used warning
					bRetCode = false;
				}
				return bRetCode;
			}
		}

		[MonoTODO]
		public override string Name
		{
			get
			{	//TODO: Implement this as per the documenation
				return FullPath;
			}
		}

		[MonoTODO]
		public DirectoryInfo Root
		{
			get
			{	//TODO: Implement
				return null;
			}
		}

		[MonoTODO]
		public void Create()
		{
			//TODO: Implement
		}

		[MonoTODO]
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
		[MonoTODO]
		public DirectoryInfo[] GetDirectories()
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of DirectoryInfos
		/// matching the filter specified by mask
		/// </summary>
		[MonoTODO]
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
		[MonoTODO]
		public static FileInfo[] GetFiles(string mask)
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of FileSystemInfo for subdirectories
		/// </summary>
		[MonoTODO]
		public FileSystemInfo[] GetFileSystemInfos()
		{
			return null;	//TODO: Implement
		}
		/// <summary>
		/// Returns an array of FileSystemInfo
		/// matching the filter specified by mask
		/// </summary>
		[MonoTODO]
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
