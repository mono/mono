//
// SortVersion.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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

#if NET_4_5

using System.Collections.Generic;

namespace System.Globalization
{
	[Serializable]
	public sealed class SortVersion : IEquatable<SortVersion>
	{
		public SortVersion (int fullVersion, Guid sortId)
		{
			FullVersion = fullVersion;
			SortId = sortId;
		}

		public Guid SortId { get; private set; }

		public int FullVersion { get; private set; }

		public override bool Equals (object obj)
		{
			return Equals (obj as SortVersion);
		}

		public override int GetHashCode ()
		{
			return FullVersion.GetHashCode ();
		}

		public bool Equals (SortVersion other)
		{
			if (other == null)
				return false;

			return FullVersion == other.FullVersion && SortId == other.SortId;
		}

		public static bool operator == (SortVersion left, SortVersion right)
		{
			if (left != null)
				return left.Equals (right);

			return right == null || right.Equals (left);
		}

		public static bool operator != (SortVersion left, SortVersion right)
		{
			return !(left == right);
		}
	}
}

#endif