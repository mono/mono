//
// System.IO.BufferedStream
//
// Author:
//   Matt Kimball (matt@kimball.net)
//

namespace System.IO {
	public sealed class BufferedStream : Stream {
		Stream m_stream;
		byte[] m_buffer;
		int m_buffer_pos;
		int m_buffer_read_ahead;
		bool m_buffer_reading;

		public BufferedStream(Stream stream) : this(stream, 4096) {
		}

		public BufferedStream(Stream stream, int buffer_size) {
			m_stream = stream;
			m_buffer = new byte[buffer_size];
		}

		public override bool CanRead {
			get {
				return m_stream.CanRead;
			}
		}

		public override bool CanWrite {
			get {
				return m_stream.CanWrite;
			}
		}

		public override bool CanSeek {
			get {
				return m_stream.CanSeek;
			}
		}

		public override long Length {
			get {
				return m_stream.Length;
			}
		}
		
		public override long Position {
			get {
				return m_stream.Position - m_buffer_read_ahead + m_buffer_pos;
			}

			set {
				Flush();
				m_stream.Position = value;
			}
		}

		public override void Close() {
			Flush();
			m_stream.Close();
			m_stream = null;
			m_buffer = null;
		}

		public override void Flush() {
			if (m_buffer_reading) {
				if (CanSeek)
					m_stream.Position = Position;
			} else if (m_buffer_pos > 0) {
				m_stream.Write(m_buffer, 0, m_buffer_pos);
			}

			m_buffer_read_ahead = 0;
			m_buffer_pos = 0;
		}

		public override long Seek(long offset, SeekOrigin origin) {
			Flush();
			return m_stream.Seek(offset, origin);
		}

		public override void SetLength(long value) {
			m_stream.SetLength(value);
		}

		public override int ReadByte() {
			byte[] b = new byte[1];

			if (Read(b, 0, 1) == 1) {
				return b[0];
			} else {
				return -1;
			}
		}

		public override void WriteByte(byte value) {
			byte[] b = new byte[1];

			b[0] = value;
			Write(b, 0, 1);
		}

		public override int Read(byte[] array, int offset, int count) {
			if (!m_buffer_reading) {
				Flush();
				m_buffer_reading = true;
			}

			if (count <= m_buffer_read_ahead - m_buffer_pos) {
				Array.Copy(m_buffer, m_buffer_pos, array, offset, count);

				m_buffer_pos += count;
				if (m_buffer_pos == m_buffer_read_ahead) {
					m_buffer_pos = 0;
					m_buffer_read_ahead = 0;
				}

				return count;
			}

			int ret = m_buffer_read_ahead - m_buffer_pos;
			Array.Copy(m_buffer, m_buffer_pos, array, offset, ret);
			m_buffer_pos = 0;
			m_buffer_read_ahead = 0;
			offset += ret;
			count -= ret;

			if (count >= m_buffer.Length) {
				ret += m_stream.Read(array, offset, count);
			} else {
				m_buffer_read_ahead = m_stream.Read(m_buffer, 0, m_buffer.Length);
				
				if (count < m_buffer_read_ahead) {
					Array.Copy(m_buffer, 0, array, offset, count);
					m_buffer_pos = count;
					ret += count;
				} else {
					Array.Copy(m_buffer, 0, array, offset, m_buffer_read_ahead);
					ret += m_buffer_read_ahead;
					m_buffer_read_ahead = 0;
				}
			}

			return ret;
		}

		public override void Write(byte[] array, int offset, int count) {
			if (m_buffer_reading) {
				Flush();
				m_buffer_reading = false;
			}

			if (m_buffer_pos + count >= m_buffer.Length) {
				Flush();
				m_stream.Write(array, offset, count);
			} else {
				Array.Copy(array, offset, m_buffer, m_buffer_pos, count);
				m_buffer_pos += count;
			}
		}
	}
}
