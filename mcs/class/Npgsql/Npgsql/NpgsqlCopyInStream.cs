// Npgsql.NpgsqlCopyInStream.cs
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
	/// Stream for writing data to a table on a PostgreSQL version 7.4 or newer database during an active COPY FROM STDIN operation.
	/// <b>Passes data exactly as is and when given</b>, so see to it that you use server encoding, correct format and reasonably sized writes!
	/// </summary>
	internal class NpgsqlCopyInStream : Stream
	{
		private NpgsqlConnector _context;
		private long _bytesPassed = 0;

		/// <summary>
		/// True while this stream can be used to write copy data to server
		/// </summary>
		private bool IsActive
		{
			get { return _context != null && _context.CurrentState is NpgsqlCopyInState && _context.Mediator.CopyStream == this; }
		}

		/// <summary>
		/// Created only by NpgsqlCopyInState.StartCopy()
		/// </summary>
		internal NpgsqlCopyInStream(NpgsqlConnector context)
		{
			_context = context;
		}

		/// <summary>
		/// False
		/// </summary>
		public override bool CanRead
		{
			get { return false; }
		}

		/// <summary>
		/// True
		/// </summary>
		public override bool CanWrite
		{
			get { return true; }
		}

		/// <summary>
		/// False
		/// </summary>
		public override bool CanSeek
		{
			get { return false; }
		}

		/// <summary>
		/// Number of bytes written so far
		/// </summary>
		public override long Length
		{
			get { return _bytesPassed; }
		}

		/// <summary>
		/// Number of bytes written so far; not settable
		/// </summary>
		public override long Position
		{
			get { return _bytesPassed; }
			set { throw new NotSupportedException("Tried to set Position of network stream " + this); }
		}

		/// <summary>
		/// Successfully completes copying data to server. Returns after operation is finished.
		/// Does nothing if this stream is not the active copy operation writer.
		/// </summary>
		public override void Close()
		{
			if (_context != null)
			{
				if (IsActive)
				{
					_context.CurrentState.SendCopyDone(_context);
				}
				if (_context.Mediator.CopyStream == this)
				{
					_context.Mediator.CopyStream = null;
				}
				_context = null;
			}
		}

		/// <summary>
		/// Withdraws an already started copy operation. The operation will fail with given error message.
		/// Does nothing if this stream is not the active copy operation writer.
		/// </summary>
		public void Cancel(string message)
		{
			if (IsActive)
			{
				NpgsqlConnector c = _context;
				_context = null;
				c.Mediator.CopyStream = null;
				c.CurrentState.SendCopyFail(_context, message ?? "Cancel Copy");
			}
		}

		/// <summary>
		/// Writes given bytes to server.
		/// Fails if this stream is not the active copy operation writer.
		/// </summary>
		public override void Write(byte[] buf, int off, int len)
		{
			if (! IsActive)
			{
				throw new ObjectDisposedException("Writing to closed " + this);
			}
			_context.CurrentState.SendCopyData(_context, buf, off, len);
			_bytesPassed += len;
		}

		/// <summary>
		/// Flushes stream contents to server.
		/// Fails if this stream is not the active copy operation writer.
		/// </summary>
		public override void Flush()
		{
			if (! IsActive)
			{
				throw new ObjectDisposedException("Flushing closed " + this);
			}
			_context.Stream.Flush();
		}

		/// <summary>
		/// Not readable
		/// </summary>
		public override int Read(byte[] buf, int off, int len)
		{
			throw new NotSupportedException("Tried to read non-readable " + this);
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
	}
}