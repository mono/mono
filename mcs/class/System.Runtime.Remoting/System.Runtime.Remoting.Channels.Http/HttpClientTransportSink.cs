//
// HttpClientTransportSink.cs
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

namespace System.Runtime.Remoting.Channels.Http
{
	class HttpClientTransportSink : IClientChannelSink
	{
		string url;
		HttpClientChannel channel;

		public HttpClientTransportSink (HttpClientChannel channel, string url)
		{
			this.channel = channel;
			this.url = url;
		}

		//always the last sink in the chain
		public IClientChannelSink NextChannelSink
		{
			get { return null; }
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack, IMessage msg,
			ITransportHeaders headers, Stream requestStream)
		{
			bool isOneWay = RemotingServices.IsOneWay (((IMethodMessage)msg).MethodBase);
			
			HttpWebRequest request = CreateRequest (headers);
	
			using (Stream targetStream = request.GetRequestStream ()) {
				CopyStream (requestStream, targetStream, 1024);
			}

			if (!isOneWay) {
				sinkStack.Push (this, request);
				request.BeginGetResponse (new AsyncCallback (AsyncProcessResponseCallback), sinkStack);
			}
		}
		
		void AsyncProcessResponseCallback (IAsyncResult ar)
		{
			IClientChannelSinkStack sinkStack = (IClientChannelSinkStack)ar.AsyncState;
			HttpWebRequest request = (HttpWebRequest)sinkStack.Pop(this);
			
			WebResponse response;
			try {
				response = request.EndGetResponse (ar);
			} catch (WebException ex) {
				response = ex.Response;
				//only error 500 is handled by the remoting stack
				HttpWebResponse httpResponse = response as HttpWebResponse;
				if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.InternalServerError) {
					sinkStack.DispatchException (ex);
					return;
				}
			}
			
			//this is only valid after the response is fetched
			SetConnectionLimit (request);

			using (response) {
				Stream responseStream = response.GetResponseStream ();
				ITransportHeaders responseHeaders = GetHeaders (response);
				sinkStack.AsyncProcessResponse (responseHeaders, responseStream);
			}
		}

		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack, object state,
			ITransportHeaders headers, Stream stream)
		{
			// Should never be called
			throw new NotSupportedException ();
		}

		public Stream GetRequestStream (IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		HttpWebRequest CreateRequest (ITransportHeaders requestHeaders)
		{
			string url = this.url;
			
			
			//FIXME: requestUri should contain the URL-less URI only when it's a CAO call; 
			// at all other times it should be null. On Mono, whenever it should be null, it contains the full
			// URL+URI, so we have a broken mixure of path types and we need to hack around it
			string requestUri = requestHeaders[CommonTransportKeys.RequestUri] as string;
			string objectURI;
			if (requestUri != null && HttpChannel.ParseInternal (requestUri, out objectURI) == null) {
				url = HttpChannel.ParseInternal (url, out objectURI);
				if (!url.EndsWith ("/"))
					url = url + "/";
				url = url + requestUri;
			}
			
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);
			request.UserAgent = string.Format ("Mozilla/4.0+(compatible; Mono Remoting; Mono {0})",
				System.Environment.Version);
			
			//Only set these if they deviate from the defaults, as some map to 
			//properties that throw NotImplementedExceptions
			if (channel.Timeout != -1)
				request.Timeout = channel.Timeout;
			if (channel.AllowAutoRedirect == false)
				request.AllowAutoRedirect = false;
			if (channel.Credentials != null)
				request.Credentials = channel.Credentials;
#if NET_2_0
			else if (channel.UseDefaultCredentials == true)
				request.UseDefaultCredentials = true;
#endif
			else if (channel.Username != null && channel.Username.Length > 0) {
				if (channel.Domain != null && channel.Domain.Length > 0) {
					request.Credentials = new NetworkCredential (channel.Username, channel.Password,
						channel.Domain);
				} else {
					request.Credentials = new NetworkCredential (channel.Username, channel.Password);
				}
			}
			
			if (channel.UnsafeAuthenticatedConnectionSharing == true)
				request.UnsafeAuthenticatedConnectionSharing = true;
			if (channel.ConnectionGroupName != null)
				request.ConnectionGroupName = channel.ConnectionGroupName;
			
			/*
			FIXME: implement these
			MachineName
			ProxyName
			ProxyPort
			ProxyUri
			ServicePrincipalName
			UseAuthenticatedConnectionSharing
			*/
			
			//build the headers
			request.ContentType = (string)requestHeaders["Content-Type"];
			
			//BUG: Mono formatters/dispatcher don't set this. Something in the MS stack does.
			string method = (string)requestHeaders["__RequestVerb"];
			if (method == null)
				method = "POST";
			request.Method = method;
			
			foreach (DictionaryEntry entry in requestHeaders) {
				string key = entry.Key.ToString ();
				if (key != "__RequestVerb" && key != "Content-Type" && key != CommonTransportKeys.RequestUri) {
					request.Headers.Add (key, entry.Value.ToString ());
				}
			}
			return request;
		}
		
		void SetConnectionLimit (HttpWebRequest request)
		{
			if (channel.ClientConnectionLimit != 2) {
				request.ServicePoint.ConnectionLimit = channel.ClientConnectionLimit;
			}
		}

		static TransportHeaders GetHeaders (WebResponse response)
		{
			TransportHeaders headers = new TransportHeaders ();
			foreach (string key in response.Headers) {
				headers[key] = response.Headers[key];
			}
			return headers;
		}

		internal static void CopyStream (Stream source, Stream target, int bufferSize)
		{
			byte[] buffer = new byte[bufferSize];
			int readLen = source.Read (buffer, 0, buffer.Length);
			while (readLen > 0) {
				target.Write (buffer, 0, readLen);
				readLen = source.Read (buffer, 0, buffer.Length);
			}
		}

		public void ProcessMessage (IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
			out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			HttpWebRequest request = CreateRequest (requestHeaders);
			Stream targetStream = request.GetRequestStream ();
			CopyStream (requestStream, targetStream, 1024);
			targetStream.Close ();
			
			WebResponse response;
			try {
				response = request.GetResponse ();
			} catch (WebException ex) {
				response = ex.Response;
				//only error 500 is handled by the remoting stack
				HttpWebResponse httpResponse = response as HttpWebResponse;
				if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.InternalServerError)
					throw;
			}
			
			//this is only valid after the response is fetched
			SetConnectionLimit (request);
			
			//FIXME: can we assume that the formatters will close the stream? Or do we need to make
			// a copy and close it ourselves?
			responseHeaders = GetHeaders (response);
			responseStream = response.GetResponseStream ();
		}

		public IDictionary Properties
		{
			get { return null; }
		}
	}
}
