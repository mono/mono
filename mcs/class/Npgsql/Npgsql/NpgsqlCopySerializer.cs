// Npgsql.NpgsqlCopySerializer.cs
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
using System.Text;

namespace Npgsql
{
	/// <summary>
	/// Writes given objects into a stream for PostgreSQL COPY in default copy format (not CSV or BINARY).
	/// </summary>
	public class NpgsqlCopySerializer
	{
		private static readonly Encoding ENCODING_UTF8 = Encoding.UTF8;

		public static String DEFAULT_DELIMITER = "\t",
		                     DEFAULT_SEPARATOR = "\n",
		                     DEFAULT_NULL = "\\N",
		                     DEFAULT_ESCAPE = "\\",
		                     DEFAULT_QUOTE = "\"";

		public static int DEFAULT_BUFFER_SIZE = 8192;

		private readonly NpgsqlConnector _context;
		private Stream _toStream;

		private String _delimiter = DEFAULT_DELIMITER,
		               _escape = DEFAULT_ESCAPE,
		               _separator = DEFAULT_SEPARATOR,
		               _null = DEFAULT_NULL;

		private byte[] _delimiterBytes = null, _escapeBytes = null, _separatorBytes = null, _nullBytes = null;
		private byte[][] _escapeSequenceBytes = null;
		private String[] _stringsToEscape = null;

		private byte[] _sendBuffer = null;
		private int _sendBufferAt = 0, _lastFieldEndAt = 0, _lastRowEndAt = 0, _atField = 0;

		public NpgsqlCopySerializer(NpgsqlConnection conn)
		{
			_context = conn.Connector;
		}

		public bool IsActive
		{
			get { return _toStream != null && _context.Mediator.CopyStream == _toStream && _context.CurrentState is NpgsqlCopyInState; }
		}

		public Stream ToStream
		{
			get
			{
				if (_toStream == null)
				{
					_toStream = _context.Mediator.CopyStream;
				}
				return _toStream;
			}
			set
			{
				if (IsActive)
				{
					throw new NpgsqlException("Do not change stream of an active " + this);
				}
				_toStream = value;
			}
		}

		public String Delimiter
		{
			get { return _delimiter; }
			set
			{
				if (IsActive)
				{
					throw new NpgsqlException("Do not change delimiter of an active " + this);
				}
				_delimiter = value ?? DEFAULT_DELIMITER;
				_delimiterBytes = null;
				_stringsToEscape = null;
				_escapeSequenceBytes = null;
			}
		}

		private byte[] DelimiterBytes
		{
			get
			{
				if (_delimiterBytes == null)
				{
					_delimiterBytes = ENCODING_UTF8.GetBytes(_delimiter);
				}
				return _delimiterBytes;
			}
		}

		public String Separator
		{
			get { return _separator; }
			set
			{
				if (IsActive)
				{
					throw new NpgsqlException("Do not change separator of an active " + this);
				}
				_separator = value ?? DEFAULT_SEPARATOR;
				_separatorBytes = null;
				_stringsToEscape = null;
				_escapeSequenceBytes = null;
			}
		}

		private byte[] SeparatorBytes
		{
			get
			{
				if (_separatorBytes == null)
				{
					_separatorBytes = ENCODING_UTF8.GetBytes(_separator);
				}
				return _separatorBytes;
			}
		}

		public String Escape
		{
			get { return _escape; }
			set
			{
				if (IsActive)
				{
					throw new NpgsqlException("Do not change escape symbol of an active " + this);
				}
				_escape = value ?? DEFAULT_ESCAPE;
				_escapeBytes = null;
				_stringsToEscape = null;
				_escapeSequenceBytes = null;
			}
		}

		private byte[] EscapeBytes
		{
			get
			{
				if (_escapeBytes == null)
				{
					_escapeBytes = ENCODING_UTF8.GetBytes(_escape);
				}
				return _escapeBytes;
			}
		}

		public String Null
		{
			get { return _null; }
			set
			{
				if (IsActive)
				{
					throw new NpgsqlException("Do not change null symbol of an active " + this);
				}
				_null = value ?? DEFAULT_NULL;
				_nullBytes = null;
				_stringsToEscape = null;
				_escapeSequenceBytes = null;
			}
		}

