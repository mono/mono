//
// System.IO.IntPtrStream: A stream that is backed up by unmanaged memory
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Based on the code for MemoryStream.cs:
//
// Authors:	Marcin Szczepanski (marcins@zipworld.com.au)
//		Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2001,2002 Marcin Szczepanski
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.IO;

namespace System.Web {

	internal class IntPtrStream : Stream {
		unsafe byte *base_address;
		int size;
		int position;
		bool owns;

		public IntPtrStream (IntPtr base_address, int size)
		{
			unsafe {
				this.base_address = (byte*)((void *)base_address);
			}
			this.size = size;
			owns = true;
		}

		public IntPtrStream (Stream stream)
		{
			IntPtrStream st = (IntPtrStream) stream;
			this.size = st.size;
			unsafe {
				this.base_address = st.base_address;
			}
		}

		protected IntPtr BaseAddress {
			get {
				unsafe  { return (IntPtr) base_address; }
			}
		}

		protected int Size {
			get { return size; }
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return true;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override long Position {
			get {
				return position;
			}

			set {
				if (position < 0)
					throw new ArgumentOutOfRangeException ("Position", "Can not be negative");
				if (position > size)
					throw new ArgumentOutOfRangeException ("Position", "Pointer falls out of range");

				position = (int) value;
			}
		}

		public override long Length {
			get {
				return size;
			}
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("offset or count less than zero.");

			if (buffer.Length - offset < count )
				throw new ArgumentException ("offset+count",
							      "The size of the buffer is less than offset + count.");

			unsafe {
				if (base_address == null)
					throw new ObjectDisposedException ("Stream has been closed");
			}

			if (position >= size || count == 0)
				return 0;

			if (position > size - count)
				count = size - position;

			unsafe {
				Marshal.Copy ((IntPtr) (base_address + position), buffer, offset, count);
			}
			position += count;
			return count;
		}

		public override int ReadByte ()
		{
			if (position >= size)
				return -1;

			unsafe {
				if (base_address == null)
					throw new ObjectDisposedException ("Stream has been closed");
			}

			unsafe {
				return base_address [position++];
			}
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			// It's funny that they don't throw this exception for < Int32.MinValue
			if (offset > (long) Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("Offset out of range. " + offset);

			unsafe {
				if (base_address == null)
					throw new ObjectDisposedException ("Stream has been closed");
			}

			int ref_point;
			switch (loc) {
			case SeekOrigin.Begin:
				if (offset < 0)
					throw new IOException ("Attempted to seek before start of MemoryStream.");
				ref_point = 0;
				break;
			case SeekOrigin.Current:
				ref_point = position;
				break;
			case SeekOrigin.End:
				ref_point = size;
				break;
			default:
				throw new ArgumentException ("loc", "Invalid SeekOrigin");
			}

			checked {
				try {
					ref_point += (int) offset;
				} catch {
					throw new ArgumentOutOfRangeException ("Too large seek destination");
				}
				
				if (ref_point < 0)
					throw new IOException ("Attempted to seek before start of MemoryStream.");
			}

			position = ref_point;
			return position;
		}
		
		public override void SetLength (long value)
		{
			throw new NotSupportedException ("This stream can not change its size");
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			throw new NotSupportedException ("This stream can not change its size");
		}

		public override void WriteByte (byte value)
		{
			throw new NotSupportedException ("This stream can not change its size");
		}
		
		public override void Flush ()
		{
		}

		public override void Close ()
		{
			if (owns) {
				unsafe {
					IntPtr ptr = (IntPtr) base_address;
					if (ptr != IntPtr.Zero)
						Marshal.FreeHGlobal (ptr);
					base_address = null;
				}
			}
		}

		~IntPtrStream ()
		{
			if (owns) {
				unsafe {
					IntPtr ptr = (IntPtr) base_address;
					if (ptr != IntPtr.Zero)
						Marshal.FreeHGlobal (ptr);
					base_address = null;
				}
			}
		}
	}
}

