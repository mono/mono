//------------------------------------------------------------------------------
// 
// System.IO.FileSystemInfo.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

using System;

namespace System.IO {
	
	[Serializable]
	public abstract class FileSystemInfo : MarshalByRefObject {
		// public properties

		public abstract bool Exists { get; }

		public abstract string Name { get; }

		public virtual string FullName {
			get {
				return FullPath;
			}
		}

		public string Extension {
			get {
				return Path.GetExtension (Name);
			}
		}

		public FileAttributes Attributes {
			get {
				Refresh (false);
				return stat.Attributes;
			}

			set {
				MonoIOError error;
				
				if (!MonoIO.SetFileAttributes (FullName,
							       value,
							       out error))
					throw MonoIO.GetException (error);
			}
		}

		public DateTime CreationTime {
			get {
				Refresh (false);
				return DateTime.FromFileTime (stat.CreationTime);
			}

			set {
				long filetime = value.ToFileTime ();
			
				MonoIOError error;
				
				if (!MonoIO.SetFileTime (FullName, filetime,
							 -1, -1, out error))
					throw MonoIO.GetException (error);
			}
		}

		public DateTime LastAccessTime {
			get {
				Refresh (false);
				return DateTime.FromFileTime (stat.LastAccessTime);
			}

			set {
				long filetime = value.ToFileTime ();

				MonoIOError error;
				
				if (!MonoIO.SetFileTime (FullName, -1,
							 filetime, -1,
							 out error))
					throw MonoIO.GetException (error);
			}
		}

		public DateTime LastWriteTime {
			get {
				Refresh (false);
				return DateTime.FromFileTime (stat.LastWriteTime);
			}

			set {
				long filetime = value.ToFileTime ();

				MonoIOError error;
				
				if (!MonoIO.SetFileTime (FullName, -1, -1,
							 filetime, out error))
					throw MonoIO.GetException (error);
			}
		}

		// public methods

		public abstract void Delete ();

		public void Refresh ()
		{
			Refresh (true);
		}

		// protected

		protected FileSystemInfo ()
		{
			this.valid = false;
			this.FullPath = null;
		}

		protected string FullPath;
		protected string OriginalPath;

		// internal

		internal void Refresh (bool force)
		{
			if (valid && !force)
				return;

			MonoIOError error;
			
			MonoIO.GetFileStat (FullName, out stat, out error);
			valid = true;
		}

		internal void CheckPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path.");
		}

		internal MonoIOStat stat;
		internal bool valid;
	}
}
