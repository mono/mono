//
// System.Runtime.Remoting.Channels.Http.HttpRemotingHandler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.IO;
using System.Web;

namespace System.Runtime.Remoting.Channels.Http 
{
	public class HttpRemotingHandler : IHttpHandler 
	{
		HttpServerTransportSink transportSink;
		
		public HttpRemotingHandler ()
		{
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
			
			theaders ["__RequestUri"] = objectUri;
			theaders ["Content-Type"] = request.ContentType;
			theaders ["__RequestVerb"]= request.HttpMethod;
			theaders ["__HttpVersion"] = request.Headers ["http-version"];
			theaders ["User-Agent"] = request.UserAgent;
			theaders ["Host"] = request.Headers ["host"];

			ITransportHeaders responseHeaders;
			Stream responseStream;
			
			// Dispatch the request
			
			transportSink.DispatchRequest (request.InputStream, theaders, out responseStream, out responseHeaders);

			// Write the response
			
			if (responseHeaders != null && responseHeaders["__HttpStatusCode"] != null) 
			{
				// The formatter can set the status code
				response.StatusCode = int.Parse ((string) responseHeaders["__HttpStatusCode"]);
				response.StatusDescription = (string) responseHeaders["__HttpReasonPhrase"];
			}
			
			byte[] bodyBuffer = bodyBuffer = new byte [responseStream.Length];
			responseStream.Seek (0, SeekOrigin.Begin);
			
			int nr = 0;
			while (nr < responseStream.Length)
				nr += responseStream.Read (bodyBuffer, nr, bodyBuffer.Length - nr);
			
			response.OutputStream.Write (bodyBuffer, 0, bodyBuffer.Length);
		}
	}	
}
