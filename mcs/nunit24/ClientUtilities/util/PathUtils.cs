// ****************************************************************
// Copyright 2002-2003, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;

namespace NUnit.Util
{
	/// <summary>
	/// Static methods for manipulating project paths, including both directories
	/// and files. Some synonyms for System.Path methods are included as well.
	/// </summary> 
	public class PathUtils
	{
		public const uint FILE_ATTRIBUTE_DIRECTORY  = 0x00000010;  
		public const uint FILE_ATTRIBUTE_NORMAL     = 0x00000080;  
		public const int MAX_PATH = 256;

		protected static char DirectorySeparatorChar = Path.DirectorySeparatorChar;
		protected static char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

		#region Public methods

		public static bool IsAssemblyFileType( string path )
		{
			string extension = Path.GetExtension( path ).ToLower();
			return extension == ".dll" || extension == ".exe";
		}

		/// <summary>
		/// Returns the relative path from a base directory to another
		/// directory or file.
		/// </summary>
		public static string RelativePath( string from, string to )
		{
			if (from == null)
				throw new ArgumentNullException (from);
			if (to == null)
				throw new ArgumentNullException (to);
			if (!Path.IsPathRooted (to))
				return to;
			if (Path.GetPathRoot (from) != Path.GetPathRoot (to))
				return null;

			string[] _from = from.Split (PathUtils.DirectorySeparatorChar, 
				PathUtils.AltDirectorySeparatorChar);
			string[] _to   =   to.Split (PathUtils.DirectorySeparatorChar, 
				PathUtils.AltDirectorySeparatorChar);

			StringBuilder sb = new StringBuilder (Math.Max (from.Length, to.Length));

			int last_common, min = Math.Min (_from.Length, _to.Length);
			for (last_common = 0; last_common < min;  ++last_common) 
			{
				if (!_from [last_common].Equals (_to [last_common]))
					break;
			}

			if (last_common < _from.Length)
				sb.Append ("..");
			for (int i = last_common + 1; i < _from.Length; ++i) 
			{
				sb.Append (PathUtils.DirectorySeparatorChar).Append ("..");
			}

			if (sb.Length > 0)
				sb.Append (PathUtils.DirectorySeparatorChar);
			if (last_common < _to.Length)
				sb.Append (_to [last_common]);
			for (int i = last_common + 1; i < _to.Length; ++i) 
			{
				sb.Append (PathUtils.DirectorySeparatorChar).Append (_to [i]);
			}

			return sb.ToString ();
		}

		/// <summary>
		/// Return the canonical form of a path.
		/// </summary>
		public static string Canonicalize( string path )
		{
			ArrayList parts = new ArrayList(
				path.Split( DirectorySeparatorChar, AltDirectorySeparatorChar ) );

			for( int index = 0; index < parts.Count; )
			{
				string part = (string)parts[index];
		
				switch( part )
				{
					case ".":
						parts.RemoveAt( index );
						break;
				
					case "..":
						parts.RemoveAt( index );
						if ( index > 0 )
							parts.RemoveAt( --index );
						break;
					default:
						index++;
						break;
				}
			}
	
			return String.Join( DirectorySeparatorChar.ToString(), (string[])parts.ToArray( typeof( string ) ) );
		}

		/// <summary>
		/// True if the two paths are the same. However, two paths
		/// to the same file or directory using different network
		/// shares or drive letters are not treated as equal.
		/// </summary>
		public static bool SamePath( string path1, string path2 )
		{
			return string.Compare( Canonicalize(path1), Canonicalize(path2), PathUtils.IsWindows() ) == 0;
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
				//return path1.ToLower() == path2.ToLower();
				return string.Compare( path1, path2, IsWindows() ) == 0;

			// path 2 is longer than path 1: see if initial parts match
			//if ( path1.ToLower() != path2.Substring( 0, length1 ).ToLower() )
			if ( string.Compare( path1, path2.Substring( 0, length1 ), IsWindows() ) != 0 )
				return false;
			
			// must match through or up to a directory separator boundary
			return	path2[length1-1] == DirectorySeparatorChar ||
				path2[length1] == DirectorySeparatorChar;
		}

		public static string Combine( string path1, params string[] morePaths )
		{
			string result = path1;
			foreach( string path in morePaths )
				result = Path.Combine( result, path );
			return result;
		}

		// TODO: This logic should be in shared source
		public static string GetAssemblyPath( Assembly assembly )
		{
			string uri = assembly.CodeBase;

			// If it wasn't loaded locally, use the Location
			if ( !uri.StartsWith( Uri.UriSchemeFile ) )
				return assembly.Location;

			return GetAssemblyPathFromFileUri( uri );
		}

		// Separate method for testability
		public static string GetAssemblyPathFromFileUri( string uri )
		{
			// Skip over the file://
			int start = Uri.UriSchemeFile.Length + Uri.SchemeDelimiter.Length;
			
			if ( PathUtils.DirectorySeparatorChar == '\\' )
			{
				if ( uri[start] == '/' && uri[start+2] == ':' )
					++start;
			}
			else
			{
				if ( uri[start] != '/' )
					--start;
			}

			return uri.Substring( start );
		}
		#endregion

		#region Helper Methods
		private static bool IsWindows()
		{
			return PathUtils.DirectorySeparatorChar == '\\';
		}
		#endregion
	}
}
