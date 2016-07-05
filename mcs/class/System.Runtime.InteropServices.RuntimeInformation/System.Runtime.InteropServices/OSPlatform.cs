//
// OSPlatform.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
//

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

namespace System.Runtime.InteropServices
{
	public struct OSPlatform : IEquatable<OSPlatform>
	{
		private readonly string _osPlatform;

		public static OSPlatform Linux { get; } = new OSPlatform ("LINUX");

		public static OSPlatform OSX { get; } = new OSPlatform ("OSX");

		public static OSPlatform Windows { get; } = new OSPlatform ("WINDOWS");

		private OSPlatform (string osPlatform)
		{
			if (osPlatform == null) throw new ArgumentNullException (nameof (osPlatform));
			if (osPlatform.Length == 0) throw new ArgumentException ("Value cannot be empty.", nameof (osPlatform));
			
			_osPlatform = osPlatform;
		}

		public static OSPlatform Create (string osPlatform)
		{
			return new OSPlatform (osPlatform);
		}

		public bool Equals (OSPlatform other)
		{
			return Equals (other._osPlatform);
		}

		internal bool Equals (string other)
		{
			return string.Equals (_osPlatform, other, StringComparison.Ordinal);
		}

		public override bool Equals (object obj)
		{
			return obj is OSPlatform && Equals ((OSPlatform)obj);
		}

		public override int GetHashCode ()
		{
			return _osPlatform == null ? 0 : _osPlatform.GetHashCode ();
		}

		public override string ToString ()
		{
			return _osPlatform ?? string.Empty;
		}

		public static bool operator ==(OSPlatform left, OSPlatform right)
		{
			return left.Equals (right);
		}

		public static bool operator !=(OSPlatform left, OSPlatform right)
		{
			return !(left == right);
		}
	}
}