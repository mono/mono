//
// System.Web.ReusableMemoryStream
// Trimmed down copy of System.IO.MemoryStream used by HttpWriter.
//
// Authors:	Marcin Szczepanski (marcins@zipworld.com.au)
//		Patrik Torstensson
//		Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2001,2002 Marcin Szczepanski, Patrik Torstensson
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System.IO;
using System.Runtime.InteropServices;

namespace System.Web
{
	[Serializable]
	class ReusableMemoryStream : Stream
	{
		int capacity;
		int length;
		byte [] internalBuffer;
		bool streamClosed;
		int position;

		public ReusableMemoryStream (int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");

			this.capacity = capacity;
			internalBuffer = new byte [capacity];
		}

		public ReusableMemoryStream (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			
			capacity = buffer.Length;
			internalBuffer = buffer;
		}

		void CheckIfClosedThrowDisposed ()
		{
			if (streamClosed)
				throw new ObjectDisposedException (GetType ().ToString ());
		}
		
		public override bool CanRead {
			get { return !streamClosed; }
		}

		public override bool CanSeek {
			get { return !streamClosed; }
		}

		public override bool CanWrite {
			get { return !streamClosed; }
		}

		int Capacity {
			get {
				CheckIfClosedThrowDisposed ();
				return capacity;
			}

			set {
				CheckIfClosedThrowDisposed ();
				if (value == capacity)
					return; // LAMENESS: see MemoryStreamTest.ConstructorFive

				if (value < 0 || value < length)
					throw new ArgumentOutOfRangeException ("value",
					"New capacity cannot be negative or less than the current capacity " + value + " " + capacity);

				byte [] newBuffer = null;
				if (value != 0) {
					newBuffer = new byte [value];
					Buffer.BlockCopy (internalBuffer, 0, newBuffer, 0, length);
				}

				internalBuffer = newBuffer; // It's null when capacity is set to 0
				capacity = value;
			}
		}

		public override long Length {
			get {
				CheckIfClosedThrowDisposed ();
				return length;
			}
		}

		public override long Position {
			get {
				CheckIfClosedThrowDisposed ();
				return position;
			}

			set {
				CheckIfClosedThrowDisposed ();
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value",
								"Position cannot be negative" );

				if (value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("value",
					"Position must be non-negative and less than 2^31 - 1 - origin");

				position = (int) value;
			}
		}

		public override void Close ()
		{
			streamClosed = true;
		}

		public override void Flush ()
		{
		}

		public byte [] GetBuffer ()
		{
			return internalBuffer;
		}

		public override int Read ([In,Out] byte [] buffer, int offset, int count)
		{
			CheckIfClosedThrowDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (position >= length || count == 0)
				return 0;

			if (position > length - count)
				count = length - position;

			Buffer.BlockCopy (internalBuffer, position, buffer, offset, count);
			position += count;
			return count;
		}

		public override int ReadByte ()
		{
			CheckIfClosedThrowDisposed ();
			if (position >= length)
				return -1;

			return internalBuffer [position++];
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			CheckIfClosedThrowDisposed ();

			if (offset > (long) Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("Offset out of range. " + offset);

			int refPoint;
			switch (loc) {
			case SeekOrigin.Begin:
				if (offset < 0)
					throw new IOException ("Attempted to seek before start of MemoryStream.");
				refPoint = 0;
				break;
			case SeekOrigin.Current:
				refPoint = position;
				break;
			case SeekOrigin.End:
				refPoint = length;
				break;
			default:
				throw new ArgumentException ("loc", "Invalid SeekOrigin");
			}

			refPoint += (int) offset;
			if (refPoint < 0)
				throw new IOException ("Attempted to seek before start of MemoryStream.");

			position = refPoint;
			return position;
		}

		int CalculateNewCapacity (int minimum)
		{
			if (minimum < 256)
				minimum = 256;

			if (minimum < capacity * 2)
				minimum = capacity * 2;

			return minimum;
		}

		public override void SetLength (long value)
		{
			CheckIfClosedThrowDisposed ();

			if (value < 0 || value > (long) Int32.MaxValue)
				throw new ArgumentOutOfRangeException ();

			int newSize = (int) value;
			if (newSize > capacity)
				Capacity = CalculateNewCapacity (newSize);
			else if (newSize < length)
				Array.Clear (internalBuffer, newSize, length - newSize);

			length = newSize;
			if (position > length)
				position = length;
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			CheckIfClosedThrowDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (buffer.Length - offset < count)
				throw new ArgumentException ("offset+count",
							     "The size of the buffer is less than offset + count.");

			// reordered to avoid possible integer overflow
			if (position > capacity - count)
				Capacity = CalculateNewCapacity (position + count);

			Buffer.BlockCopy (buffer, offset, internalBuffer, position, count);
			position += count;
			if (position >= length)
				length = position;
		}

		public override void WriteByte (byte value)
		{
			CheckIfClosedThrowDisposed ();
			if (position >= capacity)
				Capacity = CalculateNewCapacity (position + 1);

			if (position >= length)
				length = position + 1;

			internalBuffer [position++] = value;
		}
	}               
}

