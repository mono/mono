// Imported from https://github.com/kumpera/SimpleJIT/blob/77a7f3a7fcd971426bd3f6d3416eab6e42bc535b/src/SimpleJit.Extensions/StreamExtensions.cs
//
// StreamExtensions.cs
//
// Author:
//   Rodrigo Kumpera  <kumpera@gmail.com>
//
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

using System.IO;

namespace SimpleJit.Extensions {

public static class StreamExtensions {
	public static void WriteInt (this Stream stream, int val) {
		stream.WriteByte ((byte)(val & 0xFF));
		stream.WriteByte ((byte)((val >> 8) & 0xFF));
		stream.WriteByte ((byte)((val >> 16) & 0xFF));
		stream.WriteByte ((byte)((val >> 24) & 0xFF));
	}

	public static short ReadShort (this Stream stream) {
		int low = stream.ReadByte ();
		return (short) ((low << 8) | stream.ReadByte ());
	}

	public static int ReadInt (this Stream stream) {
		int low = stream.ReadByte ();
		int mid0 = stream.ReadByte ();
		int mid1 = stream.ReadByte ();
		return (low << 8) | (mid0 << 16) | (mid1 << 24) | stream.ReadByte ();
	}

	public static void Skip (this Stream stream, int count) {
		while (count-- > 0)
			stream.ReadByte ();
	}

}

}
