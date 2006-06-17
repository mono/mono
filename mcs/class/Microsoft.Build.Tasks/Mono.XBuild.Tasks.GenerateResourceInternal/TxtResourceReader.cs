//
// TxtResourceReader: Reader from monoresgen.
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
	internal class TxtResourceReader : IResourceReader {
		Hashtable data;
		Stream s;

		public TxtResourceReader (Stream stream)
		{
			data = new Hashtable ();
			s = stream;
			Load ();
		}

		public virtual void Close ()
		{
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return data.GetEnumerator ();
		}

		void Load ()
		{
			StreamReader reader = new StreamReader (s);
			string line, key, val;
			int epos, line_num = 0;
			while ((line = reader.ReadLine ()) != null) {
				line_num++;
				line = line.Trim ();
				if (line.Length == 0 || line [0] == '#' ||
				    line [0] == ';')
					continue;
				epos = line.IndexOf ('=');
				if (epos < 0) 
					throw new Exception ("Invalid format at line " + line_num);
				key = line.Substring (0, epos);
				val = line.Substring (epos + 1);
				key = key.Trim ();
				val = val.Trim ();
				if (key.Length == 0) 
					throw new Exception ("Key is empty at line " + line_num);
				data.Add (key, val);
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator();
		}

		void IDisposable.Dispose ()
		{
		}
	}
}

#endif