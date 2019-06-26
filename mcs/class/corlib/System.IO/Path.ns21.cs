using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Diagnostics;

namespace System.IO
{
	partial class Path
	{
		public static ReadOnlySpan<char> GetExtension (ReadOnlySpan<char> path) => GetExtension (path.ToString ()).AsSpan ();

		public static ReadOnlySpan<char> GetFileNameWithoutExtension (ReadOnlySpan<char> path) => GetFileNameWithoutExtension (path.ToString ()).AsSpan ();

		public static ReadOnlySpan<char> GetPathRoot (ReadOnlySpan<char> path) => GetPathRoot (path.ToString ()).AsSpan ();

		public static bool HasExtension (ReadOnlySpan<char> path) => HasExtension (path.ToString ());

		public static string GetRelativePath(string relativeTo, string path)
		{
			return GetRelativePath(relativeTo, path, StringComparison);
		}

		private static string GetRelativePath(string relativeTo, string path, StringComparison comparisonType)
		{
			if (string.IsNullOrEmpty(relativeTo)) throw new ArgumentNullException(nameof(relativeTo));
			if (PathInternal.IsEffectivelyEmpty(path.AsSpan())) throw new ArgumentNullException(nameof(path));
			Debug.Assert(comparisonType == StringComparison.Ordinal || comparisonType == StringComparison.OrdinalIgnoreCase);

			relativeTo = GetFullPath(relativeTo);
			path = GetFullPath(path);

			// Need to check if the roots are different- if they are we need to return the "to" path.
			if (!PathInternal.AreRootsEqual(relativeTo, path, comparisonType))
				return path;

			int commonLength = PathInternal.GetCommonPathLength(relativeTo, path, ignoreCase: comparisonType == StringComparison.OrdinalIgnoreCase);

			// If there is nothing in common they can't share the same root, return the "to" path as is.
			if (commonLength == 0)
				return path;

			// Trailing separators aren't significant for comparison
			int relativeToLength = relativeTo.Length;
			if (PathInternal.EndsInDirectorySeparator(relativeTo.AsSpan()))
				relativeToLength--;

			bool pathEndsInSeparator = PathInternal.EndsInDirectorySeparator(path.AsSpan());
			int pathLength = path.Length;
			if (pathEndsInSeparator)
				pathLength--;

			// If we have effectively the same path, return "."
			if (relativeToLength == pathLength && commonLength >= relativeToLength) return ".";

			// We have the same root, we need to calculate the difference now using the
			// common Length and Segment count past the length.
			//
			// Some examples:
			//
			//  C:\Foo C:\Bar L3, S1 -> ..\Bar
			//  C:\Foo C:\Foo\Bar L6, S0 -> Bar
			//  C:\Foo\Bar C:\Bar\Bar L3, S2 -> ..\..\Bar\Bar
			//  C:\Foo\Foo C:\Foo\Bar L7, S1 -> ..\Bar

			StringBuilder sb = StringBuilderCache.Acquire(Math.Max(relativeTo.Length, path.Length));

			// Add parent segments for segments past the common on the "from" path
			if (commonLength < relativeToLength)
			{
				sb.Append("..");

				for (int i = commonLength + 1; i < relativeToLength; i++)
				{
					if (PathInternal.IsDirectorySeparator(relativeTo[i]))
					{
						sb.Append(DirectorySeparatorChar);
						sb.Append("..");
					}
				}
			}
			else if (PathInternal.IsDirectorySeparator(path[commonLength]))
			{
				// No parent segments and we need to eat the initial separator
				//  (C:\Foo C:\Foo\Bar case)
				commonLength++;
			}

			// Now add the rest of the "to" path, adding back the trailing separator
			int differenceLength = pathLength - commonLength;
			if (pathEndsInSeparator)
				differenceLength++;

			if (differenceLength > 0)
			{
				if (sb.Length > 0)
				{
					sb.Append(DirectorySeparatorChar);
				}

				sb.Append(path, commonLength, differenceLength);
			}

			return StringBuilderCache.GetStringAndRelease(sb);
		}

		/// <summary>Returns a comparison that can be used to compare file and directory names for equality.</summary>
		internal static StringComparison StringComparison
		{
			get
			{
				return IsCaseSensitive ?
					StringComparison.Ordinal :
					StringComparison.OrdinalIgnoreCase;
			}
		}

		internal static bool IsCaseSensitive => !IsWindows;

		static bool IsWindows
		{
			get
			{
				PlatformID platform = Environment.OSVersion.Platform;
				if (platform == PlatformID.Win32S ||
					platform == PlatformID.Win32Windows ||
					platform == PlatformID.Win32NT ||
					platform == PlatformID.WinCE) {
					return true;
				}
				return false;
			}
		}

		public static bool IsPathFullyQualified(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			return IsPathFullyQualified(path.AsSpan());
		}

		public static bool IsPathFullyQualified(ReadOnlySpan<char> path)
		{
			return !PathInternal.IsPartiallyQualified(path);
		}

		public static string GetFullPath(string path, string basePath)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (basePath == null)
				throw new ArgumentNullException(nameof(basePath));

			if (!IsPathFullyQualified(basePath))
				throw new ArgumentException(SR.Arg_BasePathNotFullyQualified, nameof(basePath));

			if (basePath.Contains('\0') || path.Contains('\0'))
				throw new ArgumentException(SR.Argument_InvalidPathChars);

			if (IsPathFullyQualified(path))
				return GetFullPath(path);

			return GetFullPath(CombineInternal(basePath, path));
		}

		private static string CombineInternal(string first, string second)
		{
			if (string.IsNullOrEmpty(first))
				return second;

			if (string.IsNullOrEmpty(second))
				return first;

			if (IsPathRooted(second.AsSpan()))
				return second;

			return JoinInternal(first.AsSpan(), second.AsSpan());
		}
	}
}
