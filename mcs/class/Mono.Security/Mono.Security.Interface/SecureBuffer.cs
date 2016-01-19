//
// SecureBuffer.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014-2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Mono.Security.Interface
{
	public class SecureBuffer : SecretParameters, IBufferOffsetSize
	{
		byte[] buffer;

		public byte[] Buffer {
			get {
				CheckDisposed ();
				return buffer;
			}
		}

		public int Size {
			get {
				CheckDisposed ();
				return buffer != null ? buffer.Length : 0;
			}
		}

		int IBufferOffsetSize.Offset {
			get { return 0; }
		}

		public SecureBuffer (int size)
		{
			buffer = new byte [size];
		}

		public SecureBuffer (byte[] buffer)
		{
			this.buffer = buffer;
		}

		public byte[] StealBuffer ()
		{
			CheckDisposed ();
			var retval = this.buffer;
			this.buffer = null;
			return retval;
		}

		public static SecureBuffer CreateCopy (byte[] buffer)
		{
			var copy = new byte [buffer.Length];
			Array.Copy (buffer, copy, buffer.Length);
			return new SecureBuffer (copy);
		}

		protected override void Clear ()
		{
			if (buffer != null) {
				Array.Clear (buffer, 0, buffer.Length);
				buffer = null;
			}
		}
	}
}

