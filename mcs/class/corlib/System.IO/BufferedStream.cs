//
// System.IO.BufferedStream
//
// Author:
//   Matt Kimball (matt@kimball.net)
//   Ville Palo <vi64pa@kolumbus.fi>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.IO {
	public sealed class BufferedStream : Stream {
		Stream m_stream;
		byte[] m_buffer;
		int m_buffer_pos;
		int m_buffer_read_ahead;
		bool m_buffer_reading;
		private bool disposed = false;

		public BufferedStream (Stream stream) : this (stream, 4096) 
		{
		}

		public BufferedStream (Stream stream, int buffer_size) 
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			// LAMESPEC: documented as < 0
			if (buffer_size <= 0)
				throw new ArgumentOutOfRangeException ("buffer_size", "<= 0");
			if (!stream.CanRead && !stream.CanWrite) {
				throw new ObjectDisposedException (
					Locale.GetText ("Cannot access a closed Stream."));
			}

			m_stream = stream;
			m_buffer = new byte [buffer_size];
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
				Flush ();
				return m_stream.Length;
			}
		}
		
		public override long Position {
			get {
				CheckObjectDisposedException ();
				return m_stream.Position - m_buffer_read_ahead + m_buffer_pos;
			}

			set {
				if (value < Position && (Position - value <= m_buffer_pos) && m_buffer_reading) {
					m_buffer_pos -= (int) (Position - value);
				}
				else if (value > Position && (value - Position < m_buffer_read_ahead - m_buffer_pos) && m_buffer_reading) {
					m_buffer_pos += (int) (value - Position);
				}
				else {
					Flush();
					m_stream.Position = value;
				}
			}
		}

		public override void Close ()
		{
			if (m_buffer != null)
				Flush();

			m_stream.Close();
			m_buffer = null;
			disposed = true;
		}

		public override void Flush ()
		{
			CheckObjectDisposedException ();

			if (m_buffer_reading) {
				if (CanSeek)
					m_stream.Position = Position;
			} else if (m_buffer_pos > 0) {
				m_stream.Write(m_buffer, 0, m_buffer_pos);
			}

			m_buffer_read_ahead = 0;
			m_buffer_pos = 0;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			CheckObjectDisposedException ();
			if (!CanSeek) {
				throw new NotSupportedException (
					Locale.GetText ("Non seekable stream."));
			}
			Flush ();
			return m_stream.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			CheckObjectDisposedException ();
			m_stream.SetLength(value);
			if (Position > value)
				Position = value;
		}

		public override int ReadByte ()
		{
			CheckObjectDisposedException ();
			
			byte[] b = new byte[1];

			if (Read(b, 0, 1) == 1) {
				return b[0];
			} else {
				return -1;
			}
		}

		public override void WriteByte (byte value) 
		{
			CheckObjectDisposedException ();
			byte[] b = new byte[1];

			b[0] = value;
			Write(b, 0, 1);
		}

		public override int Read ([In,Out] byte[] array, int offset, int count) 
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			CheckObjectDisposedException ();
			if (!m_stream.CanRead) {
				throw new NotSupportedException (
					Locale.GetText ("Cannot read from stream"));
			}
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (array.Length - offset < count)
				throw new ArgumentException ("array.Length - offset < count");

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

		public override void Write (byte[] array, int offset, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			CheckObjectDisposedException ();
			if (!m_stream.CanWrite) {
				throw new NotSupportedException (
					Locale.GetText ("Cannot write to stream"));
			}
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// avoid possible integer overflow
			if (array.Length - offset < count)
				throw new ArgumentException ("array.Length - offset < count");

			if (m_buffer_reading) {
				Flush();
				m_buffer_reading = false;
			}

			// reordered to avoid possible integer overflow
			if (m_buffer_pos >= m_buffer.Length - count) {
				Flush ();
				m_stream.Write (array, offset, count);
			} 
			else {
				Array.Copy (array, offset, m_buffer, m_buffer_pos, count);
				m_buffer_pos += count;
			}
		}

		private void CheckObjectDisposedException () 
		{
			if (disposed) {
				throw new ObjectDisposedException ("BufferedStream", 
					Locale.GetText ("Stream is closed"));
			}
		}			
	}
}
