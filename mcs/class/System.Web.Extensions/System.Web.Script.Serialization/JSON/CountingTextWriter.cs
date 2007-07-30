//
// CountingTextWriter.cs
//
// Author:
//   Adar Wesley <adarw@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace System.Web.Extensions.System.Web.Script.Serialization.JSON
{
	class CountingTextWriter : TextWriter
	{
		public CountingTextWriter (TextWriter writer)
			: base () 
		{
			realWriter = writer;
		}

		public override void Close () 
		{
			realWriter.Close ();
		}

		public override void Flush () 
		{
			realWriter.Flush ();
		}

		public override void Write (char value) 
		{
			if (MaxJsonLength > 0 && (writtenChars + 1) > MaxJsonLength) {
				throw new InvalidOperationException ("Maximum length exceeded.");
			}
			realWriter.Write (value);
			writtenChars++;
		}

		public override void Write (string value) 
		{
			if (MaxJsonLength > 0 && (writtenChars + value.Length) > MaxJsonLength) {
				throw new InvalidOperationException ("Maximum length exceeded.");
			}
			realWriter.Write (value);
			writtenChars += value.Length;
		}

		public override Encoding Encoding 
		{
			get { return realWriter.Encoding; }
		}

		public int MaxJsonLength {
			get { return maxJsonLength; }
			set { maxJsonLength = value; }
		}

		public int WrittenChars {
			get { return writtenChars; }
		}

		private TextWriter realWriter;
		private int maxJsonLength;
		private int writtenChars;
	}
}
