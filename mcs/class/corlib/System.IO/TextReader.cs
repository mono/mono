//
// System.IO.TextReader
//
// Authors:
//   Marcin Szczepanski (marcins@zipworld.com.au)
//   Miguel de Icaza (miguel@gnome.org)
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

using System;
using System.Runtime.InteropServices;
#if NET_4_5
using System.Threading.Tasks;
#endif

namespace System.IO {

	[Serializable]
	[ComVisible (true)]
#if NET_2_1
	public abstract class TextReader : IDisposable {
#else
	public abstract class TextReader : MarshalByRefObject, IDisposable {
#endif

		sealed class NullTextReader : TextReader
		{
			public override string ReadLine ()
			{
				return null;
			}

			public override string ReadToEnd ()
			{
				return String.Empty;
			}
		}

		public static readonly TextReader Null = new NullTextReader ();

		protected TextReader()
		{
		}
		
		public virtual void Close()
		{ 
			Dispose(true);
		}

		public void Dispose ()
		{
			Dispose(true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing){
				// If we are explicitly disposed, we can avoid finalization.
				GC.SuppressFinalize (this);
			}
			return;
		}
		
		public virtual int Peek()
		{
			return -1;
		}
		
		public virtual int Read()
		{
			return -1;
		}
		
		public virtual int Read ([In, Out] char[] buffer, int index, int count)
		{
			int c, i;
			
			for (i = 0; i < count; i++) {
				if ((c = Read ()) == -1)
					return i;
				buffer [index + i] = (char)c;
			}
			
			return i;
		}
		
		public virtual int ReadBlock ([In, Out] char [] buffer, int index, int count)
		{
			int total_read_count = 0;
			int current_read_count;

			do {
				current_read_count = Read (buffer, index, count);
				index += current_read_count;
				total_read_count += current_read_count;
				count -= current_read_count;
			} while (current_read_count != 0 && count > 0);

			return total_read_count;
		}

		public virtual string ReadLine ()
		{ 
			var result = new System.Text.StringBuilder ();
			int c;

			while ((c = Read ()) != -1){
				// check simple character line ending
				if (c == '\n')
					break;
				if (c == '\r') {
					if (Peek () == '\n') 
						Read ();
					break;
				}
				result.Append ((char) c);
			}
			if (c == -1 && result.Length == 0)
				return null;
			
			return result.ToString ();
		}

		public virtual string ReadToEnd ()
		{ 
			var result = new System.Text.StringBuilder ();
			int c;
			while ((c = Read ()) != -1)
				result.Append ((char) c);
			return result.ToString ();
		}

		public static TextReader Synchronized (TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader is null");
			if (reader is SynchronizedReader)
				return reader;

			return new SynchronizedReader (reader);
		}

#if NET_4_5
		//
		// Use tuple to pack the arguments because it's faster than
		// setting up anonymous method container with an instance delegate
		//
		public virtual Task<int> ReadAsync (char[] buffer, int index, int count)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextReader, char[], int, int>) l;
				return t.Item1.Read (t.Item2, t.Item3, t.Item4);
			}, Tuple.Create (this, buffer, index, count));
		}

		public virtual Task<int> ReadBlockAsync (char[] buffer, int index, int count)
		{
			return Task.Factory.StartNew (l => {
				var t = (Tuple<TextReader, char[], int, int>) l;
				return t.Item1.ReadBlock (t.Item2, t.Item3, t.Item4);
			}, Tuple.Create (this, buffer, index, count));
		}

		public virtual Task<string> ReadLineAsync ()
		{
			return Task.Factory.StartNew (l => ((TextReader) l).ReadLine (), this);
		}

		public virtual Task<string> ReadToEndAsync ()
		{
			return Task.Factory.StartNew (l => ((TextReader) l).ReadToEnd (), this);
		}
#endif
	}

	//
	// Synchronized Reader implementation, used internally.
	//
	[Serializable]
	sealed class SynchronizedReader : TextReader
	{
		readonly TextReader reader;
		
		public SynchronizedReader (TextReader reader)
		{
			this.reader = reader;
		}

		public override void Close ()
		{
			lock (this){
				reader.Close ();
			}
		}

		public override int Peek ()
		{
			lock (this){
				return reader.Peek ();
			}
		}

		public override int ReadBlock (char [] buffer, int index, int count)
		{
			lock (this){
				return reader.ReadBlock (buffer, index, count);
			}
		}

		public override string ReadLine ()
		{
			lock (this){
				return reader.ReadLine ();
			}
		}

		public override string ReadToEnd ()
		{
			lock (this){
				return reader.ReadToEnd ();
			}
		}

		public override int Read ()
		{
			lock (this){
				return reader.Read ();
			}
		}

		public override int Read (char [] buffer, int index, int count)
		{
			lock (this){
				return reader.Read (buffer, index, count);
			}
		}

#if NET_4_5
		public override Task<int> ReadAsync (char[] buffer, int index, int count)
		{
			lock (this) {
				return reader.ReadAsync (buffer, index, count);
			}
		}

		public override Task<int> ReadBlockAsync (char[] buffer, int index, int count)
		{
			lock (this) {
				return reader.ReadBlockAsync (buffer, index, count);
			}
		}

		public override Task<string> ReadLineAsync ()
		{
			lock (this) {
				return reader.ReadLineAsync ();
			}
		}

		public override Task<string> ReadToEndAsync ()
		{
			lock (this) {
				return reader.ReadToEndAsync ();
			}
		}
#endif
	}
}