		private byte[] NullBytes
		{
			get
			{
				if (_nullBytes == null)
				{
					_nullBytes = ENCODING_UTF8.GetBytes(_null);
				}
				return _nullBytes;
			}
		}

		public Int32 BufferSize
		{
			get { return _sendBuffer != null ? _sendBuffer.Length : DEFAULT_BUFFER_SIZE; }
			set
			{
				byte[] _newBuffer = new byte[value];
				if (_sendBuffer != null)
				{
					for (int i = 0; i < _sendBufferAt; i++)
					{
						_newBuffer[i] = _sendBuffer[i];
					}
				}
				_sendBuffer = _newBuffer;
			}
		}

		public void Flush()
		{
			if (_sendBufferAt > 0)
			{
				ToStream.Write(_sendBuffer, 0, _sendBufferAt);
				ToStream.Flush();
			}
			_sendBufferAt = 0;
			_lastRowEndAt = 0;
			_lastFieldEndAt = 0;
		}

		public void FlushRows()
		{
			if (_lastRowEndAt > 0)
			{
				ToStream.Write(_sendBuffer, 0, _lastRowEndAt);
				ToStream.Flush();
				int len = _sendBufferAt - _lastRowEndAt;
				for (int i = 0; i < len; i++)
				{
					_sendBuffer[i] = _sendBuffer[_lastRowEndAt + i];
				}
				_lastFieldEndAt -= _lastRowEndAt;
				_sendBufferAt -= _lastRowEndAt;
				_lastRowEndAt = 0;
			}
		}

		public void FlushFields()
		{
			if (_lastFieldEndAt > 0)
			{
				ToStream.Write(_sendBuffer, 0, _lastFieldEndAt);
				ToStream.Flush();
				int len = _sendBufferAt - _lastFieldEndAt;
				for (int i = 0; i < len; i++)
				{
					_sendBuffer[i] = _sendBuffer[_lastFieldEndAt + i];
				}
				_lastRowEndAt -= _lastFieldEndAt;
				_sendBufferAt -= _lastFieldEndAt;
				_lastFieldEndAt = 0;
			}
		}

		public void Close()
		{
			if (_atField > 0)
			{
				EndRow();
			}
			Flush();
			ToStream.Close();
		}

		protected int SpaceInBuffer
		{
			get { return BufferSize - _sendBufferAt; }
		}

		protected String[] StringsToEscape
		{
			get
			{
				if (_stringsToEscape == null)
				{
					_stringsToEscape = new String[] {Delimiter, Separator, Escape, "\r", "\n"};
				}
				return _stringsToEscape;
			}
		}

		protected byte[][] EscapeSequenceBytes
		{
			get
			{
				if (_escapeSequenceBytes == null)
				{
					_escapeSequenceBytes = new byte[StringsToEscape.Length][];
					for (int i = 0; i < StringsToEscape.Length; i++)
					{
						_escapeSequenceBytes[i] = EscapeSequenceFor(StringsToEscape[i].ToCharArray(0, 1)[0]);
					}
				}
				return _escapeSequenceBytes;
			}
		}

		private static readonly byte[] esc_t = new byte[] {(byte) 't'};

		private static readonly byte[] esc_n = new byte[] {(byte) 'n'};

		private static readonly byte[] esc_r = new byte[] {(byte) 'r'};

		private static readonly byte[] esc_b = new byte[] {(byte) 'b'};

		private static readonly byte[] esc_f = new byte[] {(byte) 'f'};

		private static readonly byte[] esc_v = new byte[] {(byte) 'v'};

		protected static byte[] EscapeSequenceFor(char c)
		{
			return
				c == '\t'
					? esc_t
					: c == '\n'
					  	? esc_n
					  	: c == '\r'
					  	  	? esc_r
					  	  	: c == '\b'
					  	  	  	? esc_b
					  	  	  	: c == '\f'
					  	  	  	  	? esc_f
					  	  	  	  	: c == '\v'
					  	  	  	  	  	? esc_v
					  	  	  	  	  	: (c < 32 || c > 127)
					  	  	  	  	  	  	? new byte[] {(byte) ('0' + ((c/64) & 7)), (byte) ('0' + ((c/8) & 7)), (byte) ('0' + (c & 7))}
					  	  	  	  	  	  	: new byte[] {(byte) c};
		}

