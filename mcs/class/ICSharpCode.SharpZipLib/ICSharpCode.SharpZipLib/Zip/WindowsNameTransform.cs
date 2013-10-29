// WindowsNameTransform.cs
//
// Copyright 2007 John Reilly
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

using System;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// WindowsNameTransform transforms <see cref="ZipFile"/> names to windows compatible ones.
	/// </summary>
	public class WindowsNameTransform : INameTransform
	{
		/// <summary>
		/// Initialises a new instance of <see cref="WindowsNameTransform"/>
		/// </summary>
		/// <param name="baseDirectory"></param>
		public WindowsNameTransform(string baseDirectory)
		{
			if ( baseDirectory == null ) {
				throw new ArgumentNullException("baseDirectory", "Directory name is invalid");
			}

			BaseDirectory = baseDirectory;
		}

		/// <summary>
		/// Initialise a default instance of <see cref="WindowsNameTransform"/>
		/// </summary>
		public WindowsNameTransform()
		{
			// Do nothing.
		}

		/// <summary>
		/// Gets or sets a value containing the target directory to prefix values with.
		/// </summary>
		public string BaseDirectory
		{
			get { return _baseDirectory; }
			set {
				if ( value == null ) {
					throw new ArgumentNullException("value");
				}

				_baseDirectory = Path.GetFullPath(value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating wether paths on incoming values should be removed.
		/// </summary>
		public bool TrimIncomingPaths
		{
			get { return _trimIncomingPaths; }
			set { _trimIncomingPaths = value; }
		}

		/// <summary>
		/// Transform a Zip directory name to a windows directory name.
		/// </summary>
		/// <param name="name">The directory name to transform.</param>
		/// <returns>The transformed name.</returns>
		public string TransformDirectory(string name)
		{
			name = TransformFile(name);
			if (name.Length > 0) {
				while ( name.EndsWith(@"\") ) {
					name = name.Remove(name.Length - 1, 1);
				}
			}
			else {
				throw new ZipException("Cannot have an empty directory name");
			}
			return name;
		}

		/// <summary>
		/// Transform a Zip format file name to a windows style one.
		/// </summary>
		/// <param name="name">The file name to transform.</param>
		/// <returns>The transformed name.</returns>
		public string TransformFile(string name)
		{
			if (name != null) {
				name = MakeValidName(name, _replacementChar);

				if ( _trimIncomingPaths ) {
					name = Path.GetFileName(name);
				}

				// This may exceed windows length restrictions.
				// Combine will throw a PathTooLongException in that case.
				if ( _baseDirectory != null ) {
					name = Path.Combine(_baseDirectory, name);
				}
			}
			else {
				name = string.Empty;
			}
			return name;
		}

		/// <summary>
		/// Test a name to see if it is a valid name for a windows filename as extracted from a Zip archive.
		/// </summary>
		/// <param name="name">The name to test.</param>
		/// <returns>Returns true if the name is a valid zip name; false otherwise.</returns>
		/// <remarks>The filename isnt a true windows path in some fundamental ways like no absolute paths, no rooted paths etc.</remarks>
		public static bool IsValidName(string name)
		{
			bool result =
				(name != null) &&
				(name.Length <= MaxPath) &&
				(string.Compare(name, MakeValidName(name, '_')) == 0)
				;

			return result;
		}

		/// <summary>
		/// Initialise static class information.
		/// </summary>
		static WindowsNameTransform()
		{
			char[] invalidPathChars;

#if NET_1_0 || NET_1_1 || NETCF_1_0
			invalidPathChars = Path.InvalidPathChars;
#else
			invalidPathChars = Path.GetInvalidPathChars();
#endif
			int howMany = invalidPathChars.Length + 3;

			InvalidEntryChars = new char[howMany];
			Array.Copy(invalidPathChars, 0, InvalidEntryChars, 0, invalidPathChars.Length);
			InvalidEntryChars[howMany - 1] = '*';
			InvalidEntryChars[howMany - 2] = '?';
			InvalidEntryChars[howMany - 3] = ':';
		}

		/// <summary>
		/// Force a name to be valid by replacing invalid characters with a fixed value
		/// </summary>
		/// <param name="name">The name to make valid</param>
		/// <param name="replacement">The replacement character to use for any invalid characters.</param>
		/// <returns>Returns a valid name</returns>
		public static string MakeValidName(string name, char replacement)
		{
			if ( name == null ) {
				throw new ArgumentNullException("name");
			}

			name = WindowsPathUtils.DropPathRoot(name.Replace("/", @"\"));

			// Drop any leading slashes.
			while ( (name.Length > 0) && (name[0] == '\\')) {
				name = name.Remove(0, 1);
			}

			// Drop any trailing slashes.
			while ( (name.Length > 0) && (name[name.Length - 1] == '\\')) {
				name = name.Remove(name.Length - 1, 1);
			}

			// Convert consecutive \\ characters to \
			int index = name.IndexOf(@"\\");
			while (index >= 0) {
				name = name.Remove(index, 1);
				index = name.IndexOf(@"\\");
			}

			// Convert any invalid characters using the replacement one.
			index = name.IndexOfAny(InvalidEntryChars);
			if (index >= 0) {
				StringBuilder builder = new StringBuilder(name);

				while (index >= 0 ) {
					builder[index] = replacement;

					if (index >= name.Length) {
						index = -1;
					}
					else {
						index = name.IndexOfAny(InvalidEntryChars, index + 1);
					}
				}
				name = builder.ToString();
			}

			// Check for names greater than MaxPath characters.
			// TODO: Were is CLR version of MaxPath defined?  Can't find it in Environment.
			if ( name.Length > MaxPath ) {
				throw new PathTooLongException();
			}

			return name;
		}

		/// <summary>
		/// Gets or set the character to replace invalid characters during transformations.
		/// </summary>
		public char Replacement
		{
			get { return _replacementChar; }
			set {
				for ( int i = 0; i < InvalidEntryChars.Length; ++i ) {
					if ( InvalidEntryChars[i] == value ) {
						throw new ArgumentException("invalid path character");
					}
				}

				if ((value == '\\') || (value == '/')) {
					throw new ArgumentException("invalid replacement character");
				}

				_replacementChar = value;
			}
		}

		/// <summary>
		///  The maximum windows path name permitted.
		/// </summary>
		/// <remarks>This may not valid for all windows systems - CE?, etc but I cant find the equivalent in the CLR.</remarks>
		const int MaxPath = 260;

		#region Instance Fields
		string _baseDirectory;
		bool _trimIncomingPaths;
		char _replacementChar = '_';
		#endregion

		#region Class Fields
		static readonly char[] InvalidEntryChars;
		#endregion
	}
}
