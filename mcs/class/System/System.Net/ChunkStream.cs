//
// System.Net.ChunkStream
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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

using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Net
{
	class ChunkStream
	{
		enum State {
			None,
			Body,
			BodyFinished,
			Trailer
		}

		class Chunk {
			public byte [] Bytes;
			public int Offset;

			public Chunk (byte [] chunk)
			{
				this.Bytes = chunk;
			}

			public int Read (byte [] buffer, int offset, int size)
			{
				int nread = (size > Bytes.Length - Offset) ? Bytes.Length - Offset : size;
				Buffer.BlockCopy (Bytes, Offset, buffer, offset, nread);
				Offset += nread;
				return nread;
			}
		}

		WebHeaderCollection headers;
		int chunkSize;
		int chunkRead;
		State state;
		//byte [] waitBuffer;
		StringBuilder saved;
		bool sawCR;
		bool gotit;
		ArrayList chunks;
		
		public ChunkStream (byte [] buffer, int offset, int size, WebHeaderCollection headers)
		{
			this.headers = headers;
			saved = new StringBuilder ();
			chunks = new ArrayList ();
			chunkSize = -1;
			Write (buffer, offset, size);
		}

		public void ResetBuffer ()
		{
			chunkSize = -1;
			chunkRead = 0;
			chunks.Clear ();
		}
		
		public void WriteAndReadBack (byte [] buffer, int offset, int size, ref int read)
		{
			Write (buffer, offset, offset+read);
			read = Read (buffer, offset, size);
		}

		public int Read (byte [] buffer, int offset, int size)
		{
			return ReadFromChunks (buffer, offset, size);
		}

		int ReadFromChunks (byte [] buffer, int offset, int size)
		{
			int count = chunks.Count;
			int nread = 0;
			for (int i = 0; i < count; i++) {
				Chunk chunk = (Chunk) chunks [i];
				if (chunk == null)
					continue;

				if (chunk.Offset == chunk.Bytes.Length) {
					chunks [i] = null;
					continue;
				}
				
				nread += chunk.Read (buffer, offset + nread, size - nread);
				if (nread == size)
					break;
			}

			return nread;
		}
		
		public void Write (byte [] buffer, int offset, int size)
		{
			InternalWrite (buffer, ref offset, size);
		}
		
		void InternalWrite (byte [] buffer, ref int offset, int size)
		{
			if (state == State.None) {
				state = GetChunkSize (buffer, ref offset, size);
				if (state == State.None)
					return;
				
				saved.Length = 0;
				sawCR = false;
				gotit = false;
			}
			
			if (state == State.Body && offset < size) {
				state = ReadBody (buffer, ref offset, size);
				if (state == State.Body)
					return;
			}
			
			if (state == State.BodyFinished && offset < size) {
				state = ReadCRLF (buffer, ref offset, size);
				if (state == State.BodyFinished)
					return;

				sawCR = false;
			}
			
			if (state == State.Trailer && offset < size) {
				state = ReadTrailer (buffer, ref offset, size);
				if (state == State.Trailer)
					return;

				saved.Length = 0;
				sawCR = false;
				gotit = false;
			}

			if (offset < size)
				InternalWrite (buffer, ref offset, size);
		}

		public bool WantMore {
			get { return (chunkRead != chunkSize || chunkSize != 0 || state != State.None); }
		}

		public int ChunkLeft {
			get { return chunkSize - chunkRead; }
		}
		
		State ReadBody (byte [] buffer, ref int offset, int size)
		{
			if (chunkSize == 0)
				return State.BodyFinished;

			int diff = size - offset;
			if (diff + chunkRead > chunkSize)
				diff = chunkSize - chunkRead;

			byte [] chunk = new byte [diff];
			Buffer.BlockCopy (buffer, offset, chunk, 0, diff);
			chunks.Add (new Chunk (chunk));
			offset += diff;
			chunkRead += diff;
			return (chunkRead == chunkSize) ? State.BodyFinished : State.Body;
				
		}
		
		State GetChunkSize (byte [] buffer, ref int offset, int size)
		{
			char c = '\0';
			while (offset < size) {
				c = (char) buffer [offset++];
				if (c == '\r') {
					if (sawCR)
						throw new ProtocolViolationException ("2 CR found");

					sawCR = true;
					continue;
				}
				
				if (sawCR && c == '\n')
					break;

				if (c == ' ')
					gotit = true;

				if (!gotit)
					saved.Append (c);
			}

			if (!sawCR || c != '\n')
				return State.None;

			chunkRead = 0;
			chunkSize = Int32.Parse (saved.ToString (), NumberStyles.HexNumber);
			if (chunkSize == 0)
				return State.Trailer;

			return State.Body;
		}

		State ReadCRLF (byte [] buffer, ref int offset, int size)
		{
			if (!sawCR) {
				if ((char) buffer [offset++] != '\r')
					throw new ProtocolViolationException ("Expecting \\r");

				sawCR = true;
				if (offset == size)
					return State.BodyFinished;
			}
			
			if ((char) buffer [offset++] != '\n')
				throw new ProtocolViolationException ("Expecting \\n");

			return State.None;
		}

		State ReadTrailer (byte [] buffer, ref int offset, int size)
		{
			char c = '\0';

			// short path
			if ((char) buffer [offset] == '\r') {
				offset++;
				if ((char) buffer [offset] == '\n') {
					offset++;
					return State.None;
				}
				offset--;
			}
			
			int st = 0;
			string stString = "\r\n\r";
			while (offset < size && st < 4) {
				c = (char) buffer [offset++];
				if ((st == 0 || st == 2) && c == '\r') {
					st++;
					continue;
				}

				if ((st == 1 || st == 3) && c == '\n') {
					st++;
					continue;
				}

				if (st > 0) {
					saved.Append (stString.Substring (0, st));
					st = 0;
				}
			}

			if (st < 4)
				return State.Trailer;

			StringReader reader = new StringReader (saved.ToString ());
			string line;
			while ((line = reader.ReadLine ()) != null && line != "")
				headers.Add (line);

			return State.None;
		}
	}
}

