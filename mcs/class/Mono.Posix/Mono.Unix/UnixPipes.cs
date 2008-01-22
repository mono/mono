//
// Mono.Unix/UnixPipes.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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
using System.IO;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public struct UnixPipes
#if NET_2_0
		: IEquatable <UnixPipes>
#endif
	{
		public UnixPipes (UnixStream reading, UnixStream writing)
		{
			Reading = reading;
			Writing = writing;
		}

		public UnixStream Reading;
		public UnixStream Writing;

		public static UnixPipes CreatePipes ()
		{
			int reading, writing;
			int r = Native.Syscall.pipe (out reading, out writing);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return new UnixPipes (new UnixStream (reading), new UnixStream (writing));
		}

		public override bool Equals (object value)
		{
			if ((value == null) || (value.GetType () != GetType ()))
				return false;
			UnixPipes other = (UnixPipes) value;
			return Reading.Handle == other.Reading.Handle &&
				Writing.Handle == other.Writing.Handle;
		}

		public bool Equals (UnixPipes value)
		{
			return Reading.Handle == value.Reading.Handle &&
				Writing.Handle == value.Writing.Handle;
		}

		public override int GetHashCode ()
		{
			return Reading.Handle.GetHashCode () ^ Writing.Handle.GetHashCode ();
		}

		public static bool operator== (UnixPipes lhs, UnixPipes rhs)
		{
			return lhs.Equals (rhs);
		}

		public static bool operator!= (UnixPipes lhs, UnixPipes rhs)
		{
			return !lhs.Equals (rhs);
		}
	}
}

// vim: noexpandtab
