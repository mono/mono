//------------------------------------------------------------------------------
// 
// System.IO.FileInfo.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.IO {

	[Serializable]
	public sealed class FileInfo : FileSystemInfo {
	

		private bool exists;

		public FileInfo (string path) {
			CheckPath (path);
		
			OriginalPath = path;
			FullPath = Path.GetFullPath (path);
		}
		
		internal override void InternalRefresh ()
		{
			exists = File.Exists (FullPath);
		}


		// public properties

		public override bool Exists {
			get {
				Refresh (false);

				if (stat.Attributes == MonoIO.InvalidFileAttributes)
					return false;

				if ((stat.Attributes & FileAttributes.Directory) != 0)
					return false;

				return exists;
			}
		}

		public override string Name {
			get {
				return Path.GetFileName (FullPath);
			}
		}

		public long Length {
			get {
				if (!Exists)
					throw new FileNotFoundException ("Could not find file \"" + OriginalPath + "\".", OriginalPath);

				return stat.Length;
			}
		}

		public string DirectoryName {
			get {
				return Path.GetDirectoryName (FullPath);
			}
		}

		public DirectoryInfo Directory {
			get {
				return new DirectoryInfo (DirectoryName);
			}
		}

		// streamreader methods

		public StreamReader OpenText () {
			return new StreamReader (Open (FileMode.Open, FileAccess.Read));
		}

		public StreamWriter CreateText () {
			return new StreamWriter (Open (FileMode.Create, FileAccess.Write));
		}
		
		public StreamWriter AppendText () {
			return new StreamWriter (Open (FileMode.Append, FileAccess.Write));
		}

		// filestream methods

		public FileStream Create ()
		{
			return File.Create (FullPath);
		}
		
		
		public FileStream OpenRead () {
			return Open (FileMode.Open, FileAccess.Read);
		}

		public FileStream OpenWrite () {
			return Open (FileMode.OpenOrCreate, FileAccess.Write);
		}

		public FileStream Open (FileMode mode) {
			return Open (mode, FileAccess.ReadWrite);
		}

		public FileStream Open (FileMode mode, FileAccess access) {
			return Open (mode, access, FileShare.None);
		}

		public FileStream Open (FileMode mode, FileAccess access, FileShare share) {
			return new FileStream (FullPath, mode, access, share);
		}

		// file methods

		public override void Delete () {
			MonoIOError error;
						
			if (!MonoIO.Exists (FullPath, out error)) {
				// a weird MS.NET behaviour
				return;
			}

			if (MonoIO.ExistsDirectory (FullPath, out error)) {
				throw new UnauthorizedAccessException ("Access to the path \"" + FullPath + "\" is denied.");
			}
			
			if (!MonoIO.DeleteFile (FullPath, out error)) {
				throw MonoIO.GetException (OriginalPath,
							   error);
			}
		}
		
		public void MoveTo (string dest) {

			if (dest == null)
				throw new ArgumentNullException ();

                        if (dest == Name || dest == FullName)
                                return;

			MonoIOError error;
			if (MonoIO.Exists (dest, out error) ||
				MonoIO.ExistsDirectory (dest, out error))
				throw new IOException ();
			File.Move (FullPath, dest);
			this.FullPath = Path.GetFullPath (dest);
		}

		public FileInfo CopyTo (string path) {
			return CopyTo (path, false);
		}

		public FileInfo CopyTo (string path, bool overwrite) {
			string dest = Path.GetFullPath (path);

			if (overwrite && File.Exists (path))
				File.Delete (path);

			File.Copy (FullPath, dest);
		
			return new FileInfo (dest);
		}

		public override string ToString () {
			return OriginalPath;
		}
	}
}
