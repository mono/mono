//
// System.IO.UnmanagedMemoryStream.cs
//
// Copyright (C) 2006 Sridhar Kulkarni, All Rights Reserved
//
// Authors:
// 	Sridhar Kulkarni (sridharkulkarni@gmail.com)
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.IO
{
	[CLSCompliantAttribute(false)]
	public class UnmanagedMemoryStream : Stream
	{
		long length;
		bool closed;
//		bool canseek;
		long capacity;
		FileAccess fileaccess;
		IntPtr initial_pointer;
		long initial_position;
		long current_position;
		
		internal event EventHandler Closed;
		
#region Constructor
		protected UnmanagedMemoryStream()
		{
			fileaccess = FileAccess.Read;
//			canseek = true;
		}
		
		public unsafe UnmanagedMemoryStream (byte *pointer, long length)
			: this ()
		{
			if (pointer == null)
				throw new ArgumentNullException("pointer");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length", "Non-negative number required.");

			this.length = length;
			capacity = length;
			initial_pointer = new IntPtr((void*)pointer);
		}
		
		public unsafe UnmanagedMemoryStream (byte *pointer, long length, long capacity, FileAccess access)
		{
			if (pointer == null)
				throw new ArgumentNullException("pointer");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity", "Non-negative number required.");
			if (length > capacity)
				throw new ArgumentOutOfRangeException("length", "The length cannot be greater than the capacity.");
			if (!Enum.IsDefined (typeof (FileAccess), access))
				throw new ArgumentOutOfRangeException ("access", "Enum value was out of legal range.");
				
			fileaccess = access;
			this.length = length;
			this.capacity = capacity;
//			canseek = true;
			initial_pointer = new IntPtr ((void*)pointer);
		}
#endregion
	
#region Properties
		public override bool CanRead {
			get {
				if (closed)
					return false;
				return (fileaccess == FileAccess.Read || fileaccess == FileAccess.ReadWrite);
			}
		}

		public override bool CanSeek {
			get {
				return !closed;
			}
		}
		
		public override bool CanWrite {
			get {
				if (closed)
					return (false);
				return (fileaccess == FileAccess.Write || fileaccess == FileAccess.ReadWrite);
			}
		}
		public long Capacity {
			get {
				if (closed)
					throw new ObjectDisposedException("The stream is closed");
				else
					return (capacity);
			}
		}
		public override long Length {
			get {
				if (closed)
					throw new ObjectDisposedException("The stream is closed");
				else
					return (length);
			}
		}
		public override long Position {
			get {
				if (closed)
					throw new ObjectDisposedException("The stream is closed");
				return (current_position);
			}
			set {
				if (closed)
					throw new ObjectDisposedException("The stream is closed");
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
				if (value > (long)Int32.MaxValue)
					throw new ArgumentOutOfRangeException("value", "The position is larger than Int32.MaxValue.");
				current_position = value;
			}
		}

#if NET_2_1
		[CLSCompliantAttribute(false)]
#endif
		public unsafe byte* PositionPointer {
			get {
				return (byte *) initial_pointer + current_position;
			}
			set {
				if (value < (byte *)initial_pointer)
					throw new IOException ("Address is below the inital address");

				if (value >= (byte *) ((byte *)initial_pointer + length))
					throw new ArgumentOutOfRangeException ("value");

				current_position = (long) (value - (byte *) initial_pointer);
			}
		}
#endregion
		
#region Methods
		public override int Read ([InAttribute] [OutAttribute] byte[] buffer, int offset, int count)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
			if ((buffer.Length - offset) < count)
				throw new ArgumentException("The length of the buffer array minus the offset parameter is less than the count parameter");
			
			if (fileaccess == FileAccess.Write)
				throw new NotSupportedException("Stream does not support reading");
			else {
				if (current_position >= length)
					return (0);
				else {
					int progress = current_position + count < length ? count : (int) (length - current_position);
					for (int i = 0; i < progress; i++)
						buffer [offset + i] = Marshal.ReadByte (initial_pointer, (int) current_position++);
					return progress;
				}
			}
		}

		public override int ReadByte ()
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			
			if (fileaccess== FileAccess.Write)
				throw new NotSupportedException("Stream does not support reading");
			else {
				if (current_position >= length)
					return (-1);
				return (int) Marshal.ReadByte(initial_pointer, (int) current_position++);
			}
		}
		
		public override long Seek (long offset,	SeekOrigin loc)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");

			long refpoint;
			switch(loc) {
			case SeekOrigin.Begin:
				if (offset < 0)
					throw new IOException("An attempt was made to seek before the beginning of the stream");
				refpoint = initial_position;
				break;
			case SeekOrigin.Current:
				refpoint = current_position;
				break;
			case SeekOrigin.End:
				refpoint = length;
				break;
			default:
				throw new ArgumentException("Invalid SeekOrigin option");
			}
			refpoint += (int)offset;
			if (refpoint < initial_position)
				throw new IOException("An attempt was made to seek before the beginning of the stream");
			current_position = refpoint;
			return(current_position);
		}
		 
		public override void SetLength (long value)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			if (value < 0)
				throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
			if (value > capacity)
				throw new IOException ("Unable to expand length of this stream beyond its capacity.");
			if (fileaccess == FileAccess.Read)
				throw new NotSupportedException ("Stream does not support writing.");
			length = value;
			if (length < current_position)
				current_position = length;
		}

		public override void Flush ()
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			//This method performs no action for this class
			//but is included as part of the Stream base class
		}
		 
		protected override void Dispose (bool disposing)
		{
			if (closed)
				return;
			closed = true;
			if (Closed != null)
				Closed (this, null);
		}
		 
		public override void Write (byte[] buffer, int offset, int count)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			if (buffer == null)
				throw new ArgumentNullException("The buffer parameter is a null reference");
			if ((current_position + count) > capacity)
				throw new NotSupportedException ("Unable to expand length of this stream beyond its capacity.");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
			if ((buffer.Length - offset) < count)
				throw new ArgumentException("The length of the buffer array minus the offset parameter is less than the count parameter");
			if (fileaccess == FileAccess.Read)
				throw new NotSupportedException ("Stream does not support writing.");
			else {
				unsafe {
					// use Marshal.WriteByte since that allow us to start writing
					// from the current position
					for (int i = 0; i < count; i++)
						Marshal.WriteByte (initial_pointer, (int) current_position++, buffer [offset + i]);

					if (current_position > length)
						length = current_position;
				}
			}
		}
		
		public override void WriteByte (byte value)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			
			if (current_position == capacity)
				throw new NotSupportedException("The current position is at the end of the capacity of the stream");
			if (fileaccess == FileAccess.Read)
				throw new NotSupportedException("Stream does not support writing.");
			else {
				unsafe {
					Marshal.WriteByte(initial_pointer, (int)current_position, value);
					current_position++;
					if (current_position > length)
						length = current_position;
				}
			}
		}

		protected unsafe void Initialize (byte* pointer, long length,
						  long capacity,
						  FileAccess access)
		{
			fileaccess = access;
			this.length = length;
			this.capacity = capacity;
			initial_position = 0;
			current_position = initial_position;
//			canseek = true;
			initial_pointer = new IntPtr ((void *)pointer);
			closed = false;
		}
#endregion
	}
}
#endif

