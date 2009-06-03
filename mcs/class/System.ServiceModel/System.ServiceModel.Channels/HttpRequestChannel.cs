//
// HttpRequestChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class HttpRequestChannel : RequestChannelBase
	{
		HttpChannelFactory<IRequestChannel> source;
		EndpointAddress address;
		Uri via;

		WebRequest web_request;

		// FIXME: supply maxSizeOfHeaders.
		int max_headers = 0x10000;

		// Constructor

		public HttpRequestChannel (HttpChannelFactory<IRequestChannel> factory,
			EndpointAddress address, Uri via)
			: base (factory)
		{
			this.source = factory;
			this.address = address;
			this.via = via;
		}

		public int MaxSizeOfHeaders {
			get { return max_headers; }
		}

		public MessageEncoder Encoder {
			get { return source.MessageEncoder; }
		}

		public override EndpointAddress RemoteAddress {
			get { return address; }
		}

		public override Uri Via {
			get { return via; }
		}

		// Request

		public override Message Request (Message message, TimeSpan timeout)
		{
			return ProcessRequest (message, timeout);
		}

		Message ProcessRequest (Message message, TimeSpan timeout)
		{
			// FIXME: is distination really like this?
			Uri destination = message.Headers.To ?? Via ?? RemoteAddress.Uri;

			web_request = HttpWebRequest.Create (destination);
			web_request.Method = "POST";
			web_request.ContentType = Encoder.ContentType;

#if !NET_2_1 // FIXME: implement this to not depend on Timeout property
			web_request.Timeout = (int) timeout.TotalMilliseconds;
#endif

			// There is no SOAP Action/To header when AddressingVersion is None.
			if (message.Version.Addressing == AddressingVersion.None) {
				if (message.Headers.Action != null) {
					web_request.Headers ["SOAPAction"] = message.Headers.Action;
					message.Headers.RemoveAll ("Action", message.Version.Addressing.Namespace);
					if (message.Headers.Action != null) throw new Exception (message.Headers.Action);
				}
			}

			// apply HttpRequestMessageProperty if exists.
			bool suppressEntityBody = false;
#if !NET_2_1
			string pname = HttpRequestMessageProperty.Name;
			if (message.Properties.ContainsKey (pname)) {
				HttpRequestMessageProperty hp = (HttpRequestMessageProperty) message.Properties [pname];
				web_request.Headers.Add (hp.Headers);
				web_request.Method = hp.Method;
				// FIXME: do we have to handle hp.QueryString ?
				if (hp.SuppressEntityBody)
					suppressEntityBody = true;
			}
#endif

			if (!suppressEntityBody && String.Compare (web_request.Method, "GET", StringComparison.OrdinalIgnoreCase) != 0) {
				MemoryStream buffer = new MemoryStream ();
				Encoder.WriteMessage (message, buffer);

				if (buffer.Length > int.MaxValue)
					throw new InvalidOperationException ("The argument message is too large.");

#if !NET_2_1
				web_request.ContentLength = (int) buffer.Length;
#endif

#if NET_2_1
				// We can verify cross domain access policy 
				// with full set of headers and target URL.
				if (!CrossDomainAccessManager.Current.IsAllowed (destination, web_request.Headers.AllKeys))
					throw new InvalidOperationException (String.Format ("Cross domain web service access to {0} is not allowed", destination));
#endif

				object state = new object ();
				Stream requestStream = web_request.EndGetRequestStream (web_request.BeginGetRequestStream (delegate (IAsyncResult result) {
					if (result.AsyncState != state)
						throw new InvalidOperationException ("The argument async result has wrong state");
					}, state));
				requestStream.Write (buffer.GetBuffer (), 0, (int) buffer.Length);
				requestStream.Close ();
			}

			WebResponse res;
			Stream resstr;
			try {
				object state = new object ();
				res = web_request.EndGetResponse (web_request.BeginGetResponse (delegate (IAsyncResult result) {
					if (result.AsyncState != state)
						throw new InvalidOperationException ("The argument async result has wrong state");
					}, state));
				resstr = res.GetResponseStream ();
			} catch (WebException we) {
				res = we.Response;
#if NET_2_1 // debug
				Console.WriteLine (we);
#endif
				try {
					// The response might contain SOAP fault. It might not.
					resstr = res.GetResponseStream ();
				} catch (WebException we2) {
#if NET_2_1 // debug
					Console.WriteLine (we2);
#endif
					throw we;
				}
			}
			
			try {
				using (var responseStream = resstr) {
					MemoryStream ms = new MemoryStream ();
					byte [] b = new byte [65536];
					int n = 0;

					while (true) {
						n = responseStream.Read (b, 0, 65536);
						if (n == 0)
							break;
						ms.Write (b, 0, n);
					}
					ms.Seek (0, SeekOrigin.Begin);

					Message ret = Encoder.ReadMessage (
						//responseStream, MaxSizeOfHeaders);
						ms, MaxSizeOfHeaders, res.ContentType);
/*
MessageBuffer buf = ret.CreateBufferedCopy (0x10000);
ret = buf.CreateMessage ();
System.Xml.XmlTextWriter w = new System.Xml.XmlTextWriter (Console.Out);
w.Formatting = System.Xml.Formatting.Indented;
buf.CreateMessage ().WriteMessage (w);
w.Close ();
*/
					return ret;
				}
			} finally {
				res.Close ();
			}
		}

		public override IAsyncResult BeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			ThrowIfDisposedOrNotOpen ();

			return new HttpChannelRequestAsyncResult (this, message, timeout, callback, state);
		}

		public override Message EndRequest (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			HttpChannelRequestAsyncResult r = result as HttpChannelRequestAsyncResult;
			if (r == null)
				throw new InvalidOperationException ("Wrong IAsyncResult");
			r.WaitEnd ();
			return r.Response;
		}

		// Abort

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		// Close

		protected override void OnClose (TimeSpan timeout)
		{
			if (web_request != null)
				web_request.Abort ();
			web_request = null;
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		// Open

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		class HttpChannelRequestAsyncResult : IAsyncResult
		{
			HttpRequestChannel channel;
			Message message;
			TimeSpan timeout;
			AsyncCallback callback;
			object state;
			AutoResetEvent wait;
			bool done, waiting;
			Message response;
			Exception error;

			public HttpChannelRequestAsyncResult (HttpRequestChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
			{
				this.channel = channel;
				this.message = message;
				this.timeout = timeout;
				this.callback = callback;
				this.state = state;

				wait = new AutoResetEvent (false);
				Thread t = new Thread (delegate () {
					try {
						response = channel.ProcessRequest (message, timeout);
						if (callback != null)
							callback (this);
					} catch (Exception ex) {
						error = ex;
					} finally {
						done = true;
						wait.Set ();
					}
				});
				t.Start ();
			}

			public Message Response {
				get { return response; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return wait; }
			}

			public object AsyncState {
				get { return state; }
			}

			public bool CompletedSynchronously {
				get { return done && !waiting; }
			}

			public bool IsCompleted {
				get { return done; }
			}

			public void WaitEnd ()
			{
				if (!done) {
					waiting = true;
					wait.WaitOne (timeout, true);
				}
				if (error != null)
					throw error;
			}
		}
	}
}
