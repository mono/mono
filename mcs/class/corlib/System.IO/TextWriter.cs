//
// System.IO.TextWriter.cs
//
// Authors:
//   Marcin Szczepanski (marcins@zipworld.com.au)
//   Miguel de Icaza (miguel@gnome.org)
//   Paolo Molaro (lupus@ximian.com)
//   Marek Safar (marek.safar@gmail.com)

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

using System.Text;
using System.Runtime.InteropServices;
#if NET_4_5
using System.Threading.Tasks;
#endif

namespace System.IO
{

	[Serializable]
	[ComVisible (true)]
#if NET_2_1
	public abstract class TextWriter : IDisposable {
#else
	public abstract class TextWriter : MarshalByRefObject, IDisposable
	{
#endif
		//
		// Null version of the TextWriter, for the `Null' instance variable
		//
		sealed class NullTextWriter : TextWriter
		{
			public override Encoding Encoding
			{
				get
				{
					return Encoding.Default;
				}
			}

			public override void Write (string s)
			{
			}
			public override void Write (char value)
			{
			}
			public override void Write (char[] value, int index, int count)
			{
			}
		}

		protected TextWriter ()
		{
			CoreNewLine = System.Environment.NewLine.ToCharArray ();
		}

		protected TextWriter (IFormatProvider formatProvider)
		{
			CoreNewLine = System.Environment.NewLine.ToCharArray ();
			internalFormatProvider = formatProvider;
		}

		protected char[] CoreNewLine;

		internal IFormatProvider internalFormatProvider;

		public static readonly TextWriter Null = new NullTextWriter ();

		public abstract Encoding Encoding { get; }

		public virtual IFormatProvider FormatProvider {
			get {
				return internalFormatProvider;
			}
		}

		public virtual string NewLine {
			get {
				return new string (CoreNewLine);
			}

			set {
				if (value == null)
					value = Environment.NewLine;

				CoreNewLine = value.ToCharArray ();
			}
		}

		public virtual void Close ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				// If we are explicitly disposed, we can avoid finalization.
				GC.SuppressFinalize (this);
			}
		}
		public void Dispose ()
		{
			Dispose (true);

			// If we are explicitly disposed, we can avoid finalization.
			GC.SuppressFinalize (this);
		}

		public virtual void Flush ()
		{
			// do nothing
		}

		public static TextWriter Synchronized (TextWriter writer)
		{
			return Synchronized (writer, false);
		}

		internal static TextWriter Synchronized (TextWriter writer, bool neverClose)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer is null");

			if (writer is SynchronizedWriter)
				return writer;

