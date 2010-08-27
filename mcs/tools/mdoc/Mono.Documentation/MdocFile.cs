//
// MdocFile.cs: File utility methods
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using System;
using System.IO;

namespace Mono.Documentation {

	static class MdocFile {

		public static void UpdateFile (string file, Action<string> creator)
		{
			if (!File.Exists (file) || file == "-") {
				creator (file);
				return;
			}

			string temp = Path.GetTempFileName ();
			bool move = true;
			try {
				creator (temp);

				using (var a = File.OpenRead (file))
				using (var b = File.OpenRead (temp)) {
					if (a.Length == b.Length)
						move = !FileContentsIdentical (a, b);
				}

				if (move) {
					File.Delete (file);
					File.Move (temp, file);
				}
			}
			finally {
				if (!move && File.Exists (temp))
					File.Delete (temp);
			}
		}

		static bool FileContentsIdentical (Stream a, Stream b)
		{
			byte[] ba = new byte[4096];
			byte[] bb = new byte[4096];
			int ra, rb;

			while ((ra = a.Read (ba, 0, ba.Length)) > 0 &&
					(rb = b.Read (bb, 0, bb.Length)) > 0) {
				if (ra != rb)
					return false;
				for (int i = 0; i < ra; ++i) {
					if (ba [i] != bb [i])
						return false;
				}
			}
			return true;
		}
	}
}
