//
// System.IO.StringWriter
//
// Authors
//	Marcin Szczepanski (marcins@zipworld.com.au)
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Marek Safar (marek.safar@gmail.com)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc.
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

using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;
#if NET_4_5
using System.Threading.Tasks;
#endif

namespace System.IO
{
	[Serializable]
	[ComVisible (true)]
	[MonoLimitation ("Serialization format not compatible with .NET")]
	public class StringWriter : TextWriter
	{
		private StringBuilder internalString;
		private bool disposed;

		public StringWriter ()
			: this (new StringBuilder ())
		{
		}

		public StringWriter (IFormatProvider formatProvider)
			: this (new StringBuilder (), formatProvider)
		{
		}

		public StringWriter (StringBuilder sb)
			: this (sb, null)
		{
		}

		public StringWriter (StringBuilder sb, IFormatProvider formatProvider)
		{
			if (sb == null)
				throw new ArgumentNullException ("sb");

			internalString = sb;
			internalFormatProvider = formatProvider;
		}

		public override Encoding Encoding {
			get {
				return Encoding.Unicode;
			}
		}

		public override void Close ()
		{
			Dispose (true);
			disposed = true;
		}

		protected override void Dispose (bool disposing)
		{
			// MS.NET doesn't clear internal buffer.
			// internalString = null;
			base.Dispose (disposing);
			disposed = true;
		}

		public virtual StringBuilder GetStringBuilder ()
		{
			return internalString;
		}

		public override string ToString ()
		{
			return internalString.ToString ();
		}

		public override void Write (char value)
		{
			if (disposed) {
				throw new ObjectDisposedException ("StringReader",
					Locale.GetText ("Cannot write to a closed StringWriter"));
			}

			internalString.Append (value);
		}

		public override void Write (string value)
		{
			if (disposed) {
				throw new ObjectDisposedException ("StringReader",
					Locale.GetText ("Cannot write to a closed StringWriter"));
			}

			internalString.Append (value);
		}

		public override void Write (char[] buffer, int index, int count)
		{
			if (disposed) {
				throw new ObjectDisposedException ("StringReader",
					Locale.GetText ("Cannot write to a closed StringWriter"));
			}
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
			if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			internalString.Append (buffer, index, count);
		}

#if NET_4_5
		public override Task FlushAsync ()
		{
			// it appears to do nothing
			return TaskConstants.Finished;
		}

		//
		// All async methods return finished task with a result as it's faster
		// than setting up async call
		//
		public override Task WriteAsync (char value)
		{
			Write (value);
			return TaskConstants.Finished;
		}

		public override Task WriteAsync (char[] buffer, int index, int count)
		{
			Write (buffer, index, count);
			return TaskConstants.Finished;
		}

		public override Task WriteAsync (string value)
		{
			Write (value);
			return TaskConstants.Finished;
		}

		public override Task WriteLineAsync (char value)
		{
			WriteLine (value);
			return TaskConstants.Finished;
		}

		public override Task WriteLineAsync (char[] buffer, int index, int count)
		{
			WriteLine (buffer, index, count);
			return TaskConstants.Finished;
		}

		public override Task WriteLineAsync (string value)
		{
			WriteLine (value);
			return TaskConstants.Finished;
		}
#endif
	}
}