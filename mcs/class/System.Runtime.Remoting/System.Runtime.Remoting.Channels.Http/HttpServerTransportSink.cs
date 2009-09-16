//
// HttpServerTransportSink.cs
// 
// Author:
//   Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
#if !NET_2_0
using MonoHttp;
#endif

namespace System.Runtime.Remoting.Channels.Http
{
	class HttpServerTransportSink : IServerChannelSink
	{

		IServerChannelSink nextSink;

		public HttpServerTransportSink (IServerChannelSink nextSink)
		{
			this.nextSink = nextSink;
		}

		public IServerChannelSink NextChannelSink
		{
			get { return nextSink; }
		}

		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
			IMessage msg, ITransportHeaders headers, Stream responseStream)
		{
			ContextWithId ctx = (ContextWithId) state;
			WriteOut (ctx.Context, headers, responseStream);
			ctx.Context.Response.Close ();
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
			IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack, IMessage requestMsg,
			ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg,
			out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			//we know we never call this
			throw new NotSupportedException ();
		}

		public IDictionary Properties
		{
			get
			{
				if (nextSink != null)
					return nextSink.Properties;
				else return null;
			}
		}

		internal void HandleRequest (HttpListenerContext context)
		{
			//build the headers
			ITransportHeaders requestHeaders = new TransportHeaders ();
			System.Collections.Specialized.NameValueCollection httpHeaders = context.Request.Headers;
			foreach (string key in httpHeaders.Keys) {
				requestHeaders[key] = httpHeaders[key];
			}

			//get an ID for this connection
			ContextWithId identitiedContext = new ContextWithId (context);

			requestHeaders[CommonTransportKeys.RequestUri] = context.Request.Url.PathAndQuery;
			requestHeaders[CommonTransportKeys.IPAddress] = context.Request.RemoteEndPoint.Address;
			requestHeaders[CommonTransportKeys.ConnectionId] = identitiedContext.ID;
			requestHeaders["__RequestVerb"] = context.Request.HttpMethod;
			requestHeaders["__HttpVersion"] = string.Format ("HTTP/{0}.{1}",
				context.Request.ProtocolVersion.Major, context.Request.ProtocolVersion.Minor);

			if (RemotingConfiguration.CustomErrorsEnabled (context.Request.IsLocal))
				requestHeaders["__CustomErrorsEnabled"] = false;

			IMessage responseMsg;
			Stream responseStream;
			ITransportHeaders responseHeaders;
			
			// attach the context as state so that our async handler can use it to send the response
			ServerChannelSinkStack sinkStack = new ServerChannelSinkStack ();
			sinkStack.Push (this, identitiedContext);

			// NOTE: if we copy the InputStream before passing it so the sinks, the .NET formatters have 
			// unspecified internal errors. Let's hope they don't need to seek the stream!
			ServerProcessing proc = nextSink.ProcessMessage (sinkStack, null, requestHeaders, context.Request.InputStream, 
				out responseMsg, out responseHeaders, out responseStream);
			
			switch (proc) {
			case ServerProcessing.Complete:
				WriteOut (context, responseHeaders, responseStream);
				context.Response.Close ();
				break;

			case ServerProcessing.Async:
				break;

			case ServerProcessing.OneWay:
				context.Response.Close ();
				break;
			}

		}
		
		internal ServerProcessing SynchronousDispatch (ITransportHeaders requestHeaders, Stream requestStream,
			out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			IMessage responseMsg;
			ContextWithId identitiedContext = new ContextWithId (null);
			
			// attach the context as state so that our async handler can use it to send the response
			ServerChannelSinkStack sinkStack = new ServerChannelSinkStack ();
			sinkStack.Push (this, identitiedContext);
			
			return nextSink.ProcessMessage (sinkStack, null, requestHeaders, requestStream, 
				out responseMsg, out responseHeaders, out responseStream);
		}

		static void WriteOut (HttpListenerContext context, ITransportHeaders responseHeaders, Stream responseStream)
		{
			//header processing taken/modified from HttpRemotingHandler
			if (responseHeaders != null && responseHeaders["__HttpStatusCode"] != null) {
				context.Response.StatusCode = Convert.ToInt32 (responseHeaders["__HttpStatusCode"]);
				context.Response.StatusDescription = (string) responseHeaders["__HttpReasonPhrase"];
			}

			if (responseHeaders != null) {
				foreach (DictionaryEntry entry in responseHeaders) {
					string key = entry.Key.ToString ();
					if (key != "__HttpStatusCode" && key != "__HttpReasonPhrase") {
						context.Response.AddHeader ((string)entry.Key, responseHeaders[entry.Key].ToString ());
					}
				}
			}
			
			//we need a stream with a length, so if it's not a MemoryStream we copy it
			MemoryStream ms;
			if (responseStream is MemoryStream) {
				ms = (MemoryStream) responseStream;
				//this seems to be necessary for some third-party formatters
				//even though my testing suggested .NET doesn't seem to seek incoming streams
				ms.Position = 0;
			} else {
				ms = new MemoryStream ();
				HttpClientTransportSink.CopyStream (responseStream, ms, 1024);
				ms.Position = 0;
				responseStream.Close ();
			}
			
			//FIXME: WHY DOES CHUNKING BREAK THE TESTS?
			//for now, we set the content length so that the server doesn't use chunking
			context.Response.ContentLength64 = ms.Length;
			HttpClientTransportSink.CopyStream (ms, context.Response.OutputStream, 1024);
			ms.Close ();
		}

		class ContextWithId
		{
			static object lockObj = new object ();
			static int nextId = 0;

			HttpListenerContext context;
			int id;

			public HttpListenerContext Context { get { return context; } }
			public int ID { get { return id; } } 
			
			public ContextWithId (HttpListenerContext context)
			{
				this.context = context;
				lock (lockObj) {
					//FIXME: is it really valid to roll arund and reset the ID?
					unchecked {
						this.id = nextId++;
					}
				}
			}

		}

	}
}
