// ILReader.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.IO;
using System.Text;

namespace Mono.ILASM {


	/// <summary>
	/// </summary>
	public class ILReader {
		internal static readonly int CHUNK_SIZE = 4096;

		private StreamReader reader;
		private int inUse;
		private int pos;
		private int termPos;
		private Location location;
		private Location markedLocation;
		private char[] buffer;

		public ILReader (StreamReader reader)
		{
			this.reader = reader;
			location = new Location ();
			markedLocation = Location.Unknown;
			buffer = new char [CHUNK_SIZE];
			pos = buffer.Length + 1;
			termPos = buffer.Length + 2;
			inUse = 0;
		}



		/// <summary>
		/// </summary>
		public Location Location {
			get {
				return location;
			}
		}


		/// <summary>
		/// Provides access to underlying StreamReader.
		/// </summary>
		public StreamReader BaseReader {
			get {
				return reader;
			}
		}


		private void EnsureCapacity (int required, int shift)
		{
			int n = buffer.Length;

			if (shift < 0) shift = 0;

			if (n < required) {
				for (;n < required; n <<= 1);
				char [] newBuff = new char [n];
				Array.Copy (buffer, 0, newBuff, shift, buffer.Length);
			}
		}

		private void EnsureCapacity (int required)
		{
			EnsureCapacity (required, 0);
		}

		private int NormalizeEOL (int len)
		{
			if (len <= 0) return -1;
			int n = len;

			for (int i = 0, j = 0; i < len; i++, j++) {
				if (buffer [i] == '\r') {
					if (i == len - 1) {
						// strip the very last char
						--n;
						break;
					} else {
						buffer [j] = '\n';
						if (buffer [i + 1] == '\n') {
							i++;
							--n;
						}
					}
				} else if (i != j) {
					buffer [j] = buffer [i];
				}

			}

			return n;
		}



		private int DoRead (int advance)
		{
			int n = 1;

			if (pos == termPos) {
				n = -1;
			} else if (pos >= inUse) {
				pos = 0;

				// read as much as possible
				n = reader.Read (buffer, 0, buffer.Length);
				n = NormalizeEOL (n);

				// Manually terminate the buffer if we had
				// reached the end of the input stream.
				if (n != -1 && n < buffer.Length) termPos = n + 1;
				
				inUse = n;
			}

			int res = -1;
			if (n != -1) {
				res = buffer [pos];
			}
			pos += advance;


			// Track location
			if (res == '\n') {
				location.NewLine();
			} else {
				location.NextColumn();
			}

			return res;
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public int Read ()
		{
			return DoRead (1);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public int Peek ()
		{
			return DoRead (0);
		}


		/// <summary>
		/// </summary>
		public void Unread (char c)
		{
			if (pos > 0) {
				if (buffer [pos - 1] == c) {
					--pos;
				} else {
					throw new Exception ("Invalid putback.");
				}
			} else {
				EnsureCapacity (++inUse, 1);
				pos = 0;
				buffer [0] = c;
			}

			location.PreviousColumn ();
		}


		/// <summary>
		/// </summary>
		/// <param name="chars"></param>
		public void Unread (char [] chars)
		{
			int len = chars.Length;

			if (len == 0) return;

			if (pos >= len) {
				pos -= len;
			} else {
				inUse += len;
				EnsureCapacity (inUse, len);
				pos -= len;
				Array.Copy (chars, 0, buffer, pos, len);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="c"></param>
		public void Unread (int c)
		{
			Unread ((char)c);
		}


		/// <summary>
		/// </summary>
		public void SkipWhitespace ()
		{
			int ch = Read ();
			for (; ch != -1 && Char.IsWhiteSpace((char) ch); ch = Read ());
			if (ch != -1) Unread (ch);
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public string ReadToWhitespace ()
		{
			StringBuilder sb = new StringBuilder ();
			int ch = Read ();
			for (; ch != -1 && !Char.IsWhiteSpace((char) ch); sb.Append ((char) ch), ch = Read ());
			if (ch != -1) Unread (ch);
			return sb.ToString ();
		}


		/// <summary>
		/// </summary>
		public void MarkLocation ()
		{
			if (markedLocation == Location.Unknown) {
				markedLocation = new Location (location);
			} else {
				markedLocation.CopyFrom (location);
			}
		}


		/// <summary>
		/// </summary>
		public void RestoreLocation ()
		{
			if (markedLocation != Location.Unknown) {
				location.CopyFrom (markedLocation);
			}
		}

	}

}
