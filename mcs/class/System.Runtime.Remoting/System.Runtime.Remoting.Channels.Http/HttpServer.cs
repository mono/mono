//==========================================================================
//  File:       HttpServer.cs
//
//  Summary:       Implements an HttpServer to be used by the HttpServerChannel class
//				 
//
//  Classes:    internal sealed HttpServer
//		internal  sealed   ReqMessageParser
//		private RequestArguments
//
//	By :
//		Ahmad    Tantawy	popsito82@hotmail.com
//		Ahmad	 Kadry		kadrianoz@hotmail.com
//		Hussein  Mehanna	hussein_mehanna@hotmail.com
//
//==========================================================================

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
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

using System.Runtime.Remoting.Channels;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http
{

	internal class RequestArguments
	{
		static int count;
		Socket socket;
		
		public RequestArguments (Socket socket, HttpServerTransportSink sink)
		{
			Id = count++;
			NetworkStream ns = new NetworkStream (socket);
			this.Stream = ns;
			this.socket = socket;
			Sink = sink;
		}
		
		public void Process ()
		{
			HttpServer.ProcessRequest (this);
		}
		
		public IPAddress ClientAddress
		{
			get {
				IPEndPoint ep = socket.RemoteEndPoint as IPEndPoint;
				if (ep != null) return ep.Address;
				else return null;
			}
		}
		
		public void Close()
		{
			Stream.Close();
			socket.Close();
		}
		
		public int Id;
		public Stream Stream;
		public HttpServerTransportSink Sink;
	}

	internal sealed class HttpServer
	{
		static HttpServer ()
		{
			Array.Sort (knownHeaders);
		}
		
		public static void ProcessRequest (RequestArguments reqArg)
		{
			try {
				//Step (1) Start Reciceve the header
				ArrayList  Headers = RecieveHeader (reqArg);
					
				//Step (2) Start Parse the header
				IDictionary HeaderFields = new Hashtable();
				IDictionary CustomHeaders = new Hashtable();
				if (!ParseHeader (reqArg, Headers, HeaderFields, CustomHeaders))
					return;

				//Step (3)
				if (!CheckRequest (reqArg, HeaderFields, CustomHeaders))
					return;

				//Step (4) Recieve the entity body
				
				byte[] buffer;
				object len = HeaderFields["content-length"];
				if (len != null)
				{
					buffer = new byte [(int)len];
					if (!RecieveEntityBody (reqArg, buffer))
						return;
				}
				else
					buffer = new byte [0];
					
				//Step (5)
				SendRequestForChannel (reqArg, HeaderFields, CustomHeaders, buffer);
			} finally {
				reqArg.Close ();
			}
		}

		private static ArrayList RecieveHeader (RequestArguments reqArg)
		{
			bool bLastLine = false;
			bool bEndOfLine = false;
	
			byte[] buffer = new byte[1024];
			ArrayList  Headers = new ArrayList();
			
			Stream ist = reqArg.Stream;

			int index =0;
			while (!bLastLine)
			{ 
				//recieve line by line 
				index = 0;
				bEndOfLine = false;

				//Step (1) is it an empty line?
				ist.Read (buffer, index, 1);
				
				if(buffer[index++]==13)
				{
					ist.Read (buffer, index, 1);
					bLastLine=true;
					bEndOfLine = true;
				}
				
				//Step (2) recieve line bytes
				while (!bEndOfLine)
				{
					ist.Read (buffer, index, 1);

					if(buffer [index++]==13)
					{
						bEndOfLine = true;
						ist.Read (buffer,index,1);
					}
				}

				//Step (3) convert bytes to a string
				if (bLastLine)
					continue;
					
                Headers.Add (Encoding.ASCII.GetString (buffer,0,index));

			}//end while loop
			
			return Headers;
		}
		
		private static bool ParseHeader (RequestArguments reqArg, ArrayList Headers, IDictionary HeaderFields, IDictionary CustomHeaders)
		{
			// The first "header" is the method

			string[] met = ((string) Headers [0]).Split (' ');
			HeaderFields.Add ("method", met[0]);

			if (met.Length >= 1)
				HeaderFields.Add ("request-url", met [1]);
			
			if (met.Length >= 2)
				HeaderFields.Add ("http-version", met [2]);
			
			for (int i = 1; i < Headers.Count; i++)
			{
				string header = (string) Headers [i];
			 	int p = header.IndexOf (':');
			 	
			 	string id;
			 	object val;
			 	if (p == -1) {
			 		id = header.Trim ().ToLower ();
			 		val = "";
			 	} else {
			 		id = header.Substring (0, p).Trim ().ToLower ();
			 		val = header.Substring (p + 1).Trim ();
			 	}
			 	
			 	if (Array.BinarySearch (knownHeaders, id) >= 0) {
			 		if (id == "content-length")
			 			val = int.Parse ((string)val);
			 		HeaderFields [id] = val;
			 	}
			 	else {
			 		CustomHeaders [id] = val;
			 	}
			}

			return true;
		}

		private static bool CheckRequest (RequestArguments reqArg, IDictionary HeaderFields, IDictionary CustomHeaders)
		{
			string temp;
			
			if (HeaderFields["expect"] as string == "100-continue")
				SendResponse (reqArg, 100, null, null);

			//Check the method
			temp = HeaderFields["method"].ToString();
			if (temp != "POST")
                return true;

			//Check for the content-length field
			if (HeaderFields["content-length"]==null)
			{
				SendResponse (reqArg, 411, null, null);
				return false;
			}
			return true;
		}

		
		private static bool RecieveEntityBody (RequestArguments reqArg, byte[] buffer)
		{
			try
			{
				int nr = 0;
				while (nr < buffer.Length)
					nr += reqArg.Stream.Read (buffer, nr, buffer.Length - nr);
			}
			catch (SocketException e)
			{
				switch(e.ErrorCode)
				{
					case 10060 : //TimeOut
						SendResponse (reqArg, 408, null, null);
						break;
					default :
						throw e;
				}
				
				return false;
			}//end catch

			return true;
		}
	
		private static bool SendRequestForChannel (RequestArguments reqArg, IDictionary HeaderFields, IDictionary CustomHeaders, byte[] buffer)
		{
			TransportHeaders THeaders = new TransportHeaders();

			Stream stream = new MemoryStream(buffer);

			if(stream.Position !=0)
				stream.Seek(0,SeekOrigin.Begin);

			THeaders[CommonTransportKeys.RequestUri] = FixURI((string)HeaderFields["request-url"]);
			THeaders[CommonTransportKeys.ContentType]= HeaderFields["content-type"];
			THeaders[CommonTransportKeys.RequestVerb]= HeaderFields["method"];
			THeaders[CommonTransportKeys.HttpVersion] = HeaderFields["http-version"];
			THeaders[CommonTransportKeys.UserAgent] = HeaderFields["user-agent"];
			THeaders[CommonTransportKeys.Host] = HeaderFields["host"];
			THeaders[CommonTransportKeys.SoapAction] = HeaderFields["soapaction"];
			THeaders[CommonTransportKeys.IPAddress] = reqArg.ClientAddress;
			THeaders[CommonTransportKeys.ConnectionId] = reqArg.Id;

			foreach(DictionaryEntry DictEntry in CustomHeaders)
			{
				THeaders[DictEntry.Key.ToString()] = DictEntry.Value.ToString();
			}

			reqArg.Sink.ServiceRequest (reqArg, stream, THeaders);
			return true;
		}

		private static string FixURI(string RequestURI)
		{

			if(RequestURI.IndexOf ( '.' ) == -1)
				return RequestURI;
			else
				return RequestURI.Substring(1);
			
		}
		
		public static void SendResponse (RequestArguments reqArg, int httpStatusCode, ITransportHeaders headers, Stream responseStream)
		{
			byte [] headersBuffer = null;
			byte [] entityBuffer = null;

			StringBuilder responseStr;
			String reason = null;

			if (headers != null && headers[CommonTransportKeys.HttpStatusCode] != null) {
				// The formatter can override the result code
				httpStatusCode = int.Parse ((string)headers [CommonTransportKeys.HttpStatusCode]);
				reason = (string) headers [CommonTransportKeys.HttpReasonPhrase];
			}

			if (reason == null)
				reason = GetReasonPhrase (httpStatusCode);
			
			//Response Line 
			responseStr = new StringBuilder ("HTTP/1.0 " + httpStatusCode + " " + reason + "\r\n" );
			
			if (headers != null)
			{
				foreach (DictionaryEntry entry in headers)
				{
					string key = entry.Key.ToString();
					if (key != CommonTransportKeys.HttpStatusCode && key != CommonTransportKeys.HttpReasonPhrase)
						responseStr.Append(key + ": " + entry.Value.ToString() + "\r\n");
				}
			}
			
			responseStr.Append("Server: Mono Remoting, Mono CLR " + System.Environment.Version.ToString() + "\r\n");

			if(responseStream != null && responseStream.Length!=0)
			{
				responseStr.Append("Content-Length: "+responseStream.Length.ToString()+"\r\n"); 
				entityBuffer  = new byte[responseStream.Length];
				responseStream.Seek(0 , SeekOrigin.Begin);
				responseStream.Read(entityBuffer,0,entityBuffer.Length);
			}
			else
				responseStr.Append("Content-Length: 0\r\n"); 

			responseStr.Append("X-Powered-By: Mono\r\n"); 
			responseStr.Append("Connection: close\r\n"); 
   			responseStr.Append("\r\n");
		
		   	headersBuffer = Encoding.ASCII.GetBytes (responseStr.ToString());

			//send headersBuffer
			reqArg.Stream.Write (headersBuffer, 0, headersBuffer.Length);

			if (entityBuffer != null)
				reqArg.Stream.Write (entityBuffer, 0, entityBuffer.Length);
		}

		internal static string GetReasonPhrase (int HttpStatusCode)
		{
			switch (HttpStatusCode)
			{
				case 100 : return "Continue" ;
				case 101  :return "Switching Protocols";
				case 200  :return "OK";
				case 201  :return "Created";
				case 202  :return "Accepted";
				case 203   :return "Non-Authoritative Information";
				case 204  :return "No Content";
				case 205   :return "Reset Content";
				case 206   :return "Partial Content";
				case 300   :return "Multiple Choices";
				case 301   :return "Moved Permanently";
				case 302   :return  "Found";
				case 303   :return  "See Other";
				case 304   :return  "Not Modified";
				case 305  :return   "Use Proxy";
				case 307   :return  "Temporary Redirect";
				
				case 400   :return  "Bad Request";
				case 401   :return  "Unauthorized";
				case 402   :return  "Payment Required";
				case 403   :return  "Forbidden";
				case 404   :return  "Not Found";
				case 405   :return  "Method Not Allowed";
				case 406   :return  "Not Acceptable";
											 
				case 407   :return  "Proxy Authentication Required";
				case 408   :return  "Request Time-out";
				case 409   :return  "Conflict";
				case 410   :return  "Gone";
				case 411   :return  "Length Required";
				case 412   :return  "Precondition Failed";
				case 413   :return  "Request Entity Too Large";
				case 414   :return  "Request-URI Too Large";
				case 415   :return  "Unsupported Media Type";
				case 416   :return  "Requested range not satisfiable";
				case 417   :return  "Expectation Failed";
				
				case 500  :return   "Internal Server Error";
				case 501  :return   "Not Implemented";
				case 502   :return  "Bad Gateway";
				case 503  :return   "Service Unavailable";
				case 504   :return  "Gateway Time-out";
				case 505   :return  "HTTP Version not supported";
				default: return "";

			}
		}
		
		static string[] knownHeaders = new string [] {
			"accept",
			"accept-charset",
			"accept-encoding",
			"authorization",
			"accept-language",
			"from",
			"host",
			"if-modified-since",
			"proxy-authorization",
			"range",
			"user-agent",
			"expect",
			"connection",
			"allow",
		    "content-encoding",
			"content-language",
			"content-length",
			"content-range",
			"content-type",
			"content-version",
			"derived-from",
			"expires",
			"last-modified",
			"link",
			"title",
		    "transfere-encoding",
		    "url-header",
			"extension-header"
		 };
	}
}


