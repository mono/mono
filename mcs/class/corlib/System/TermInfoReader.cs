//
// System.TermInfoReader
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
#if MONO_FEATURE_CONSOLE
using System.IO;
using System.Text;
namespace System {
	// This class reads data from a byte array or file containing the terminfo capabilities
	// information for any given terminal. The maximum allowed size is 4096 (or
	// 32768 for terminfo2) bytes.
	//
	// Terminfo database files are divided in the following sections:
	//
	//	Header
	//	Terminal names
	//	Boolean capabilities
	//	Numeric capabilities
	//	Offset of string capabilities
	//	String capabilities
	//
	// The header is as follows:
	//
	//	Magic number (0x11A/0432 or 0x21e/01036 for terminfo2)
	//	Terminal names size
	//	Boolean section size
	//	Numeric section size
	//	String offsets section size
	//	Strings size
	//
	// After the header, there's a NUL terminated string containing terminal names separated
	// by the character '|'. The last one is the terminal description.
	//
	// The boolean capabilities section has bytes that are set to 1 if the capability is supported
	// and 0 otherwise. If the index of a capability is greater than the section size, 0 is assumed.
	//
	// The numeric capabilities section holds 2-byte integers (4-byte integers for terminfo2) in
	// little endian format. No negative values are allowed and the absence of a capability is marked
	// as two 0xFF (four 0xFF for terminfo2).
	//
	// The string offsets section contains 2-byte integer offsets into the string capabilies section.
	// If the capability is not supported, the index will be two 0xFF bytes.
	//
	// The string capabilities section contains NUL terminated strings starting at the offsets found
	// in the previous section.
	//
	// Terminal capabilities indexes are found in /usr/include/term.h file and described in
	// 'man 5 terminfo'.
	//

	class TermInfoReader {
		int boolSize;
		int numSize;
		int strOffsets;

		//string [] names; // Last one is the description
		byte [] buffer;
		int booleansOffset;
		//string term;

		int intOffset;

		public TermInfoReader (string term, string filename)
		{
			using (FileStream st = File.OpenRead (filename)) {
				long length = st.Length;
				if (length > 4096)
					throw new Exception ("File must be smaller than 4K");

				buffer = new byte [(int) length];
				if (st.Read (buffer, 0, buffer.Length) != buffer.Length)
					throw new Exception ("Short read");

				ReadHeader (buffer, ref booleansOffset);
				ReadNames (buffer, ref booleansOffset);
			}

		}

		public TermInfoReader (string term, byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			this.buffer = buffer;
			ReadHeader (buffer, ref booleansOffset);
			ReadNames (buffer, ref booleansOffset);
		}

//		public string Term {
//			get { return term; }
//		}

		void DetermineVersion (short magic)
		{
			if (magic == 0x11a)
				intOffset = 2;
			else if (magic == 0x21e)
				intOffset = 4;
			else
				throw new Exception (String.Format ("Magic number is unexpected: {0}", magic));
		}

		void ReadHeader (byte [] buffer, ref int position)
		{
			short magic = GetInt16 (buffer, position);
			position += 2;
			DetermineVersion (magic);
			
			/*nameSize =*/ GetInt16 (buffer, position);
			position += 2;
			boolSize = GetInt16 (buffer, position);
			position += 2;
			numSize = GetInt16 (buffer, position);
			position += 2;
			strOffsets = GetInt16 (buffer, position);
			position += 2;
			/*strSize =*/ GetInt16 (buffer, position);
			position += 2;
		}

		void ReadNames (byte [] buffer, ref int position)
		{
			string prev = GetString (buffer, position);
			position += prev.Length + 1;
			//names = prev.Split ('|');
		}

		public bool Get (TermInfoBooleans boolean)
		{
			int x = (int) boolean;
			if (x < 0 || boolean >= TermInfoBooleans.Last || x >= boolSize)
				return false;

			int offset = booleansOffset;
			offset += (int) boolean;
			return (buffer [offset] != 0);
		}

		public int Get (TermInfoNumbers number)
		{
			int x = (int) number;
			if (x < 0 || number >= TermInfoNumbers.Last || x > numSize)
				return -1;

			int offset = booleansOffset + boolSize;
			if ((offset % 2) == 1)
				offset++;

			offset += ((int) number) * intOffset;
			return GetInt16 (buffer, offset);
		}

		public string Get (TermInfoStrings tstr)
		{
			int x = (int) tstr;
			if (x < 0 || tstr >= TermInfoStrings.Last || x > strOffsets)
				return null;

			int offset = booleansOffset + boolSize;
			if ((offset % 2) == 1)
				offset++;

			offset += numSize * intOffset;
			int off2 = GetInt16 (buffer, offset + (int) tstr * 2);
			if (off2 == -1)
				return null;

			return GetString (buffer, offset + strOffsets * 2 + off2);
		}

		public byte [] GetStringBytes (TermInfoStrings tstr)
		{
			int x = (int) tstr;
			if (x < 0 || tstr >= TermInfoStrings.Last || x > strOffsets)
				return null;

			int offset = booleansOffset + boolSize;
			if ((offset % 2) == 1)
				offset++;

			offset += numSize * intOffset;
			int off2 = GetInt16 (buffer, offset + (int) tstr * 2);
			if (off2 == -1)
				return null;

			return GetStringBytes (buffer, offset + strOffsets * 2 + off2);
		}

		short GetInt16 (byte [] buffer, int offset)
		{
			int uno = (int) buffer [offset];
			int dos = (int) buffer [offset + 1];
			if (uno == 255  && dos == 255)
				return -1;

			return (short) (uno + dos * 256);
		}

		string GetString (byte [] buffer, int offset)
		{
			int length = 0;
			int off = offset;
			while (buffer [off++] != 0)
				length++;

			return Encoding.ASCII.GetString (buffer, offset, length);
		}

		byte [] GetStringBytes (byte [] buffer, int offset)
		{
			int length = 0;
			int off = offset;
			while (buffer [off++] != 0)
				length++;

			byte [] result = new byte [length];
			Buffer.InternalBlockCopy (buffer, offset, result, 0, length);
			return result;
		}

		internal static string Escape (string s)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < s.Length; i++) {
				char current = s [i];
				if (Char.IsControl (current)) {
					sb.AppendFormat ("\\x{0:X2}", (int) current);
				} else {
					sb.Append (current);
				}
			}

			return sb.ToString ();
		}
	}
}
#endif

