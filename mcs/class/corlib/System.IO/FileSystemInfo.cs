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
		private FileAttributes itsAttributes;
		private DateTime itsCreated;
		private DateTime itsLastAccess;
		private DateTime itsLastWrite;
		//private string itsFullName;
		protected string FullPath;
		protected string OriginalPath;

		protected FileSystemInfo()
		{
			itsAttributes = FileAttributes.Normal;
			itsCreated = itsLastAccess = itsLastWrite = DateTime.MinValue;
			FullPath = OriginalPath = String.Empty;
		}

		public FileAttributes Attributes
		{
			get
			{
				return itsAttributes;
			}
			set
			{
				itsAttributes = value;
			}
		}

		public DateTime CreationTime
		{
			get
			{
				return itsCreated;
			}
			set
			{
				itsCreated = value;
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
				return Path.GetExtension(FullPath);
			}
		}

		public string FullName
		{
			get
			{
				return FullPath;
			}
		}

		public DateTime LastAccessTime
		{
			get {
				return itsLastAccess;
			}

			set {
				// FIXME: IMPLEMENT ME!
				
			}
		}

		public DateTime LastWriteTime
		{
			get {
				return itsLastWrite;
			}

			set {
				// FIXME: IMPLEMENT ME!
			}
		}

		public override int GetHashCode()
		{
			return FullPath.GetHashCode();
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
		{	// TODO: Implement
		}

		/* TODO: determine if we need these
		public override ObjRef CreateObjRef(Type requestedType)
		{
			return null;
		}
		
		/*public object GetLifeTimeService ()
		{
			return null;
		}

		public override object InitializeLifeTimeService ()
		{
			return null;
		}
		*/
	}
}
