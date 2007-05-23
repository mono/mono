//------------------------------------------------------------------------------
//
// System.IO.UnmanagedMemoryStream.cs
//
// Copyright (C) 2006 Sridhar Kulkarni, All Rights Reserved
//
// Author:         Sridhar Kulkarni (sridharkulkarni@gmail.com)
// Created:        Monday, July 10, 2006
//
//------------------------------------------------------------------------------

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
		bool canseek = false;
		long capacity;
		FileAccess fileaccess;
		IntPtr initial_pointer;
		IntPtr pointer_position;
		long initial_position;
		long current_position;
		
#region Constructor
		protected UnmanagedMemoryStream()
		{
			fileaccess = FileAccess.Read;
			initial_position = 0;
			canseek = true;
			closed = false;
			current_position = initial_position;
		}
		
		public unsafe UnmanagedMemoryStream (byte *pointer, long len)
		{
			if (pointer == null)
				throw new ArgumentNullException("The pointer value is a null reference ");
			if (len < 0)
				throw new ArgumentOutOfRangeException("The length value is less than zero");
			fileaccess = FileAccess.Read;
			length = len;
			capacity = len;
			initial_position = 0;
			current_position = initial_position;
			canseek = true;
			closed = false;
			initial_pointer = new IntPtr((void*)pointer);
		}
		
		public unsafe UnmanagedMemoryStream (byte *pointer, long len, long capacity, FileAccess access)
		{
			if (pointer == null)
				throw new ArgumentNullException("The pointer value is a null reference");
			if (len < 0)
				throw new ArgumentOutOfRangeException("The length value is less than zero");
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("The capacity value is less than zero");
			if (len > capacity)
				throw new ArgumentOutOfRangeException("The length value is greater than the capacity value");
			fileaccess = access;
			length = len;
			this.capacity = capacity;
			initial_position = 0;
			current_position = initial_position;
			canseek = true;
			initial_pointer = new IntPtr ((void*)pointer);
			closed = false;
			fileaccess = access;
		}
#endregion
	
#region Properties
		public override bool CanRead {
			get {
				if (closed)
					return false;
				else
					return ((fileaccess == FileAccess.Read || fileaccess == FileAccess.ReadWrite)? current_position < capacity : false);
			}
		}

		public override bool CanSeek {
			get {
				return ((closed) ? false : true);
			}
		}
		
		public override bool CanWrite {
			get {
				if (closed)
					return (false);
				else
					return ((fileaccess == FileAccess.Write || fileaccess == FileAccess.ReadWrite)? true:false);
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
				else
					return (current_position);
			}
			set {
				if (closed)
					throw new ObjectDisposedException("The stream is closed");
				if (value < 0 || value > (long)Int32.MaxValue || value > capacity)
					throw new ArgumentOutOfRangeException("value that is less than zero, or the position is larger than Int32.MaxValue or capacity of the stream");
				else
					current_position = value;
			}
		}

		public unsafe byte* PositionPointer {
			get {
				throw new NotImplementedException("Error");
			}
			set {
				throw new NotImplementedException("Error");
			}
		}
#endregion
		
#region Methods
		public override int Read ([InAttribute] [OutAttribute] byte[] buffer, int offset, int count)
			 {
				if (closed)
					throw new ObjectDisposedException("The stream is closed");


				if (buffer == null)
					throw new ArgumentNullException("The buffer parameter is set to a null reference");
				if (offset < 0 || count < 0)
					throw new ArgumentOutOfRangeException("The offset or count parameter is less than zero");
				if ((buffer.Length - offset) < count)
					throw new ArgumentException("The length of the buffer array minus the offset parameter is less than the count parameter");

				if (fileaccess == FileAccess.Write)
					throw new NotSupportedException("Read property is false");
				else {
					if (current_position == capacity)
						return (0);
					else {
						int progress = current_position + count < capacity ? count : (int) (capacity - current_position);
						unsafe {
							Marshal.Copy(initial_pointer, buffer, offset, progress);
							current_position += progress;
						}
						return progress;
					}
				}
			}
		public override int ReadByte () {
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			if (current_position == capacity)
				throw new NotSupportedException("The current position is at the end of the stream");

			int byteread;

			if (fileaccess== FileAccess.Write)
				throw new NotSupportedException("The underlying memory does not support reading");
			else {
				if (current_position == length)
					return (-1);
				else {
					unsafe {
						byteread = (int)Marshal.ReadByte(initial_pointer, (int)current_position);
						current_position++;
					}
					return(byteread);
				}
			}
		}
		public override long Seek (long offset,	SeekOrigin loc) {
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			if (offset > capacity)
				throw new ArgumentOutOfRangeException("The offset value is larger than the maximum size of the stream");
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
			refpoint =+ (int)offset;
			if (refpoint < initial_position)
				throw new IOException("An attempt was made to seek before the beginning of the stream");
			current_position = refpoint;
			return(current_position);
		}
		 
		public override void SetLength (long value)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			if (value < 0 || value > capacity)
				throw new ArgumentOutOfRangeException("The specified value is negative or exceeds the capacity of the stream");
			if (fileaccess == FileAccess.Read)
				throw new NotSupportedException("write property is set to false");
			if (fileaccess == FileAccess.Read)
				throw new NotSupportedException("Length change not supported; see object construction");
			else
				length = value;
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

			closed = true;
		}
		 
		public override void Write (byte[] buffer, int offset, int count)
		{
			if (closed)
				throw new ObjectDisposedException("The stream is closed");
			if (buffer == null)
				throw new ArgumentNullException("The buffer parameter is a null reference");
			if (count > capacity)
				throw new ArgumentOutOfRangeException("The count value is greater than the capacity of the stream");
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException("One of the specified parameters is less than zero");
			if ((buffer.Length - offset) < count)
				throw new ArgumentException("The length of the buffer array minus the offset parameter is less than the count parameter");
			if (fileaccess == FileAccess.Read)
				throw new NotSupportedException("write property is set to false");
			else {
				unsafe {
					//COPY data from managed buffer to unmanaged mem pointer
					Marshal.Copy(buffer, offset, initial_pointer, (int)length);
					current_position += length;
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
				throw new NotSupportedException("write property is set to false");
			else {
				unsafe {
					Marshal.WriteByte(initial_pointer, (int)current_position, value);
					current_position++;
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
			canseek = true;
			initial_pointer = new IntPtr ((void *)pointer);
			closed = false;
		}
#endregion
	}
}
#endif

