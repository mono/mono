//
// TxtResourceWriter: Writer from monoresgen.
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
using System.IO;
using System.Resources;
using System.Text;

namespace Mono.XBuild.Tasks.GenerateResourceInternal {
	internal class TxtResourceWriter : IResourceWriter {
		StreamWriter s;

		public TxtResourceWriter (Stream stream)
		{
			s = new StreamWriter (stream);
		}

		public void AddResource (string name, byte[] value)
		{
			throw new Exception ("Binary data not valid in a text resource file");
		}

		public void AddResource (string name, object value)
		{
			if (value is string) {
				AddResource (name, (string)value);
				return;
			}
			throw new Exception ("Objects not valid in a text resource file");
		}

		public void AddResource (string name, string value)
		{
			s.WriteLine ("{0}={1}", name, value);
		}

		public void Close ()
		{
			s.Close ();
		}

		public void Dispose ()
		{
		}

		public void Generate ()
		{
		}
	}
}

#endif