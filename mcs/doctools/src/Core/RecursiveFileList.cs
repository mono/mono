// RecursiveFileList.cs
// John Sohn (jsohn@columbus.rr.com)
// 
// Copyright (c) 2002 John Sohn
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.IO;


namespace Mono.Doc.Core 
{
	/// <summary>
	/// Recurses through the given directory and subdirectories based on a 
	/// filter (wildcard) passed into the constructor. The results are placed
	/// in the Files property which is an ArrayList of System.IO.FileInfo objects
	/// </summary>
	public class RecursiveFileList 
	{
		#region Private Instance Fields

		private ArrayList fileInfoList;

		#endregion // Private Instance Fields

		#region Constructors and Destructors

		/// <summary>Constructor for RecursiveFileList.</summary>
		/// <param name="path">The directory to recurse.</param>
		/// <exception cref="DirectoryNotFoundException">Thrown if an invalid path is specified.</exception>
		public RecursiveFileList(string path) : this(path, null) 
		{
		}

		/// <summary>
		/// Constructor for RecursiveFileList.
		/// </summary>
		/// <param name="path">The directory to recurse.</param>
		/// <param name="filter">Wildcard of files to collect.</param>
		/// <exception cref="DirectoryNotFoundException">Thrown if an invalid path is specified.</exception>
		public RecursiveFileList(string path, string filter)
		{
			this.fileInfoList = new ArrayList();
			AddFilesInPath(path, filter);
		}

		#endregion // Constructors and Destructors

		#region Private Instance Methods

		private void AddFilesInPath(string path, string filter)
		{
			DirectoryInfo dir = new DirectoryInfo(path);


			if (filter == null) 
			{
				filter = "*.*";
			}
                        
			foreach (FileSystemInfo fi in dir.GetFiles(filter)) 
			{
				this.fileInfoList.Add(fi);
			}


			foreach (DirectoryInfo di in dir.GetDirectories()) 
			{
				AddFilesInPath(di.FullName, filter);
			}
		}

		#endregion // Private Instance Methods

		#region Public Instance Properties

		/// <summary>
		/// Contains an ArrayList of System.IO.FileInfo objects
		/// based on the path (and optional filter) passed to the constructor.
		/// </summary>
		public ArrayList Files
		{
			get { return this.fileInfoList; }
		}

		#endregion // Public Instance Properties
	}
}

