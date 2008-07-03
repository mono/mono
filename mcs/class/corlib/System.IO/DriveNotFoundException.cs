//
// System.IO.DriveNotFoundException.cs
//
// Author:
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2006 Kornél Pál
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

#if NET_2_0
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO
{
	[Serializable]
	[ComVisible (true)]
	public class DriveNotFoundException : IOException
	{
		private const int ErrorCode = unchecked((int)0x80070003);

		// Constructors
		public DriveNotFoundException ()
			: base ("Attempted to access a drive that is not available.")
		{
			this.HResult = ErrorCode;
		}

		public DriveNotFoundException (string message)
			: base (message)
		{
			this.HResult = ErrorCode;
		}

		public DriveNotFoundException (string message, Exception innerException)
			: base (message, innerException)
		{
			this.HResult = ErrorCode;
		}

		protected DriveNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
#endif
