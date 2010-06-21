//
// System.TypeAccessException.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2010 Novell, Inc.  http://www.novell.com
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
#if NET_4_0 || MOONLIGHT
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible (true)]
	public class TypeAccessException : TypeLoadException
	{
		const int Result = unchecked ((int)0x80131522); // FIXME: this code is probably wrong

		// Constructors
		public TypeAccessException ()
			: base (Locale.GetText ("Attempt to access the type failed."))
		{
			HResult = Result;
		}

		public TypeAccessException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public TypeAccessException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected TypeAccessException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
#endif

