#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace NUnit.Util
{
	/// <summary>
	/// Static methods for manipulating project paths, including both directories
	/// and files. Some synonyms for System.Path methods are included as well.
	/// </summary>
	public class ProjectPath
	{
		public const uint FILE_ATTRIBUTE_DIRECTORY  = 0x00000010;  
		public const uint FILE_ATTRIBUTE_NORMAL     = 0x00000080;  
		public const int MAX_PATH = 256;

		#region Public methods

		public static bool IsAssemblyFileType( string path )
		{
			string extension = Path.GetExtension( path );
			return extension == ".dll" || extension == ".exe";
		}

		/// <summary>
		/// Returns the relative path from a base directory to another
		/// directory or file.
		/// </summary>
		public static string RelativePath( string from, string to )
		{
			from = Canonicalize( from );
			to = Canonicalize( to );

			// Second argument to PathRelativeTo must be absolute
			if ( !Path.IsPathRooted( to ) )
				return to;
			
			StringBuilder sb = new StringBuilder( MAX_PATH );

			// Return null if call fails
			if ( !PathRelativePathTo( sb, from, FILE_ATTRIBUTE_DIRECTORY, to, FILE_ATTRIBUTE_DIRECTORY ) )
				return null;

			// Remove initial .\ from path if present
			if ( sb.Length >=2 && sb[0] == '.' && sb[1] == '\\' )
				sb.Remove( 0, 2 );

			if ( sb.Length == 0 )
				return null;

			return sb.ToString();
		}

		/// <summary>
		/// Return the canonical form of a path.
		public static string Canonicalize( string path )
		{
			StringBuilder sb = new StringBuilder( MAX_PATH );
			if ( !PathCanonicalize( sb, path ) )
				throw new ArgumentException( string.Format( "Invalid path passed to PathCanonicalize: {0}", path ) );

			return sb.ToString();
		}

		/// <summary>
		/// True if the two paths are the same. However, two paths
		/// to the same file or directory using different network
		/// shares or drive letters are not treated as equal.
		/// </summary>
		public static bool SamePath( string path1, string path2 )
		{
			return Canonicalize(path1).ToLower() == Canonicalize(path2).ToLower();
		}

		/// <summary>
		/// True if the two paths are the same or if the second is
		/// directly or indirectly under the first. Note that paths 
		/// using different network shares or drive letters are 
		/// considered unrelated, even if they end up referencing
		/// the same subtrees in the file system.
		/// </summary>
		public static bool SamePathOrUnder( string path1, string path2 )
		{
			path1 = Canonicalize( path1 );
			path2 = Canonicalize( path2 );

			int length1 = path1.Length;
			int length2 = path2.Length;

			// if path1 is longer, then path2 can't be under it
			if ( length1 > length2 )
				return false;

			// if lengths are the same, check for equality
			if ( length1 == length2 )
				return path1.ToLower() == path2.ToLower();

			// path 2 is longer than path 1: see if initial parts match
			if ( path1.ToLower() != path2.Substring( 0, length1 ).ToLower() )
				return false;
			
			// must match through or up to a directory separator boundary
			return	path2[length1-1] == Path.DirectorySeparatorChar ||
					path2[length1] == Path.DirectorySeparatorChar;
		}

		#endregion

		#region Shlwapi functions used internally

		[DllImport("shlwapi.dll")]
		private static extern bool PathRelativePathTo(
			StringBuilder result,
			string from,
			uint attrFrom,
			string to,
			uint attrTo );

		[DllImport("shlwapi.dll")]
		private static extern bool PathCanonicalize(
			StringBuilder result,
			string path );

		[DllImport("shlwapi.dll")]
		private static extern int PathCommonPrefix(
			string file1,
			string file2,
			StringBuilder result );
			
		#endregion
	}
}
