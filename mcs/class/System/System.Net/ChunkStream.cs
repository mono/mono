//
// System.Net.ChunkStream
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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

		MemoryStream ms;
		WebHeaderCollection headers;
		int chunkSize;
		int chunkRead;
		State state;
		//byte [] waitBuffer;
		StringBuilder saved;
		bool sawCR;
		bool gotit;
		long readPosition;
		
		public ChunkStream (byte [] buffer, int offset, int size, WebHeaderCollection headers)
		{
			this.headers = headers;
			ms = new MemoryStream ();
			saved = new StringBuilder ();
			chunkSize = -1;
			if (offset < size)
				Write (buffer, offset, size);
		}

		public void ResetBuffer ()
		{
			ms.SetLength (0);
			readPosition = 0;
			chunkSize = -1;
			chunkRead = 0;
		}
		
		public void WriteAndReadBack (byte [] buffer, int offset, int size, ref int read)
		{
			Write (buffer, offset, read);
			read = Read (buffer, offset, size);
		}

		public int Read (byte [] buffer, int offset, int size)
		{
			ms.Position = readPosition;
			int r = ms.Read (buffer, offset, size);
			readPosition += r;
			ms.Position = ms.Length;
			return r;
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
			get { return (chunkRead != chunkSize || chunkSize != 0); }
		}
		
		public bool EOF {
			get { return (Available == 0); }
		}
		
		public int Available {
			get { return (int) (ms.Length - readPosition); }
		}
		
		State ReadBody (byte [] buffer, ref int offset, int size)
		{
			if (chunkSize == 0)
				return State.BodyFinished;

			int diff = size - offset;
			if (diff + chunkRead > chunkSize)
				diff = chunkSize - chunkRead;

			ms.Write (buffer, offset, diff);
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

