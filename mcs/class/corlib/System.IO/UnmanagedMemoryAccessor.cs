//
// System.IO.UnmanagedMemoryAccessor.cs
//
// Author:
//  Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.IO
{
	public class UnmanagedMemoryAccessor : IDisposable {
		SafeBuffer buffer;
		long offset;
		long capacity;
		bool canwrite, canread;

		public UnmanagedMemoryAccessor ()
		{
		}

		public UnmanagedMemoryAccessor (SafeBuffer buffer, long offset, long capacity)
		{
			Initialize (buffer, offset, capacity, FileAccess.ReadWrite);
		}

		public UnmanagedMemoryAccessor (SafeBuffer buffer, long offset, long capacity, FileAccess access)
		{
			Initialize (buffer, offset, capacity, access);
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected void Initialize(SafeBuffer buffer, long offset, long capacity, FileAccess access)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			if (offset + capacity < 0)
				throw new InvalidOperationException ();

			if (access == FileAccess.Read || access == FileAccess.ReadWrite)
				canread = true;
			if (access == FileAccess.Write || access == FileAccess.ReadWrite)
				canwrite = true;

			if (this.buffer != null)
				Dispose (true);
					 
			this.buffer = buffer;
			this.offset = offset;
			this.capacity = capacity;
		}
		
		public void Dispose () {
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public void Dispose (bool disposing) {
			if (buffer != null){
				if (disposing){
					buffer.Dispose ();
				}
			}
			buffer = null;
		}

#if false
	//
	// The code below can be enabled once this bug is fixed:
	// https://bugzilla.novell.com/show_bug.cgi?id=553650
	//
		public byte ReadByte (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<byte> ((ulong) position);
		}

		public bool ReadBoolean (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<bool> ((ulong) position);
		}

		public bool ReadChar (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<char> ((ulong) position);
		}
		
		public bool ReadDecimal (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<decimal> ((ulong) position);
		}
		
		public bool ReadDouble (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<double> ((ulong) position);
		}

		public bool ReadInt16 (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<short> ((ulong) position);
		}
		
		public bool ReadInt32 (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<int> ((ulong) position);
		}
		
		public bool ReadInt64 (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<long> ((ulong) position);
		}
		
		public bool ReadSByte (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<sbyte> ((ulong) position);
		}
		
		public bool ReadSingle (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<float> ((ulong) position);
		}
		
		[CLSCompliant (false)]
		public bool ReadUInt16 (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<ushort> ((ulong) position);
		}

		[CLSCompliant (false)]
		public bool ReadUInt32 (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<uint> ((ulong) position);
		}

		[CLSCompliant (false)]
		public bool ReadUInt64 (long position) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			return buffer.Read<ulong> ((ulong) position);
		}

		public void Read<T> (long position, out T structure) 
		{
			if (!canread)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			structure = buffer.Read<T> ((ulong) position);
		}

		public int ReadArray<T> (long position, T [] array, int offset, int count) 
		{
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			long left = capacity - position;
			int slots = (left / Marshal.SizeOf (typeof (T)));
			
			buffer.ReadArray ((ulong) position, array, index, slots);
			return slots;
		}
		
		public void Write (long position, bool value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, byte value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, char value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, decimal value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, short value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, int value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, long value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		[CLSCompliant (false)]
			public void Write (long position, sbyte value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write (long position, float value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		[CLSCompliant (false)]
			public void Write (long position, ushort value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		[CLSCompliant (false)]
			public void Write (long position, uint value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		[CLSCompliant (false)]
		public void Write (long position, ulong value) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write ((ulong)position, value);
		}

		public void Write<T> (long position, ref T structure) 
		{
			if (!canwrite)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ObjectDisposedException ("buffer");
			if (position < 0)
				throw new ArgumentOutOfRangeException ();
			
			buffer.Write<T> (position, structure);
		}

		public void WriteArray<T> (long position, T [] array, int offset, int count) where T : struct 
		{
			buffer.WriteArray (position, array, offset, count);
		}
#endif
	
		public bool CanRead {
			get { return canread; }
		}

		public bool CanWrite {
			get { return canwrite; }
		}

		public long Capacity {
			get { return capacity; }
		}

		
	}
}

#endif
