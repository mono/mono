// Npgsql.NpgsqlCopyOutStream.cs
//
// Author:
//     Kalle Hallivuori <kato@iki.fi>
//
//    Copyright (C) 2007 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.


using System;
using System.IO;

namespace Npgsql
{
	/// <summary>
	/// Stream for reading data from a table or select on a PostgreSQL version 7.4 or newer database during an active COPY TO STDOUT operation.
	/// <b>Passes data exactly as provided by the server.</b>
	/// </summary>
	internal class NpgsqlCopyOutStream : Stream
	{
		private NpgsqlConnector _context;
		private long _bytesPassed = 0;
		private byte[] _buf = null;
		private int _bufOffset = 0;

		/// <summary>
		/// True while this stream can be used to read copy data from server
		/// </summary>
		private bool IsActive
		{
			get { return _context != null && _context.CurrentState is NpgsqlCopyOutState && _context.Mediator.CopyStream == this; }
		}

		/// <summary>
		/// Created only by NpgsqlCopyOutState.StartCopy()
		/// </summary>
		internal NpgsqlCopyOutStream(NpgsqlConnector context)
		{
			_context = context;
		}

		/// <summary>
		/// True
		/// </summary>
		public override bool CanRead
		{
			get { return true; }
		}

		/// <summary>
		/// False
		/// </summary>
		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>
		/// False
		/// </summary>
		public override bool CanSeek
		{
			get { return false; }
		}

		/// <summary>
		/// Number of bytes read so far
		/// </summary>
		public override long Length
		{
			get { return _bytesPassed; }
		}

		/// <summary>
		/// Number of bytes read so far; can not be set.
		/// </summary>
		public override long Position
		{
			get { return _bytesPassed; }
			set { throw new NotSupportedException("Tried to set Position of network stream " + this); }
		}

		/// <summary>
		/// Discards copy data as long as server pushes it. Returns after operation is finished.
		/// Does nothing if this stream is not the active copy operation reader.
		/// </summary>
		public override void Close()
		{
			if (_context != null)
			{
				if (IsActive)
				{
					while (_context.CurrentState.GetCopyData(_context) != null)
					{
						; // flush rest
					}
				}
				if (_context.Mediator.CopyStream == this)
				{
					_context.Mediator.CopyStream = null;
				}
				_context = null;
			}
		}

		/// <summary>
		/// Not writable.
		/// </summary>
		public override void Write(byte[] buf, int off, int len)
		{
			throw new NotSupportedException("Tried to write non-writable " + this);
		}

		/// <summary>
		/// Not flushable.
		/// </summary>
		public override void Flush()
		{
			throw new NotSupportedException("Tried to flush read-only " + this);
		}

		/// <summary>
		/// Copies data read from server to given byte buffer.
		/// Since server returns data row by row, length will differ each time, but it is only zero once the operation ends.
		/// Can be mixed with calls to the more efficient NpgsqlCopyOutStream.Read() : byte[] though that would not make much sense.
		/// </summary>
		public override int Read(byte[] buf, int off, int len)
		{
			if (! IsActive)
			{
				throw new ObjectDisposedException("Reading from closed " + this);
			}

			if (_buf == null) // otherwise _buf still contains data that did not fit into request buffer in an earlier call
			{
				_buf = Read();
				_bufOffset = 0;
			}
			if (off + len > buf.Length)
			{
				len = buf.Length - off;
			}

			int i = 0;
			if (_buf != null)
			{
				for (; _bufOffset < _buf.Length && i < len; i++)
				{
					buf[off + i] = _buf[_bufOffset++];
				}
				if (_bufOffset >= _buf.Length)
				{
					_buf = null; // whole of our contents fit into request buffer
				}
				_bytesPassed += i;
			}
			return i;
		}

		/// <summary>
		/// Not seekable
		/// </summary>
		public override long Seek(long pos, SeekOrigin so)
		{
			throw new NotSupportedException("Tried to seek non-seekable " + this);
		}

		/// <summary>
		/// Not supported
		/// </summary>
		public override void SetLength(long len)
		{
			throw new NotSupportedException("Tried to set length of network stream " + this);
		}

		/// <summary>
		/// Returns a whole row of data from server without extra work.
		/// If standard Stream.Read(...) has been called before, it's internal buffers remains are returned.
		/// </summary>
		public byte[] Read()
		{
			byte[] result;
			if (_buf == null)
			{
				result = _context.CurrentState.GetCopyData(_context);
			}
			else if (_bufOffset < 1)
			{
				result = _buf;
			}
			else
			{
				result = new byte[_buf.Length - _bufOffset];
				for (int i = 0; i < result.Length; i++)
				{
					result[i] = _buf[_bufOffset + i];
				}
				_buf = null;
			}
			return result;
		}
	}
}