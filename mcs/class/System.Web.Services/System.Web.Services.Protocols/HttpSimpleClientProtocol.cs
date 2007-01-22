// 
// System.Web.Services.Protocols.HttpSimpleClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Web.Services;
using System.Net;
using System.IO;
using System.Threading;

namespace System.Web.Services.Protocols {
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
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
				RegisterMapping (asyncState, ainfo);
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
			if (!(asyncResult is SimpleWebClientAsyncResult))
				throw new ArgumentException ("asyncResult is not the return value from BeginInvoke");

			SimpleWebClientAsyncResult ainfo = (SimpleWebClientAsyncResult) asyncResult;
			lock (ainfo)
			{
				if (!ainfo.IsCompleted)
					ainfo.WaitForComplete ();

				UnregisterMapping (ainfo.AsyncState);
				
				if (ainfo.Exception != null)
					throw ainfo.Exception;
				else
					return ainfo.Result;
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
		
#if NET_2_0

		protected void InvokeAsync (string methodName, string requestUrl, object[] parameters, SendOrPostCallback callback)
		{
			InvokeAsync (methodName, requestUrl, parameters, callback, null);
		}

		protected void InvokeAsync (string methodName, string requestUrl, object[] parameters, SendOrPostCallback callback, object userState)
		{
			InvokeAsyncInfo info = new InvokeAsyncInfo (callback, userState);
			BeginInvoke (methodName, requestUrl, parameters, new AsyncCallback (InvokeAsyncCallback), info);
		}

		void InvokeAsyncCallback (IAsyncResult ar)
		{
			InvokeAsyncInfo info = (InvokeAsyncInfo) ar.AsyncState;
			SimpleWebClientAsyncResult sar = (SimpleWebClientAsyncResult) ar;
			InvokeCompletedEventArgs args = new InvokeCompletedEventArgs (sar.Exception, false, info.UserState, (object[]) sar.Result);
			if (info.Context != null)
				info.Context.Send (info.Callback, args);
			else
				info.Callback (args);
		}
#endif
		
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
