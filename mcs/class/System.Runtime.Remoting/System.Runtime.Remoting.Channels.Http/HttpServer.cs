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
		public RequestArguments (Socket socket, HttpServerTransportSink sink)
		{
			NetworkStream ns = new NetworkStream (socket);
			InputStream = ns;
			OutputStream = ns;
			Sink = sink;
		}
		
		public Stream InputStream;
		public Stream OutputStream;
		public HttpServerTransportSink Sink;
	}

	internal sealed class HttpServer
	{
		public static void ProcessRequest (object reqInfo)
		{
			if(reqInfo as RequestArguments == null)
				return;

			RequestArguments reqArg = (RequestArguments)reqInfo;
		
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
		}

		private static ArrayList RecieveHeader (RequestArguments reqArg)
		{
			bool bLastLine = false;
			bool bEndOfLine = false;
	
			byte[] buffer = new byte[1024];
			ArrayList  Headers = new ArrayList();
			
			Stream ist = reqArg.InputStream;

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
			for (int i=0;i<Headers.Count;i++)
			{
				if (ReqMessageParser.ParseHeaderField ((string)Headers[i],HeaderFields))
					continue;
					
				if (!ReqMessageParser.IsCustomHeader((string)Headers[i],CustomHeaders ) )
				{
					SendResponse (reqArg, 400, null, null);
					return false;
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
					nr += reqArg.InputStream.Read (buffer, nr, buffer.Length - nr);
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
			THeaders[CommonTransportKeys.SoapAction] = HeaderFields["SOAPAction"];

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
			reqArg.OutputStream.Write (headersBuffer, 0, headersBuffer.Length);

			if (entityBuffer != null)
				reqArg.OutputStream.Write (entityBuffer, 0, entityBuffer.Length);
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
		
	}
	
	
	internal sealed class ReqMessageParser
	{
		private  const int nCountReq = 14;
		private  const int nCountEntity = 15;
		
		private static bool bInitialized = false;
		
		private static String [] ReqRegExpString = new String [nCountReq ];
		private static String [] EntityRegExpString = new String[nCountEntity]; 
		
		private static Regex [] ReqRegExp = new Regex[nCountReq];
		private static Regex [] EntityRegExp = new Regex[nCountEntity];
		 

		 
		public ReqMessageParser ()
		{
		}

		 public static bool ParseHeaderField(string buffer,IDictionary headers)
		 {
			 try
			 {
				 if(!bInitialized)
				 {
					 Initialize();
					 bInitialized =true;
				 }

				 if(IsRequestField(buffer,headers))
					 return true;
				 if(IsEntityField(buffer,headers))
					 return true ;
			 
			 }
			 catch(Exception )
			 {
				 //<Exception>
			 }

			 //Exception

			 return false;
		 }
		 
		 private static bool Initialize()
		 {
			 if(bInitialized)
				 return true;

			 bInitialized = true;

			 //initialize array
			 //Create all the Regular expressions
			 InitializeRequestRegExp();
			 InitiazeEntityRegExp();

			 for(int i=0;i<nCountReq;i++)
				 ReqRegExp[i] = new Regex(ReqRegExpString[i],RegexOptions.Compiled|RegexOptions.IgnoreCase);
			
			 for(int i=0;i<nCountEntity;i++)
				 EntityRegExp[i] = new Regex(EntityRegExpString[i],RegexOptions.Compiled|RegexOptions.IgnoreCase);

			 return true;

		 }

		 private static void InitializeRequestRegExp()
		 {
			 //Request Header Fields
			 //
			 ReqRegExpString[0] = "^accept(\\s*:\\s*)(?<accept>\\S+)(\\s*|)(\\s*)$";
			 ReqRegExpString[1] = "^accept-charset(\\s*:\\s*)(?<accept_charset>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[2] = "^accept-encoding(\\s*:\\s*)(?<accept_Encoding>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[3] = "^authorization(\\s*:\\s*)(?<authorization>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[4] = "^accept-language(\\s*:\\s*)(?<accept_Language>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[5] = "^from(\\s*:\\s*)(?<from>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[6] = "^host(\\s*:\\s*)(?<host>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[7] = "^if-modified-since(\\s*:\\s*)(?<if_modified>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[8] = "^proxy-authorization(\\s*:\\s*)(?<proxy_auth>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[9] = "^range(\\s*:\\s*)(?<range>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[10] = "^user-agent(\\s*:\\s*)(?<user_agent>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[11] = "^expect(\\s*:\\s*)(?<expect>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[12] = "^connection(\\s*:\\s*)(?<connection>\\S+(\\s|\\S)*\\S)(\\s*)$";
			 ReqRegExpString[13] = "^(?<method>\\w+)(\\s+)(?<request_url>\\S+)(\\s+)(?<http_version>\\S+)(\\s*)$";
			// ReqRegExpString[14] = "";			 
		 }

		 private static void InitiazeEntityRegExp()
		 {
			EntityRegExpString[0] = "^allow(\\s*:\\s*)(?<allow>[0-9]+)(\\s*)$";
		    EntityRegExpString[1] = "^content-encoding(\\s*:\\s*)(?<content_encoding>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[2] = "^content-language(\\s*:\\s*)(?<content_language>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[3] = "^content-length(\\s*:\\s*)(?<content_length>[0-9]+)(\\s*)$";
			EntityRegExpString[4] = "^content-range(\\s*:\\s*)(?<content_range>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[5] = "^content-type(\\s*:\\s*)(?<content_type>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[6] = "^content-version(\\s*:\\s*)(?<content_version>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[7] = "^derived-from(\\s*:\\s*)(?<derived_from>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[8] = "^expires(\\s*:\\s*)(?<expires>\\S+(\\s|\\S)*\\S)(\\s*)$";//date
			EntityRegExpString[9] = "^last-modified(\\s*:\\s*)(?<last_modified>\\S+(\\s|\\S)*\\S)(\\s*)$";//date
			EntityRegExpString[10] = "^link(\\s*:\\s*)(?<link>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[11] = "^title(\\s*:\\s*)(?<title>\\S+(\\s|\\S)*\\S)(\\s*)$";
		    EntityRegExpString[12] = "^transfere-encoding(\\s*:\\s*)(?<transfere_encoding>\\S+(\\s|\\S)*\\S)(\\s*)$";
		    EntityRegExpString[13] = "^url-header(\\s*:\\s*)(?<url_header>\\S+(\\s|\\S)*\\S)(\\s*)$";
			EntityRegExpString[14] = "^extension-header(\\s*:\\s*)(?<extension_header>\\S+(\\s|\\S)*\\S)(\\s*)$";
		 }
				
		 private static void CopyGroupNames(Regex regEx , Match m , IDictionary headers)
		 {
			 
			 if(!m.Success)
				 return;

			 string [] ar = regEx.GetGroupNames();
			 GroupCollection gc = m.Groups;

			 for(int i=0;i<ar.Length;i++)
			 {
				 if(! char.IsLetter(ar[i],0))
					 continue;
                 
				headers.Add(ar[i],gc[ar[i]].Value);
			 }
		 }
		 

		 private static bool IsRequestField(string buffer , IDictionary HeaderItems)
		 {
			 
			 if(Request_accept(buffer , HeaderItems))
				 return true;

			 if(Request_accept_charset(buffer , HeaderItems))
				 return true;

			 if(Request_accept_encoding(buffer , HeaderItems))
				 return true;

			 if(Request_accept_language(buffer , HeaderItems))
				 return true;

			 if(Request_authorization(buffer , HeaderItems))
				 return true;

			 if(Request_connection(buffer , HeaderItems))
				 return true;

			 if(Request_expect(buffer , HeaderItems))
				 return true;

			 if(Request_from(buffer , HeaderItems))
				 return true;

			 if(Request_host(buffer , HeaderItems))
				 return true;

			 if(Request_modified(buffer , HeaderItems))
				 return true;

			 if(Request_proxy_authorization(buffer , HeaderItems))
				 return true;

			 if(Request_user_agent(buffer , HeaderItems))
				 return true;

			 if(Request_request_line(buffer , HeaderItems))
				 return true;

			 return false;
		 }

		 private static bool IsEntityField(string buffer , IDictionary HeaderItems)
		 {
			 if(Entity_allow(buffer , HeaderItems))
				 return true;

			 if(Entity_content_encoding(buffer , HeaderItems))
				 return true;

			 if(Entity_content_language(buffer , HeaderItems))
				 return true;

			 if(Entity_content_length(buffer , HeaderItems))
				 return true;

			 if(Entity_content_range(buffer , HeaderItems))
				 return true;

			 if(Entity_content_type(buffer , HeaderItems))
				 return true;

			 if(Entity_content_version(buffer , HeaderItems))
				 return true;

			 if(Entity_dervied_from(buffer , HeaderItems))
				 return true;

			 if(Entity_expires(buffer , HeaderItems))
				 return true;

			 if(Entity_extension_header(buffer , HeaderItems))
				 return true;

			 if(Entity_last_modified(buffer , HeaderItems))
				 return true;

			 if(Entity_link(buffer , HeaderItems))
				 return true;

			 if(Entity_title(buffer , HeaderItems))
				 return true;

			 if(Entity_transfere_encoding(buffer , HeaderItems))
				 return true;
			 
			 if(Entity_url_header(buffer , HeaderItems))           
				 return true;

			 return false;

		 }
		
		 public static bool IsCustomHeader(string buffer,IDictionary CustomHeader)
		 {
			 Regex CustomHeaderEx = new Regex("^(?<header>\\S+)(\\s*:\\s*)(?<field>\\S+(\\s|\\S)*\\S)(\\s*)",RegexOptions.Compiled);
			
			 Match m = CustomHeaderEx.Match(buffer);
			 if(!m.Success)
				 return false;

			 CustomHeader.Add(m.Groups["header"].Value,m.Groups["field"].Value);
			 return true;

		 }
		
		 //********************************************************
		 //REQUEST
		 private static bool Request_accept(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[0].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("accept",m.Groups["accept"].Value);
			 return true;
		 }

		 private static bool Request_accept_charset(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[1].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("accept-charset",m.Groups["accept_charset"].Value);
			 return true;

		 }
		 private static bool Request_accept_encoding(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[2].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("accept-encoding",m.Groups["accept_encoding"].Value);
			 return true;
		 }
		 private static bool Request_authorization(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[3].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("authorization",m.Groups["authorization"].Value);
			 return true;
		 }
		 private static bool Request_accept_language(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[4].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("accept-language",m.Groups["accept_language"].Value);
			 return true;
		 }
		 private static bool Request_from(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[5].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("from",m.Groups["from"].Value);
			 return true;
		 }
		 private static bool Request_host(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[6].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("host",m.Groups["host"].Value);
			 return true;
		 }
		 private static bool Request_modified(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[7].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("modified",m.Groups["modified"].Value);
			 return true;
		 }
		 private static bool Request_proxy_authorization(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[8].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("proxy-authorization",m.Groups["proxy_authorization"].Value);
			 return true;
		 }
		 private static bool Request_range(string buffer , IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[9].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("range",m.Groups["range"].Value);
			 return true;
			 
		 }
		 private static bool Request_user_agent(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[10].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("user-agent",m.Groups["user_agent"].Value);
			 return true;
		 }
		 private static bool Request_expect(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[11].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("expect",m.Groups["expect"].Value);
			 return true;
		 }

		 private static bool Request_connection(string buffer,IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[12].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("connection",m.Groups["connection"].Value);
			 return true;
		 }

		 private static bool Request_request_line(string buffer, IDictionary HeaderItems)
		 {
			 Match m = ReqRegExp[13].Match(buffer);
			 if(!m.Success)
				 return false;
			//ReqRegExpString[13] = "(?<method>\\w+)(\\s+)(?<request_url>\\S+)(\\s+)(?<http_version>\\S+)";
			 
			 HeaderItems.Add("method",m.Groups["method"].Value);
			 HeaderItems.Add("request-url",m.Groups["request_url"].Value);
			 HeaderItems.Add("http-version",m.Groups["http_version"].Value);
			 return true;
		 }
		//********************************************************

		
		 //********************************************************
		 //ENTITY
		 private static bool Entity_allow(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[0].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("allow",m.Groups["allow"].Value);
			 return true;
		 }

		 
		 private static bool Entity_content_encoding(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[1].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("content-encoding",m.Groups["content_encoding"].Value);
			 return true;
		 }
		 private static bool Entity_content_language(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[2].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("content-language",m.Groups["content_language"].Value);
			 return true;
		 }
		 private static bool Entity_content_length(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[3].Match(buffer);
			 if(!m.Success)
				 return false;
			
			int length;
			 try
			 {
				 length = Int32.Parse(m.Groups["content_length"].ToString());
			 }
			 catch (Exception )
			 {
				 //<Exception>
				 return false;
			 }

			 HeaderItems.Add("content-length",length);
			 return true;
		 }
		 private static bool Entity_content_range(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[4].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("content-range",m.Groups["content_range"].Value);
			 return true;
		 }
		 private static bool Entity_content_type(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[5].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("content-type",m.Groups["content_type"].Value);
			 return true;
		 }

		 private static bool Entity_content_version(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[6].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("content-version",m.Groups["content_version"].Value);
			 return true;
		 }
		 private static bool Entity_dervied_from(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[7].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("dervied-from",m.Groups["dervied_from"].Value);
			 return true;
		 }
		 private static bool Entity_expires(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[8].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("expires",m.Groups["expires"].Value);
			 return true;
		 }
		 private static bool Entity_last_modified(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[9].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("last-modified",m.Groups["last_modified"].Value);
			 return true;
		 }
		 private static bool Entity_link(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[10].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("link",m.Groups["link"].Value);
			 return true;
		 }
		 private static bool Entity_title(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[11].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("title",m.Groups["title"].Value);
			 return true;
		 }

		 private static bool Entity_transfere_encoding(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[12].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("transfere-encoding",m.Groups["transfere_encoding"].Value);
			 return true;
		 }
		 private static bool Entity_url_header(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[13].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("url-header",m.Groups["url_header"].Value);
			 return true;
		 }

		 private static bool Entity_extension_header(string buffer,IDictionary HeaderItems)
		 {
			 Match m = EntityRegExp[14].Match(buffer);
			 if(!m.Success)
				 return false;
			
			 HeaderItems.Add("extension-header",m.Groups["extension_header"].Value);
			 return true;
		 }

		 //********************************************************		 
	}

}


