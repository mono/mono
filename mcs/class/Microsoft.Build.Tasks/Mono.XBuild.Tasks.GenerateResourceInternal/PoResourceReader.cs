//
// PoResourceReader.cs: Reader from monoresgen.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Paolo Molaro (lupus@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Resources;
using System.Text;

namespace Mono.XBuild.Tasks.GenerateResourceInternal {
	internal class PoResourceReader : IResourceReader {
		Hashtable data;
		Stream s;
		int line_num;

		public PoResourceReader (Stream stream)
		{
			data = new Hashtable ();
			s = stream;
			Load ();
		}

		public virtual void Close ()
		{
			s.Close ();
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return data.GetEnumerator ();
		}

		string GetValue (string line)
		{
			int begin = line.IndexOf ('"');
			if (begin == -1)
				throw new FormatException (String.Format ("No begin quote at line {0}: {1}", line_num, line));

			int end = line.LastIndexOf ('"');
			if (end == -1)
				throw new FormatException (String.Format ("No closing quote at line {0}: {1}", line_num, line));

			return line.Substring (begin + 1, end - begin - 1);
		}

		void Load ()
		{
			StreamReader reader = new StreamReader (s);
			string line;
			string msgid = null;
			string msgstr = null;
			bool ignoreNext = false;

			while ((line = reader.ReadLine ()) != null) {
				line_num++;
				line = line.Trim ();
				if (line.Length == 0)
					continue;
					
				if (line [0] == '#') {
					if (line.Length == 1 || line [1] != ',')
						continue;

					if (line.IndexOf ("fuzzy") != -1) {
						ignoreNext = true;
						if (msgid != null) {
							if (msgstr == null)
								throw new FormatException ("Error. Line: " + line_num);
							data.Add (msgid, msgstr);
							msgid = null;
							msgstr = null;
						}
					}
					continue;
				}
				
				if (line.StartsWith ("msgid ")) {
					if (msgid == null && msgstr != null)
						throw new FormatException ("Found 2 consecutive msgid. Line: " + line_num);

					if (msgstr != null) {
						if (!ignoreNext)
							data.Add (msgid, msgstr);

						ignoreNext = false;
						msgid = null;
						msgstr = null;
					}

					msgid = GetValue (line);
					continue;
				}

				if (line.StartsWith ("msgstr ")) {
					if (msgid == null)
						throw new FormatException ("msgstr with no msgid. Line: " + line_num);

					msgstr = GetValue (line);
					continue;
				}

				if (line [0] == '"') {
					if (msgid == null || msgstr == null)
						throw new FormatException ("Invalid format. Line: " + line_num);

					msgstr += GetValue (line);
					continue;
				}

				throw new FormatException ("Unexpected data. Line: " + line_num);
			}

			if (msgid != null) {
				if (msgstr == null)
					throw new FormatException ("Expecting msgstr. Line: " + line_num);

				if (!ignoreNext)
					data.Add (msgid, msgstr);
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator();
		}

		void IDisposable.Dispose ()
		{
			if (data != null)
				data = null;

			if (s != null) {
				s.Close ();
				s = null;
			}
		}
	}
}

#endif