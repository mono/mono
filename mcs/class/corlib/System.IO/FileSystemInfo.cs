//------------------------------------------------------------------------------
// 
// System.IO.FileSystemInfo.cs 
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
	public abstract class FileSystemInfo : MarshalByRefObject
	{
		// protected stat status;
		private bool inited;
		
		protected string FullPath;
		protected string OriginalPath;

		protected FileSystemInfo()
		{
			/*
			status.st_dev = 0;
			status.st_mode = 0;
			status.st_nlink = 0;
			status.st_uid = 0;
			status.st_gid = 0;
			status.st_size = 0;
			status.st_atime = 0;
			status.st_mtime = 0;
			status.st_ctime = 0;
			*/

			FullPath = OriginalPath = String.Empty;
		}

		public FileAttributes Attributes
		{ 
			get
			{
				return getAttributes();
			}
			set
			{
				//TODO: Implement 
			}
		}

		public DateTime CreationTime
		{
			get
			{
				if(!inited)
				{
					update();
				}
				// TODO: fix next line as far as my research has taken me so far, Unix/Linux don't
				//       have a creation time and according to my man the ctime if the last time
				//       one of the chmod flags was changed
				return c2csharpTime(10);//status.st_ctime);
			}
			set
			{
				//TODO: Implement
			}
		}

		public abstract bool Exists {get;}
		public abstract string Name {get;}
		public abstract void Delete();

		/// <summary>
		/// Get the extension of this item
		/// </summary>
		public string Extension
		{
			get
			{
				return Path.GetExtension(getPathName());
			}
		}

		public string FullName
		{
			get
			{
				return getPathName();
			}
		}

		public DateTime LastAccessTime
		{
			get
			{
				if(!inited)
				{
					update();
				}
				return c2csharpTime(1);//status.st_atime);
			}

			set
			{
				// TODO: Implement
			}
		}

		public DateTime LastWriteTime
		{	// TODO: Implement
			get
			{
				if(!inited)
				{
					update();
				}
				return c2csharpTime(1);//status.st_mtime);
			}
			set
			{	// TODO: Implement
			}
		}

		public override int GetHashCode()
		{
			return getPathName().GetHashCode();
		}

		public override bool Equals(object obj)
		{	// TODO: Implement
			return false;
		}

		new public static bool Equals(object obj1, object obj2)
		{	// TODO: Implement
			return false;
		}
				
		public void Refresh()
		{
			update();
		}
		

		unsafe private void update()
		{
			/*
			stat fs;			
			int nRetCode = Wrapper.stat(getPathName(), &fs);
			status = fs;
			switch(nRetCode)
			{
			case 0:
				break;
			case Wrapper.ENOENT:
			case Wrapper.ENOTDIR:
				throw new ArgumentException("File not found");	
				//break; generates warning CS0162 unreachable code
			default:
				throw new IOException();
			   //break; generates warning CS0162 unreachable code
			}
			*/
			inited = true;
		}

		private DateTime c2csharpTime(double seconds)
		{	// TODO: determine if UTC time which the 
			//       calculation below is in is correct
		   DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
		   dt.AddSeconds(seconds);
		   return dt;	
		}
		
		protected string getPathName()
		{
			if(FullPath == String.Empty)
			{
				FullPath = Path.GetFullPath(OriginalPath);
			}
			return FullPath;
		} 
		
		protected FileAttributes getAttributes()
		{	
			if(!inited)
			{
				update();
			}
			
			// TODO: lots more attribute work needed
				
			FileAttributes attrib = 0;
			/*
			if(((status.st_mode & Wrapper.S_IFMT) & Wrapper.S_IFDIR) != 0)
			{
				attrib |= FileAttributes.Directory;
			}
			else
			{
				attrib |= FileAttributes.Normal;
			}
			*/

			return attrib;
		}
	}
}