			return new SynchronizedWriter (writer, neverClose);
		}

		public virtual void Write (bool value)
		{
			Write (value.ToString ());
		}

		public virtual void Write (char value)
		{
			// Do nothing
		}

		public virtual void Write (char[] buffer)
		{
			if (buffer == null)
				return;
			Write (buffer, 0, buffer.Length);
		}

		public virtual void Write (decimal value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		public virtual void Write (double value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		public virtual void Write (int value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		public virtual void Write (long value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		public virtual void Write (object value)
		{
			if (value != null)
				Write (value.ToString ());
		}

		public virtual void Write (float value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		public virtual void Write (string value)
		{
			if (value != null)
				Write (value.ToCharArray ());
		}

		[CLSCompliant (false)]
		public virtual void Write (uint value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		[CLSCompliant (false)]
		public virtual void Write (ulong value)
		{
			Write (value.ToString (internalFormatProvider));
		}

		public virtual void Write (string format, object arg0)
		{
			Write (String.Format (format, arg0));
		}

		public virtual void Write (string format, params object[] arg)
		{
			Write (String.Format (format, arg));
		}

		public virtual void Write (char[] buffer, int index, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0 || index > buffer.Length)
				throw new ArgumentOutOfRangeException ("index");
			// re-ordered to avoid possible integer overflow
			if (count < 0 || (index > buffer.Length - count))
				throw new ArgumentOutOfRangeException ("count");

			for (; count > 0; --count, ++index) {
				Write (buffer[index]);
			}
		}

		public virtual void Write (string format, object arg0, object arg1)
		{
			Write (String.Format (format, arg0, arg1));
		}

		public virtual void Write (string format, object arg0, object arg1, object arg2)
		{
			Write (String.Format (format, arg0, arg1, arg2));
		}

		public virtual void WriteLine ()
		{
			Write (CoreNewLine);
		}

		public virtual void WriteLine (bool value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (char value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (char[] buffer)
		{
			Write (buffer);
			WriteLine ();
		}

		public virtual void WriteLine (decimal value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (double value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (int value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (long value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (object value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (float value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (string value)
		{
			Write (value);
			WriteLine ();
		}

		[CLSCompliant (false)]
		public virtual void WriteLine (uint value)
		{
			Write (value);
			WriteLine ();
		}

		[CLSCompliant (false)]
		public virtual void WriteLine (ulong value)
		{
			Write (value);
			WriteLine ();
		}

		public virtual void WriteLine (string format, object arg0)
		{
			Write (format, arg0);
			WriteLine ();
		}

		public virtual void WriteLine (string format, params object[] arg)
		{
			Write (format, arg);
			WriteLine ();
		}

		public virtual void WriteLine (char[] buffer, int index, int count)
		{
			Write (buffer, index, count);
			WriteLine ();
		}

		public virtual void WriteLine (string format, object arg0, object arg1)
		{
			Write (format, arg0, arg1);
			WriteLine ();
		}

		public virtual void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			Write (format, arg0, arg1, arg2);
			WriteLine ();
		}

#if NET_4_5
		public virtual Task FlushAsync ()
		{
			return Task.Factory.StartNew (l => ((TextWriter)l).Flush (), this);
		}

		//
		// Use tuple to pack the arguments because it's faster than
		// setting up anonymous method container with an instance delegate
		//
		public virtual Task WriteAsync (char value)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, char>) l;
				t.Item1.Write (t.Item2);
			}, Tuple.Create (this, value));
		}

		public Task WriteAsync (char[] buffer)
		{
			if (buffer == null)
				return TaskConstants.Finished;

			return WriteAsync (buffer, 0, buffer.Length);
		}

		public virtual Task WriteAsync (char[] buffer, int index, int count)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, char[], int, int>) l;
				t.Item1.Write (t.Item2, t.Item3, t.Item4);
			}, Tuple.Create (this, buffer, index, count));
		}

		public virtual Task WriteAsync (string value)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, string>) l;
				t.Item1.Write (t.Item2);
			}, Tuple.Create (this, value));
		}

		public virtual Task WriteLineAsync ()
		{
			return WriteAsync (CoreNewLine);
		}

		public virtual Task WriteLineAsync (char value)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, char>) l;
				t.Item1.WriteLine (t.Item2);
			}, Tuple.Create (this, value));
		}

		public Task WriteLineAsync (char[] buffer)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, char[]>) l;
				t.Item1.WriteLine (t.Item2);
			}, Tuple.Create (this, buffer));
		}

		public virtual Task WriteLineAsync (char[] buffer, int index, int count)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, char[], int, int>) l;
				t.Item1.WriteLine (t.Item2, t.Item3, t.Item4);
			}, Tuple.Create (this, buffer, index, count));
		}

		public virtual Task WriteLineAsync (string value)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextWriter, string>) l;
				t.Item1.WriteLine (t.Item2);
			}, Tuple.Create (this, value));
		}
