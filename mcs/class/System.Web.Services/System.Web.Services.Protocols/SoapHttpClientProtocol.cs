// 
// System.Web.Services.Protocols.SoapHttpClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Ximian, Inc, 2003
//

using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Web.Services;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.Services.Description;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections;
using System.Threading;

namespace System.Web.Services.Protocols {
	public class SoapHttpClientProtocol : HttpWebClientProtocol {
		TypeStubInfo type_info;

		#region AsyncInfo class

		internal class AsyncInfo: IAsyncResult
		{
			bool _completedSynchronously;
			bool _done;
			ManualResetEvent _waitHandle;

			public object AsyncState 
			{
				get { return InternalAsyncState; }
			}

			public WaitHandle AsyncWaitHandle 
			{
				get
				{
					lock (this) {
						if (_waitHandle != null) return _waitHandle;
						_waitHandle = new ManualResetEvent (_done);
						return _waitHandle;
					}
				}
			}

			public bool CompletedSynchronously 
			{
				get { return _completedSynchronously; }
			}

			public bool IsCompleted 
			{
				get { lock (this) { return _done; } }
			}

			internal void SetCompleted (object[] result, Exception exception, bool async)
			{
				lock (this)
				{
					Exception = exception;
					Result = result;
					_done = true;
					_completedSynchronously = async;
					if (_waitHandle != null) _waitHandle.Set ();
					Monitor.PulseAll (this);
				}
				if (Callback != null) Callback (this);
			}

			internal void WaitForComplete ()
			{
				lock (this)
				{
					Monitor.Wait (this);
				}
			}

			internal object InternalAsyncState;
			internal AsyncCallback Callback;
			internal SoapClientMessage Message;
			internal SoapExtension[] Extensions;
			internal WebRequest Request;
			internal object[] Result;
			internal Exception Exception;
		}
		#endregion

		#region Constructors

		public SoapHttpClientProtocol () 
		{
			type_info = TypeStubManager.GetTypeStub (this.GetType ());
		}

		#endregion // Constructors

		#region Methods

		protected IAsyncResult BeginInvoke (string methodName, object[] parameters, AsyncCallback callback, object asyncState)
		{
			AsyncInfo ainfo = new AsyncInfo ();

			MethodStubInfo msi = type_info.GetMethod (methodName);
			ainfo.Message = new SoapClientMessage (this, msi, Url, parameters);
			ainfo.Message.CollectHeaders (this, ainfo.Message.MethodStubInfo.Headers, SoapHeaderDirection.In);
			ainfo.Extensions = SoapExtension.CreateExtensionChain (type_info.SoapExtensions[0], msi.SoapExtensions, type_info.SoapExtensions[1]);
			ainfo.InternalAsyncState = asyncState;
			ainfo.Callback = callback;

			try
			{
				ainfo.Request = GetRequestForMessage (uri, ainfo.Message);
				ainfo.Request.BeginGetRequestStream (new AsyncCallback (AsyncGetRequestStreamDone), ainfo);
			}
			catch (Exception ex)
			{
				ainfo.SetCompleted (null, ex, false);
			}

			return ainfo;
		}

		void AsyncGetRequestStreamDone (IAsyncResult ar)
		{
			AsyncInfo ainfo = (AsyncInfo) ar.AsyncState;
			try
			{
				SendRequest (ainfo.Request.EndGetRequestStream (ar), ainfo.Message, ainfo.Extensions);

				ainfo.Request.BeginGetResponse (new AsyncCallback (AsyncGetResponseDone), ainfo);
			}
			catch (Exception ex)
			{
				ainfo.SetCompleted (null, ex, true);
			}
		}

		void AsyncGetResponseDone (IAsyncResult ar)
		{
			AsyncInfo ainfo = (AsyncInfo) ar.AsyncState;
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
				object[] result = ReceiveResponse (response, ainfo.Message, ainfo.Extensions);
				ainfo.SetCompleted (result, null, true);
			}
			catch (Exception ex) {
				ainfo.SetCompleted (null, ex, true);
			}
		}

		protected object[] EndInvoke (IAsyncResult asyncResult)
		{
			if (!(asyncResult is AsyncInfo)) throw new ArgumentException ("asyncResult is not the return value from BeginInvoke");

			AsyncInfo ainfo = (AsyncInfo) asyncResult;
			lock (ainfo)
			{
				if (!ainfo.IsCompleted) ainfo.WaitForComplete ();
				if (ainfo.Exception != null) throw ainfo.Exception;
				else return ainfo.Result;
			}
		}

		[MonoTODO]
		public void Discover ()
		{
			throw new NotImplementedException ();
		}

		protected override WebRequest GetWebRequest (Uri uri)
		{
			return base.GetWebRequest (uri);
		}

