// HtmlAgilityPack V1.3.1.0 - Simon Mourier <simonm@microsoft.com>
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HtmlAgilityPack
{
	/// <summary>
	/// Represents a rewindable buffered TextReader specifically well suited for parsing operations.
	/// </summary>
	public class ParseReader: Stream
	{
		private StringBuilder _sb;
		private int _baseReaderPosition;
		private int _maxReaderPosition;
		private int _position;
		private TextReader _baseReader;

		/// <summary>
		/// Initializes an instance of the ParserReader class, based on an existing TextReader instance.
		/// </summary>
		/// <param name="baseReader">The TextReader to base parsing on. Must not be null.</param>
		public ParseReader(TextReader baseReader)
		{
			if (baseReader == null)
				throw new ArgumentNullException("baseReader");

			_baseReader = baseReader;
			_sb = new StringBuilder();
			_position = 0;
			_baseReaderPosition = 0;
			_maxReaderPosition = int.MaxValue;
		}

		/// <summary>
		/// Gets the length in bytes of the stream.
		/// Always throws a NotSupportedException for the ParserReader class.
		/// </summary>
		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Gets or sets the position within the stream.
		/// </summary>
		public override long Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value < 0)
					throw new ArgumentException("value is negative: " + value + ".");
				if (value > int.MaxValue)
					throw new ArgumentException("value must not be larger than int32 MaxValue.");
				_position = (int)value;
			}
		}

		/// <summary>
		/// Checks the length of the underlying stream.
		/// </summary>
		/// <param name="length">The required length.</param>
		/// <returns>true if the underlying stream's length is greater than the required length, false otherwise.</returns>
		public bool CheckLength(int length)
		{
			if (length <= 0)
				throw new ArgumentException("length must be greater than zero.");

			if (BufferedTextLength >= length)
				return true;

			Seek(length, SeekOrigin.Begin);
			return (BufferedTextLength >= length);
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// Always returns true for the ParserReader class.
		/// </summary>
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// Always returns true for the ParserReader class.
		/// </summary>
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// Always returns false for the ParserReader class.
		/// </summary>
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Sets the length of the current stream.
		/// Always throws a NotSupportedException for the ParserReader class.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush()
		{
			// nothing to do
		}

		/// <summary>
		/// Gets the position within the underlying stream.
		/// </summary>
		public int BaseReaderPosition
		{
			get
			{
				return _baseReaderPosition;
			}
		}

		/// <summary>
		/// Gets the maximum position within the underlying stream.
		/// </summary>
		public int MaxReaderPosition
		{
			get
			{
				return _maxReaderPosition;
			}
		}

		private void CheckBaseReader()
		{
			if (_baseReader == null)
				throw new InvalidOperationException("Cannot read from a closed ParseReader.");
		}

		/// <summary>
		/// Closes the current underlying stream.
		/// </summary>
		public void CloseBaseReader()
		{
			if (_maxReaderPosition != int.MaxValue) // we have already closed it
				return;

			CheckBaseReader();

			_baseReader.Close();
			_baseReader = null;
		}

		private void InternalCloseBaseReader()
		{
			CloseBaseReader();
			_maxReaderPosition = _position;
		}

		/// <summary>
		/// Returns the next available character but does not consume it.
		/// </summary>
		/// <returns>The next character to be read, or -1 if no more characters are available.</returns>
		public int Peek()
		{
			if (_position < _baseReaderPosition)
				return Convert.ToInt32(this[_position]);

			if (_position == _maxReaderPosition)
				return -1;

			CheckBaseReader();
			int i = _baseReader.Peek();
			if (i < 0)
			{
				InternalCloseBaseReader();
				return i;
			}

			Debug.Assert(_position >= _baseReaderPosition);
			if (_position == _baseReaderPosition)
			{
				if (_sb.Length < (_position + 1))
				{
					_sb.Append(Convert.ToChar(i));
				}
			}
			return i;
		}

		/// <summary>
		/// Reads the next character and advances the character position by one character.
		/// </summary>
		/// <returns>The next character represented as an Int32, or -1 if no more characters are available.</returns>
		public int Read()
		{
			int i;
			if (_position < _baseReaderPosition)
			{
				i =  Convert.ToInt32(_sb[_position]);
				_position++;
				return i;
			}

			if (_position == _maxReaderPosition)
				return -1;

			CheckBaseReader();
			i = _baseReader.Read();
			if (i < 0)
			{
				InternalCloseBaseReader();
				return i;
			}

			if (_position >= _baseReaderPosition)
			{
				if (_sb.Length < (_position + 1))
				{
					_sb.Append(Convert.ToChar(i));
				}
			}
			_baseReaderPosition++;
			_position++;
			return i;
		}

		/// <summary>
		/// Move the position starting from the current position.
		/// </summary>
		/// <param name="count">A character offset relative to the current position.</param>
		/// <returns>The new position.</returns>
		public int Seek(int count)
		{
			int i;
			if (count < 0)
			{
				if ((_position + count ) < 0)
				{
					i = _position;
					_position = 0;
					return i;
				}
				else
				{
					_position += count;
					return - count;
				}
			}
			for(i=0;i<count;i++)
			{
				int c = Read();
				if (c < 0)
				{
					break;
				}
			}
			return i;
		}

		/// <summary>
		/// Reads a string from the current position.
		/// </summary>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The read string or null, if an error occurred.</returns>
		public string ReadString(int count)
		{
			int first = (int)Position;
			Seek(count);
			int last = (int)Position;
			if (first >= _sb.Length)
				return null;
			return _sb.ToString(first, last - first);
		}

		/// <summary>
		/// Reads a string, represented as an array of System.Int32, from the current position.
		/// </summary>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The read string or null, if an error occurred.</returns>
		public int[] Read(int count)
		{
			string s = ReadString(count);
			if (s == null)
				return null;
			char[] chars = s.ToCharArray();
			int[] ints = new int[chars.Length];
			chars.CopyTo(ints, 0);
			return ints;
		}

		/// <summary>
		/// reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count- 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			// we don't really know how to read count bytes... so we read count chars
			string s = ReadString(count);
			if (s == null)
				return 0;
			byte[] bytes = System.Text.Encoding.Unicode.GetBytes(s); // probably around 2*count bytes
			int read = 0;
			for(int i=0;i<bytes.Length;i++)
			{
				buffer[offset + i] = bytes[i];
				read++;
				if (read == count) // enough?
					break;
			}
			return read;
		}

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// Always throws a NotSupportedException for the ParserReader class.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (offset > int.MaxValue)
				throw new ArgumentException("offset must not be larger than int32 MaxValue.");

			switch(origin)
			{
				case SeekOrigin.Begin:
					_position = 0;
					break;

				case SeekOrigin.End:
					Seek(int.MaxValue);
					break;

				case SeekOrigin.Current:
					break;
			}
			return Seek((int)offset);
		}

		/// <summary>
		/// Gets the character at the specified index or -1 if no more characters are available.
		/// </summary>
		public int this[int index]
		{
			get
			{
				if (index >= _baseReaderPosition)
				{
					int count = Seek(index - _baseReaderPosition);
					if (count < (index - _baseReaderPosition))
						return -1;
					int i = Peek();
					if (i < 0)
						return -1;
				}
				return _sb[index];
			}
		}

		/// <summary>
		/// Gets the length of the currently buffered text.
		/// </summary>
		public int BufferedTextLength
		{
			get
			{
				return _sb.Length;
			}
		}

		/// <summary>
		/// Gets the currently buffered text.
		/// </summary>
		public string BufferedText
		{
			get
			{
				return _sb.ToString();
			}
		}

		/// <summary>
		/// Extracts a string out of the buffered text.
		/// </summary>
		/// <param name="offset">The zero-based byte offset in buffered text at which to begin extracting.</param>
		/// <param name="length">The maximum number of bytes to be read from the buffered text.</param>
		/// <returns></returns>
		public string GetBufferedString(int offset, int length)
		{
			if (offset > BufferedTextLength)
			{
				return null;
			}
			if ((offset + length) > BufferedTextLength)
			{
				length -= (offset + length) - BufferedTextLength;
			}
			return BufferedText.Substring(offset, length);
		}

	}

}
