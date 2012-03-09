//
// System.IO.StreamWriter.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


// This is a wrapper around StreamWriter used by System.Console that
// catches IOException so that graphical applications don't suddenly
// get IO errors when their terminal vanishes (ie when they spew debug
// output.)  See UnexceptionalStreamReader too.

using System.Text;
using System;

namespace System.IO {
	internal class UnexceptionalStreamWriter: StreamWriter
	{
/*
		public UnexceptionalStreamWriter (Stream stream)
			: base (stream)
		{
		}
*/
		public UnexceptionalStreamWriter (Stream stream,
						  Encoding encoding)
			: base (stream, encoding)
		{
		}
/*
		public UnexceptionalStreamWriter (Stream stream,
						  Encoding encoding,
						  int bufferSize)
			: base (stream, encoding, bufferSize)
		{
		}

		public UnexceptionalStreamWriter (string path)
			: base (path)
		{
		}

		public UnexceptionalStreamWriter (string path, bool append)
			: base (path, append)
		{
		}

		public UnexceptionalStreamWriter (string path, bool append,
						  Encoding encoding)
			: base (path, append, encoding)
		{
		}

		public UnexceptionalStreamWriter (string path, bool append,
						  Encoding encoding,
						  int bufferSize)
			: base (path, append, encoding, bufferSize)
		{
		}
*/
		public override void Flush ()
		{
			try {
				base.Flush ();
			} catch (Exception) {
			}
		}

		public override void Write (char[] buffer, int index,
					    int count)
		{
			try {
				base.Write (buffer, index, count);
			} catch (Exception) {
			}
		}

		public override void Write (char value)
		{
			try {
				base.Write (value);
			} catch (Exception) {
			}
		}

		public override void Write (char[] value)
		{
			try {
				base.Write (value);
			} catch (Exception) {
			}
		}

		public override void Write (string value)
		{
			try {
				base.Write (value);
			} catch (Exception) {
			}
		}
	}
}
