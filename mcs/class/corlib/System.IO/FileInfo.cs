//------------------------------------------------------------------------------
// 
// System.IO.FileInfo.cs 
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
	public sealed class FileInfo : FileSystemInfo
	{
		public FileInfo()
		{
			// 
			// TODO: Add constructor logic here
			//
		}

		public override bool Exists
		{
			get
			{
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

		/// <summary>
		/// Gets the parent directory info
		/// </summary>
		public DirectoryInfo Directory
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get the path of the file
		/// </summary>
		public string DirectoryName
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get the length of the file
		/// </summary>
		public long Length
		{
			get
			{
				return 0;
			}
		}

		/* TODO: Uncomment / implement as classes become available
		public StreamWriter AppendText()
		{
		}
		

		public FileStream Create()
		{
			return null;
		}

		public StreamWriter CreateText()
		{
			return null;
		}
		
		public FileStream Open(FileMode mode)
		{
			return Open(mode, FileAccess.ReadWrite);
		}

		public FileStream Open(FileMode mode, FileAccess access)
		{
			return Open(mode, access, FileShare.None);
		}

		public FileStream Open(FileMode mode, FileAccess access, FileShare share)
		{
			return null;
		}

		public FileStream OpenRead()
		{	// TODO: find out what default share should be
			return Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public StreamReader OpenText()
		{
			return null;
		}

		public FileStream OpenWrite()
		{
			return Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		}
		*/

		public FileInfo CopyTo(string destFile)
		{
			return CopyTo(destFile, false);
		}

		public FileInfo CopyTo(string destFile, bool bOverwrite)
		{
			return null;
		}

		public override void Delete()
		{
		}

		public void MoveTo(string destName)
		{
		}
	}
}
