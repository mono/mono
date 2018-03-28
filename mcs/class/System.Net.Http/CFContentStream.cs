//
// CFContentStream.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;

#if XAMCORE_4_0
using CFNetwork;
using CoreFoundation;
#elif XAMCORE_2_0
using CoreServices;
using CoreFoundation;
#else
using MonoTouch.CoreServices;
using MonoTouch.CoreFoundation;
#endif

namespace System.Net.Http
{
	class BufferData 
	{
		public byte[] Buffer;
		public int Length;
	}

	class CFContentStream : HttpContent
	{
		readonly CFHTTPStream http_stream;
		BufferData data;
		Mutex data_mutex;
		AutoResetEvent data_event;
		AutoResetEvent data_read_event;
		ExceptionDispatchInfo http_exception;

		// The requirements are:
		// * We must read at least one byte from the stream every time
		//   we get a HasBytesAvailable event.
		// * SerializeToStreamAsync is executed on a separate thread,
		//   so reads must somehow be synchronized with that thread.
		//
		// Current implementation:
		// * We read data in ReadStreamData (on the same thread
		//   we got the HasBytesAvailable event, i.e. inside the 
		//   HasBytesAvailable event handler).
		// * Data is stored in a class-level buffer.
		// * SerializeToStreamAsync blocks while waiting for
		//   data from ReadStreamData.
		// * ReadStreamData will only read more data once SerializeToStreamAsync
		//   has consumed any existing data. This means we'll be
		//   blocking in the HasBytesAvailable event handler until
		//   any previously read data has been processed (this prevents
		//   any unbound memory growth).

		public CFContentStream (CFHTTPStream stream)
		{
			this.http_stream = stream;
			this.http_stream.ErrorEvent += HandleErrorEvent;
			data = new BufferData () {
				Buffer = new byte [4096],
			};
			data_event = new AutoResetEvent (false);
			data_read_event = new AutoResetEvent (true);
			data_mutex = new Mutex ();
		}

		void HandleErrorEvent (object sender, CFStream.StreamEventArgs e)
		{
			var gotMutex = data_mutex.WaitOne ();
			if (gotMutex) {
				var stream = (CFHTTPStream)sender;
				if (e.EventType == CFStreamEventType.ErrorOccurred)
					Volatile.Write (ref http_exception, ExceptionDispatchInfo.Capture (stream.GetError ()));
				data_mutex.ReleaseMutex ();
			}
		}

		public void ReadStreamData ()
		{
			data_read_event.WaitOne (); // make sure there's no pending data.

			data_mutex.WaitOne ();
			data.Length = (int) http_stream.Read (data.Buffer, 0, data.Buffer.Length);
			data_mutex.ReleaseMutex ();

			data_event.Set ();
		}

		public void Close ()
		{
			data_read_event.WaitOne (); // make sure there's no pending data

			data_mutex.WaitOne ();
			data = null;
			this.http_stream.ErrorEvent -= HandleErrorEvent;
			data_mutex.ReleaseMutex ();

			data_event.Set ();
		}

		protected internal override async Task SerializeToStreamAsync (Stream stream, TransportContext context)
		{
			while (data_event.WaitOne ()) {
				data_mutex.WaitOne ();
				if (http_exception != null) {
					http_exception.Throw ();
					data_mutex.ReleaseMutex ();
					break;
				}
				if (data == null || data.Length <= 0) {
					data_mutex.ReleaseMutex ();
					data_read_event.Set ();
					break;
				}

				await stream.WriteAsync (data.Buffer, 0, data.Length).ConfigureAwait (false);
				data_mutex.ReleaseMutex ();

				data_read_event.Set ();
			}
		}

		protected internal override bool TryComputeLength (out long length)
		{
			length = 0;
			return false;
		}
	}
}
