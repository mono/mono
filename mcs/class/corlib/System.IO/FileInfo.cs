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

		public FileInfo (string fileName)
		{
			CheckArgument.Path (fileName, false);
			//LAMESPEC: Does not throw security exception in constructor
			OriginalPath = fileName;
		}

		private bool existsOnDisk (bool exNotFound, bool exIsDirectory)
		{
			bool bRetCode;
			
			try
			{
				Refresh ();
				if ((getAttributes () & FileAttributes.Directory) != 0)
				{
					if (exIsDirectory)
					{
						throw new UnauthorizedAccessException ();
					}
					bRetCode = false;
				}
				else
				{
					bRetCode = true;
				}
			}
			catch (ArgumentException)
			{
				if (exNotFound)
				{
					throw new FileNotFoundException ();
				}
				bRetCode = false;
			}
			return bRetCode;
		}
		
		public override bool Exists
		{
			get
			{
				return existsOnDisk (false, false);	
			}
		}

		public override string Name
		{
			get
			{
				return Path.GetFileName (getPathName ());
			}
		}

		/// <summary>
		/// Gets the parent directory info
		/// </summary>
		public DirectoryInfo Directory
		{
			get
			{
				return new DirectoryInfo (Path.GetDirectoryName (getPathName ()));
			}
		}

		/// <summary>
		/// Get the path of the file
		/// </summary>
		public string DirectoryName
		{
			get
			{
				return Path.GetDirectoryName (getPathName ());
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
					Refresh ();
				}
				catch (ArgumentException)
				{
					throw new FileNotFoundException ();
				}
				return _os.FileLength (getPathName ());
			}
		}

		[MonoTODO]
   		public StreamWriter AppendText ()
		{	
			return File.AppendText (OriginalPath);
		}

		public FileStream Create ()
		{
			return File.Create (OriginalPath);
		}

		[MonoTODO]
		public StreamWriter CreateText ()
		{	
			return new StreamWriter (Open (FileMode.Create, FileAccess.Write));
		}
		
		public FileStream Open (FileMode mode)
		{
			return Open (mode, FileAccess.ReadWrite);
		}

		public FileStream Open (FileMode mode, FileAccess access)
		{
			return Open (mode, access, FileShare.None);
		}
		
		public FileStream Open (FileMode mode, FileAccess access, FileShare share)
		{
			bool bExists = existsOnDisk (false, true); // throw is directory;
			string path = getPathName ();
			CheckPermission.ModeAccess (mode, access, path, bExists);			
			return new FileStream (path, mode, access, share);
		}

		[MonoTODO]
		public FileStream OpenRead ()
		{	
			return Open (FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		[MonoTODO]
		public StreamReader OpenText ()
		{	
			return new StreamReader (Open (FileMode.OpenOrCreate, FileAccess.ReadWrite));
		}

		public FileStream OpenWrite ()
		{
			return Open (FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		}

		public FileInfo CopyTo (string destFile)
		{
			return CopyTo (destFile, false);
		}

		public FileInfo CopyTo (string destFile, bool bOverwrite)
		{
			File.Copy (OriginalPath, destFile);
			return new FileInfo (destFile);
		}

		public override void Delete ()
		{
			existsOnDisk (true, true); // throw not found, is directory
			CheckPermission.Demand (FileIOPermissionAccess.AllAccess, getPathName ());
			_os.DeleteFile (getPathName ());
		}

		public void MoveTo (string destName)
		{
			File.Move (OriginalPath, destName);
		}
	}
}