#endif
	}

	//
	// Sychronized version of the TextWriter.
	//
	[Serializable]
	sealed class SynchronizedWriter : TextWriter
	{
		private TextWriter writer;
		private bool neverClose;

		public SynchronizedWriter (TextWriter writer)
			: this (writer, false)
		{
		}

		public SynchronizedWriter (TextWriter writer, bool neverClose)
		{
			this.writer = writer;
			this.neverClose = neverClose;
		}

		public override void Close ()
		{
			if (neverClose)
				return;
			lock (this) {
				writer.Close ();
			}
		}

		public override void Flush ()
		{
			lock (this) {
				writer.Flush ();
			}
		}

		#region Write methods
		public override void Write (bool value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (char value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (char[] value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (Decimal value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (int value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (long value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (object value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (float value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (string value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (uint value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (ulong value)
		{
			lock (this) {
				writer.Write (value);
			}
		}

		public override void Write (string format, object value)
		{
			lock (this) {
				writer.Write (format, value);
			}
		}

		public override void Write (string format, object[] value)
		{
			lock (this) {
				writer.Write (format, value);
			}
		}

		public override void Write (char[] buffer, int index, int count)
		{
			lock (this) {
				writer.Write (buffer, index, count);
			}
		}

		public override void Write (string format, object arg0, object arg1)
		{
			lock (this) {
				writer.Write (format, arg0, arg1);
			}
		}

		public override void Write (string format, object arg0, object arg1, object arg2)
		{
			lock (this) {
				writer.Write (format, arg0, arg1, arg2);
			}
		}
		#endregion
		#region WriteLine methods
		public override void WriteLine ()
		{
			lock (this) {
				writer.WriteLine ();
			}
		}

		public override void WriteLine (bool value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (char value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (char[] value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (Decimal value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (double value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (int value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (long value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (object value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (float value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (string value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (uint value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (ulong value)
		{
			lock (this) {
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (string format, object value)
		{
			lock (this) {
				writer.WriteLine (format, value);
			}
		}

		public override void WriteLine (string format, object[] value)
		{
			lock (this) {
				writer.WriteLine (format, value);
			}
		}

		public override void WriteLine (char[] buffer, int index, int count)
		{
			lock (this) {
				writer.WriteLine (buffer, index, count);
			}
		}

		public override void WriteLine (string format, object arg0, object arg1)
		{
			lock (this) {
				writer.WriteLine (format, arg0, arg1);
			}
		}

		public override void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			lock (this) {
				writer.WriteLine (format, arg0, arg1, arg2);
			}
		}
		#endregion

#if NET_4_5
		public override Task FlushAsync ()
		{
			lock (this) {
				return writer.FlushAsync ();
			}
		}

		public override Task WriteAsync (char value)
		{
			lock (this) {
				return writer.WriteAsync (value);
			}
		}

		public override Task WriteAsync (char[] buffer, int index, int count)
		{
			lock (this) {
				return writer.WriteAsync (buffer, index, count);
			}
		}

		public override Task WriteAsync (string value)
		{
			lock (this) {
				return writer.WriteAsync (value);
			}
		}

		public override Task WriteLineAsync ()
		{
			lock (this) {
				return writer.WriteLineAsync ();
			}
		}

		public override Task WriteLineAsync (char value)
		{
			lock (this) {
				return writer.WriteLineAsync (value);
			}
		}

		public override Task WriteLineAsync (char[] buffer, int index, int count)
		{
			lock (this) {
				return writer.WriteLineAsync (buffer, index, count);
			}
		}

		public override Task WriteLineAsync (string value)
		{
			lock (this) {
				return writer.WriteLineAsync (value);
			}
		}
#endif
		public override Encoding Encoding {
			get {
				lock (this) {
					return writer.Encoding;
				}
			}
		}

		public override IFormatProvider FormatProvider {
			get {
				lock (this) {
					return writer.FormatProvider;
				}
			}
		}

		public override string NewLine {
			get {
				lock (this) {
					return writer.NewLine;
				}
			}

			set {
				lock (this) {
					writer.NewLine = value;
				}
			}
		}
	}
}
