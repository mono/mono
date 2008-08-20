//
// BufferManager.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public abstract class BufferManager
	{
		protected BufferManager ()
		{
		}

		public abstract void Clear ();

		[MonoTODO]
		public static BufferManager CreateBufferManager (
			long maxBufferPoolSize, int maxBufferSize)
		{
			return new DefaultBufferManager (maxBufferPoolSize,
				maxBufferSize);
		}

		public abstract void ReturnBuffer (byte[] buffer);

		public abstract byte[] TakeBuffer (int bufferSize);

		class DefaultBufferManager : BufferManager
		{
			long max_pool_size;
			int max_size;
			byte [] buffer;

			public DefaultBufferManager (long maxBufferPoolSize,
				int maxBufferSize)
			{
				this.max_pool_size = maxBufferPoolSize;
				this.max_size = maxBufferSize;
			}

			public override void Clear ()
			{
				if (buffer != null)
					Array.Clear (buffer, 0, buffer.Length);
			}

			public override void ReturnBuffer (byte [] buffer)
			{
				// is this correct?

				if (this.buffer == null)
					return;
				Array.Copy (this.buffer, buffer, this.buffer.Length);
			}

			public override byte [] TakeBuffer (int bufferSize)
			{
				if (bufferSize > max_size)
					throw new ArgumentOutOfRangeException ();

				if (buffer == null || buffer.Length < bufferSize)
					buffer = new byte [bufferSize];
				return buffer;
			}
		}
	}
}
