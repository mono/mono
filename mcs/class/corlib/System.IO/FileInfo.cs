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
using System.PAL;
//using System.Diagnostics;
using System.Security.Permissions;

namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class FileInfo : FileSystemInfo
	{
		private OpSys _os = Platform.OS;

		public FileInfo(string fileName)
		{
			CheckArgument.Path(fileName, false);
			//LAMESPEC: Does not throw security exception in constructor
			OriginalPath = fileName;
		}

		private bool existsOnDisk(bool exNotFound, bool exIsDirectory)
		{
			bool bRetCode;
			
			try
			{
				Refresh();
				if((getAttributes() & FileAttributes.Directory) != 0)
				{
					if(exIsDirectory)
					{
						throw new UnauthorizedAccessException();
					}
					bRetCode = false;
				}
				else
				{
					bRetCode = true;
				}
			}
			catch(ArgumentException ex)				
			{
				//Debug.WriteLine(ex); // eliminates not used warning
				if(exNotFound)
				{
					throw new FileNotFoundException();
				}
				bRetCode = false;
			}
			return bRetCode;
		}
		
		public override bool Exists
		{
			get
			{
				return existsOnDisk(false, false);	
			}
		}

		public override string Name
		{
			get
			{
				return Path.GetFileName(getPathName());
			}
		}

		/// <summary>
		/// Gets the parent directory info
		/// </summary>
		public DirectoryInfo Directory
		{
			get
			{
				return new DirectoryInfo(Path.GetDirectoryName(getPathName()));
			}
		}

		/// <summary>
		/// Get the path of the file
		/// </summary>
		public string DirectoryName
		{
			get
			{
				return Path.GetDirectoryName(getPathName());
			}
		}

		/// <summary>
		/// Get the length of the file
		/// </summary>
		public long Length
		{
			get
			{
				try
				{
					Refresh();
				}
				catch(ArgumentException ex)
				{
					//Debug.WriteLine(ex); // eliminates not used compiler warning
					throw new FileNotFoundException();
				}
				return _os.FileLength(getPathName());
			}
		}

		[MonoTODO]
   		public StreamWriter AppendText()
		{	// TODO: verify using correct FileMode here might be Create & Append
			return new StreamWriter(Open(FileMode.Append, FileAccess.Write));
		}

		[MonoTODO]
		public FileStream Create()
		{
			// TODO: verify using correct FileMode here
			return Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
		}

		[MonoTODO]
		public StreamWriter CreateText()
		{	//TODO: According to doc even CreateText throws a file not found ex
			//      sounds suspicious so i'll have to check it out later
			//existsOnDisk(true, true); // throw not found, is directory
			return new StreamWriter(Open(FileMode.Create, FileAccess.Write));
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
			bool bExists = existsOnDisk(false, true); // throw is directory;
			string path = getPathName();
		    CheckPermission.ModeAccess(mode, access, path, bExists);			
			return new FileStream(path, mode, access, share);
		}

		[MonoTODO]
		public FileStream OpenRead()
		{	// TODO: find out what default share should be
			return Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		[MonoTODO]
		public StreamReader OpenText()
		{	// TODO: verify mode and access values
			return new StreamReader(Open(FileMode.OpenOrCreate, FileAccess.ReadWrite));
		}

		public FileStream OpenWrite()
		{
			return Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		}

		public FileInfo CopyTo(string destFile)
		{
			return CopyTo(destFile, false);
		}

		[MonoTODO]
		public FileInfo CopyTo(string destFile, bool bOverwrite)
		{	// TODO: Implement
			return null;
		}

		public override void Delete()
		{
			existsOnDisk(true, true); // throw not found, is directory
			CheckPermission.Demand(FileIOPermissionAccess.AllAccess, getPathName());
			_os.DeleteFile(getPathName());
		}

		[MonoTODO]
		public void MoveTo(string destName)
		{	// TODO: Implement
		}
	}
}
