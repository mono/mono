// 
// System.Web.Services.Protocols.HttpSimpleClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Net;
using System.IO;

namespace System.Web.Services.Protocols {
	public abstract class HttpSimpleClientProtocol : HttpWebClientProtocol {

		#region Fields

		internal HttpSimpleTypeStubInfo TypeStub;
		
		#endregion // Fields

		#region Constructors

		protected HttpSimpleClientProtocol () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		protected IAsyncResult BeginInvoke (string methodName, string requestUrl, object[] parameters, AsyncCallback callback, object asyncState)
		{
			HttpSimpleMethodStubInfo method = (HttpSimpleMethodStubInfo) TypeStub.GetMethod (methodName);
			SimpleWebClientAsyncResult ainfo = null;
			
			try
			{
				MimeParameterWriter parameterWriter = (MimeParameterWriter) method.ParameterWriterType.Create ();
				string url = parameterWriter.GetRequestUrl (requestUrl, parameters);
				WebRequest req = GetWebRequest (new Uri(url));
				
				ainfo = new SimpleWebClientAsyncResult (req, callback, asyncState);
				ainfo.Parameters = parameters;
				ainfo.ParameterWriter = parameterWriter;
				ainfo.Method = method;
				
				ainfo.Request = req;
				ainfo.Request.BeginGetRequestStream (new AsyncCallback (AsyncGetRequestStreamDone), ainfo);
			}
			catch (Exception ex)
			{
				if (ainfo != null)
					ainfo.SetCompleted (null, ex, false);
				else
					throw ex;
			}

			return ainfo;

		}

		void AsyncGetRequestStreamDone (IAsyncResult ar)
		{
			SimpleWebClientAsyncResult ainfo = (SimpleWebClientAsyncResult) ar.AsyncState;
			try
			{
				if (ainfo.ParameterWriter.UsesWriteRequest)
				{
					Stream stream = ainfo.Request.GetRequestStream ();
					ainfo.ParameterWriter.WriteRequest (stream, ainfo.Parameters);
					stream.Close ();
				}

				ainfo.Request.BeginGetResponse (new AsyncCallback (AsyncGetResponseDone), ainfo);
			}
			catch (Exception ex)
			{
				ainfo.SetCompleted (null, ex, true);
			}
		}

		void AsyncGetResponseDone (IAsyncResult ar)
		{
			SimpleWebClientAsyncResult ainfo = (SimpleWebClientAsyncResult) ar.AsyncState;
			WebResponse response = null;

			try {
				response = GetWebResponse (ainfo.Request, ar);
			}
			catch (WebException ex) {
				response = ex.Response;
				HttpWebResponse http_response = response as HttpWebResponse;
				if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError) {
					ainfo.SetCompleted (null, ex, true);
					return;
				}
			}
			catch (Exception ex) {
				ainfo.SetCompleted (null, ex, true);
				return;
			}

			try {
				MimeReturnReader returnReader = (MimeReturnReader) ainfo.Method.ReturnReaderType.Create ();
				object result = returnReader.Read (response, response.GetResponseStream ());
				ainfo.SetCompleted (result, null, true);
			}
			catch (Exception ex) {
				ainfo.SetCompleted (null, ex, true);
			}
		}


		protected object EndInvoke (IAsyncResult asyncResult)
		{
			if (!(asyncResult is SimpleWebClientAsyncResult)) throw new ArgumentException ("asyncResult is not the return value from BeginInvoke");

			SimpleWebClientAsyncResult ainfo = (SimpleWebClientAsyncResult) asyncResult;
			lock (ainfo)
			{
				if (!ainfo.IsCompleted) ainfo.WaitForComplete ();
				if (ainfo.Exception != null) throw ainfo.Exception;
				else return ainfo.Result;
			}
		}

		protected object Invoke (string methodName, string requestUrl, object[] parameters)
		{
			HttpSimpleMethodStubInfo method = (HttpSimpleMethodStubInfo) TypeStub.GetMethod (methodName);
			MimeParameterWriter parameterWriter = (MimeParameterWriter) method.ParameterWriterType.Create ();
			
			string url = parameterWriter.GetRequestUrl (requestUrl, parameters);
			WebRequest request = GetWebRequest (new Uri(url, true));
			
			parameterWriter.InitializeRequest (request, parameters);
			
			if (parameterWriter.UsesWriteRequest)
			{
				Stream stream = request.GetRequestStream ();
				parameterWriter.WriteRequest (stream, parameters);
				stream.Close ();
			}
			
			WebResponse response = GetWebResponse (request);
			
			MimeReturnReader returnReader = (MimeReturnReader) method.ReturnReaderType.Create ();
			return returnReader.Read (response, response.GetResponseStream ());
		}
		
		#endregion // Methods
	}
	
	internal class SimpleWebClientAsyncResult : WebClientAsyncResult
	{
		public SimpleWebClientAsyncResult (WebRequest request, AsyncCallback callback, object asyncState)
		: base (request, callback, asyncState)
		{
		}
		
		public object[] Parameters;
		public HttpSimpleMethodStubInfo Method;
		public MimeParameterWriter ParameterWriter;
	}
}
