// WindowsPathUtils.cs
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

namespace ICSharpCode.SharpZipLib.Core
{
	/// <summary>
	/// WindowsPathUtils provides simple utilities for handling windows paths.
	/// </summary>
	public abstract class WindowsPathUtils
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsPathUtils"/> class.
		/// </summary>
		internal WindowsPathUtils()
		{
		}

		/// <summary>
		/// Remove any path root present in the path
		/// </summary>
		/// <param name="path">A <see cref="string"/> containing path information.</param>
		/// <returns>The path with the root removed if it was present; path otherwise.</returns>
		/// <remarks>Unlike the <see cref="System.IO.Path"/> class the path isnt otherwise checked for validity.</remarks>
		public static string DropPathRoot(string path)
		{
			string result = path;

			if ( (path != null) && (path.Length > 0) ) {
				if ((path[0] == '\\') || (path[0] == '/')) {
					// UNC name ?
					if ((path.Length > 1) && ((path[1] == '\\') || (path[1] == '/'))) {
						int index = 2;
						int elements = 2;

						// Scan for two separate elements \\machine\share\restofpath
						while ((index <= path.Length) &&
							(((path[index] != '\\') && (path[index] != '/')) || (--elements > 0))) {
							index++;
						}

						index++;

						if (index < path.Length) {
							result = path.Substring(index);
						}
						else {
							result = "";
						}
					}
				}
				else if ((path.Length > 1) && (path[1] == ':')) {
					int dropCount = 2;
					if ((path.Length > 2) && ((path[2] == '\\') || (path[2] == '/'))) {
						dropCount = 3;
					}
					result = result.Remove(0, dropCount);
				}
			}
			return result;
		}
	}
}
