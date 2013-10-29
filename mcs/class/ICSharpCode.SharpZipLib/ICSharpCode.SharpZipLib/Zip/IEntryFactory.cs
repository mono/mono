// IEntryFactory.cs
//
// Copyright 2006 John Reilly
//
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

// HISTORY
//	2012-11-29	Z-1684	Added MakeFileEntry(string fileName, string entryName, bool useFileSystem)

using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// Defines factory methods for creating new <see cref="ZipEntry"></see> values.
	/// </summary>
	public interface IEntryFactory
	{
		/// <summary>
		/// Create a <see cref="ZipEntry"/> for a file given its name
		/// </summary>
		/// <param name="fileName">The name of the file to create an entry for.</param>
		/// <returns>Returns a <see cref="ZipEntry">file entry</see> based on the <paramref name="fileName"/> passed.</returns>
		ZipEntry MakeFileEntry(string fileName);

		/// <summary>
		/// Create a <see cref="ZipEntry"/> for a file given its name
		/// </summary>
		/// <param name="fileName">The name of the file to create an entry for.</param>
		/// <param name="useFileSystem">If true get details from the file system if the file exists.</param>
		/// <returns>Returns a <see cref="ZipEntry">file entry</see> based on the <paramref name="fileName"/> passed.</returns>
		ZipEntry MakeFileEntry(string fileName, bool useFileSystem);

		/// <summary>
		/// Create a <see cref="ZipEntry"/> for a file given its actual name and optional override name
		/// </summary>
		/// <param name="fileName">The name of the file to create an entry for.</param>
		/// <param name="entryName">An alternative name to be used for the new entry. Null if not applicable.</param>
		/// <param name="useFileSystem">If true get details from the file system if the file exists.</param>
		/// <returns>Returns a <see cref="ZipEntry">file entry</see> based on the <paramref name="fileName"/> passed.</returns>
		ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem);

		/// <summary>
		/// Create a <see cref="ZipEntry"/> for a directory given its name
		/// </summary>
		/// <param name="directoryName">The name of the directory to create an entry for.</param>
		/// <returns>Returns a <see cref="ZipEntry">directory entry</see> based on the <paramref name="directoryName"/> passed.</returns>
		ZipEntry MakeDirectoryEntry(string directoryName);

		/// <summary>
		/// Create a <see cref="ZipEntry"/> for a directory given its name
		/// </summary>
		/// <param name="directoryName">The name of the directory to create an entry for.</param>
		/// <param name="useFileSystem">If true get details from the file system for this directory if it exists.</param>
		/// <returns>Returns a <see cref="ZipEntry">directory entry</see> based on the <paramref name="directoryName"/> passed.</returns>
		ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem);

		/// <summary>
		/// Get/set the <see cref="INameTransform"></see> applicable.
		/// </summary>
		INameTransform NameTransform { get; set;  }
	}
}