		//
		// Just for debugging
		//
		void DumpStackFrames ()
		{
			StackTrace st = new StackTrace ();

			for (int i = 0; i < st.FrameCount; i++){
				StackFrame sf = st.GetFrame (i);
				Console.WriteLine ("At frame: {0} {1}", i, sf.GetMethod ().Name);
			}
		}

		WebRequest GetRequestForMessage (Uri uri, SoapClientMessage message)
		{
			WebRequest request = GetWebRequest (uri);
			request.Method = "POST";
			WebHeaderCollection headers = request.Headers;
			headers.Add ("SOAPAction", message.Action);
			request.ContentType = message.ContentType + "; charset=utf-8";
			return request;
		}
		
		void SendRequest (Stream s, SoapClientMessage message, SoapExtension[] extensions)
		{
			using (s) {

				if (extensions != null) {
					s = SoapExtension.ExecuteChainStream (extensions, s);
					message.SetStage (SoapMessageStage.BeforeSerialize);
					SoapExtension.ExecuteProcessMessage (extensions, message, true);
				}

				// What a waste of UTF8encoders, but it has to be thread safe.
				XmlTextWriter xtw = new XmlTextWriter (s, new UTF8Encoding (false));
				xtw.Formatting = Formatting.Indented;

				WebServiceHelper.WriteSoapMessage (xtw, type_info, message.MethodStubInfo.RequestSerializer, message.Parameters, message.Headers);

				if (extensions != null) {
					message.SetStage (SoapMessageStage.AfterSerialize);
					SoapExtension.ExecuteProcessMessage (extensions, message, true);
				}

				xtw.Flush ();
				xtw.Close ();
			 }
		}


		//
		// TODO:
		//    Handle other web responses (multi-output?)
		//    
		object [] ReceiveResponse (WebResponse response, SoapClientMessage message, SoapExtension[] extensions)
		{
			HttpWebResponse http_response = (HttpWebResponse) response;
			HttpStatusCode code = http_response.StatusCode;
			MethodStubInfo msi = message.MethodStubInfo;

			if (!(code == HttpStatusCode.Accepted || code == HttpStatusCode.OK || code == HttpStatusCode.InternalServerError))
				throw new Exception ("Return code was: " + http_response.StatusCode);

			//
			// Remove optional encoding
			//
			Encoding encoding = WebServiceHelper.GetContentEncoding (response.ContentType);

			Stream stream = response.GetResponseStream ();

			if (extensions != null) {
				stream = SoapExtension.ExecuteChainStream (extensions, stream);
				message.SetStage (SoapMessageStage.BeforeDeserialize);
				SoapExtension.ExecuteProcessMessage (extensions, message, false);
			}
			
			// Deserialize the response

			StreamReader reader = new StreamReader (stream, encoding, false);
			string ss = reader.ReadToEnd ();
			Console.WriteLine (ss);
			XmlTextReader xml_reader = new XmlTextReader (new StringReader (ss));

			bool isSuccessful = (code != HttpStatusCode.InternalServerError);
			SoapHeaderCollection headers;
			object content;

			if (isSuccessful) {
				WebServiceHelper.ReadSoapMessage (xml_reader, type_info, msi.ResponseSerializer, out content, out headers);
				message.OutParameters = (object[]) content;
			}
			else {
				WebServiceHelper.ReadSoapMessage (xml_reader, type_info, type_info.FaultSerializer, out content, out headers);
				Fault fault = (Fault) content;
				SoapException ex = new SoapException (fault.faultstring, fault.faultcode, fault.faultactor, fault.detail);
				message.SetException (ex);
			}

			message.SetHeaders (headers);
			message.UpdateHeaderValues (this, message.MethodStubInfo.Headers);

			if (extensions != null) {
				message.SetStage (SoapMessageStage.AfterDeserialize);
				SoapExtension.ExecuteProcessMessage (extensions, message, false);
			}

			if (isSuccessful)
				return message.OutParameters;
			else
				throw message.Exception;
		}

		protected object[] Invoke (string method_name, object[] parameters)
		{
			MethodStubInfo msi = type_info.GetMethod (method_name);
			
			SoapClientMessage message = new SoapClientMessage (this, msi, Url, parameters);
			message.CollectHeaders (this, message.MethodStubInfo.Headers, SoapHeaderDirection.In);

			SoapExtension[] extensions = SoapExtension.CreateExtensionChain (type_info.SoapExtensions[0], msi.SoapExtensions, type_info.SoapExtensions[1]);

			WebResponse response;
			try
			{
				WebRequest request = GetRequestForMessage (uri, message);
				SendRequest (request.GetRequestStream (), message, extensions);
				response = GetWebResponse (request);
			}
			catch (WebException ex)
			{
				response = ex.Response;
				HttpWebResponse http_response = response as HttpWebResponse;
				if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError)
					throw ex;
			}

			return ReceiveResponse (response, message, extensions);
		}

		#endregion // Methods
	}
}
