//
// System.Runtime.Remoting.Channels.Http.HttpRemotingHandler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
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
using System.IO;
using System.Web;
using System.Collections;

namespace System.Runtime.Remoting.Channels.Http 
{
	public class HttpRemotingHandler : IHttpHandler 
	{
		HttpServerTransportSink transportSink;
		
		public HttpRemotingHandler ()
		{
		}

		[MonoTODO]
		public HttpRemotingHandler (Type type, object srvID)
		{
			throw new NotImplementedException ();
		}

		internal HttpRemotingHandler (HttpServerTransportSink sink)
		{
			transportSink = sink;
		}

		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest (HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			
			// Create transport headers for the request
			TransportHeaders theaders = new TransportHeaders();

			string objectUri = request.RawUrl;
			objectUri = objectUri.Substring (request.ApplicationPath.Length);	// application path is not part of the uri
			if (request.ApplicationPath.Length > 0 && 
			   (objectUri.StartsWith("/") || objectUri.StartsWith(@"\")) )
			{
				objectUri = objectUri.Substring(1);
			}

			theaders ["__RequestUri"] = objectUri;
			theaders ["Content-Type"] = request.ContentType;
			theaders ["__RequestVerb"]= request.HttpMethod;
			theaders ["__HttpVersion"] = request.Headers ["http-version"];
			theaders ["User-Agent"] = request.UserAgent;
			theaders ["Host"] = request.Headers ["host"];

			ITransportHeaders responseHeaders;
			Stream responseStream;
			
			// Dispatch the request	
			ServerProcessing proc = transportSink.SynchronousDispatch 
				(theaders, request.InputStream, out responseHeaders, out responseStream);
			
			if (proc == ServerProcessing.Async) {
				throw new NotSupportedException ("HttpRemotingHandler does not support async processing in " +
					"the synchronous HTTP pipeline" );
			}

			// Write the response
			if (responseHeaders != null && responseHeaders["__HttpStatusCode"] != null) 
			{
				// The formatter can set the status code
				response.StatusCode = int.Parse ((string) responseHeaders["__HttpStatusCode"]);
				response.StatusDescription = (string) responseHeaders["__HttpReasonPhrase"];
			}
			
			if (responseHeaders != null)
			{
				foreach (DictionaryEntry entry in responseHeaders)
				{
					string key = entry.Key.ToString();
					if (key != CommonTransportKeys.HttpStatusCode && key != CommonTransportKeys.HttpReasonPhrase)
						response.AppendHeader(key, entry.Value.ToString());
				}
			}
			
			if (responseStream != null) {
				HttpClientTransportSink.CopyStream (responseStream, response.OutputStream, 1024);
			}
		}
	}	
}