		protected void MakeRoomForBytes(int len)
		{
			if (_sendBuffer == null)
			{
				_sendBuffer = new byte[BufferSize];
			}
			if (len >= SpaceInBuffer)
			{
				FlushRows();
				if (len >= SpaceInBuffer)
				{
					FlushFields();
					if (len >= SpaceInBuffer)
					{
						BufferSize = len;
					}
				}
			}
		}

		protected void AddBytes(byte[] bytes)
		{
			MakeRoomForBytes(bytes.Length);

			for (int i = 0; i < bytes.Length; i++)
			{
				_sendBuffer[_sendBufferAt++] = bytes[i];
			}
		}

		public void EndRow()
		{
			if (_context != null)
			{
				while (_atField < _context.CurrentState.CopyFormat.FieldCount)
				{
					AddNull();
				}
			}
			if (_context == null || ! _context.CurrentState.CopyFormat.IsBinary)
			{
				AddBytes(SeparatorBytes);
			}
			_lastRowEndAt = _sendBufferAt;
			_atField = 0;
		}

		protected void PrefixField()
		{
			if (_atField > 0)
			{
				if (_atField >= _context.CurrentState.CopyFormat.FieldCount)
				{
					throw new NpgsqlException("Tried to add too many fields to a copy record with " + _atField + " fields");
				}
				AddBytes(DelimiterBytes);
			}
		}

		protected void FieldAdded()
		{
			_lastFieldEndAt = _sendBufferAt;
			_atField++;
		}

		public void AddNull()
		{
			PrefixField();
			AddBytes(NullBytes);
			FieldAdded();
		}

		public void AddString(String fieldValue)
		{
			PrefixField();
			int bufferedUpto = 0;
			while (bufferedUpto < fieldValue.Length)
			{
				int escapeAt = fieldValue.Length;
				byte[] escapeSequence = null;

				// choose closest instance of strings to escape in fieldValue
				for (int eachEscapeable = 0; eachEscapeable < StringsToEscape.Length; eachEscapeable++)
				{
					int i = fieldValue.IndexOf(StringsToEscape[eachEscapeable], bufferedUpto);
					if (i > -1 && i < escapeAt)
					{
						escapeAt = i;
						escapeSequence = EscapeSequenceBytes[eachEscapeable];
					}
				}

				// some, possibly all of fieldValue string does not require escaping and can be buffered for output
				if (escapeAt > bufferedUpto)
				{
					int encodedLength = ENCODING_UTF8.GetByteCount(fieldValue.ToCharArray(bufferedUpto, escapeAt));
					MakeRoomForBytes(encodedLength);
					_sendBufferAt += ENCODING_UTF8.GetBytes(fieldValue, bufferedUpto, escapeAt, _sendBuffer, _sendBufferAt);
					bufferedUpto = escapeAt;
				}

				// now buffer the escape sequence for output
				if (escapeSequence != null)
				{
					AddBytes(EscapeBytes);
					AddBytes(escapeSequence);
					bufferedUpto++;
				}
			}
			FieldAdded();
		}

		public void AddInt32(Int32 fieldValue)
		{
			AddString(string.Format("{0}", fieldValue));
		}

		public void AddInt64(Int64 fieldValue)
		{
			AddString(string.Format("{0}", fieldValue));
		}

		public void AddNumber(double fieldValue)
		{
			AddString(string.Format("{0}", fieldValue));
		}

		public void AddBool(bool fieldValue)
		{
			AddString(fieldValue ? "TRUE" : "FALSE");
		}

		public void AddDateTime(DateTime fieldValue)
		{
			AddString(string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}", fieldValue.Year, fieldValue.Month, fieldValue.Day, fieldValue.Hour, fieldValue.Minute, fieldValue.Second, fieldValue.Millisecond));
		}
	}
}