// created on 03/04/2003 at 14:09
// 
// System.Runtime.Remoting.Channels.SoapMessageFormatter
//
// Author:	Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//
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
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;



namespace System.Runtime.Remoting.Channels {
	enum RemMessageType {
		MethodCall, MethodResponse, ServerFault, NotRecognize
	}
	
	internal class SoapMessageFormatter {
		private static FieldInfo _serverFaultExceptionField;
		private Type _serverType;
		private MethodInfo _methodCallInfo;
		private ParameterInfo[] _methodCallParameters;
		private string _xmlNamespace;
		
		static SoapMessageFormatter() {
			// Get the ServerFault exception field FieldInfo that
			// will be used later if an exception occurs on the server
			MemberInfo[] mi = FormatterServices.GetSerializableMembers(typeof(ServerFault), new StreamingContext(StreamingContextStates.All));
			FieldInfo fi;
			for(int i = 0; i < mi.Length; i++){
				fi = mi[i] as FieldInfo;
				if(fi != null && fi.FieldType == typeof(Exception)){
					_serverFaultExceptionField = fi; 
				}
			}
			
		}
		
		internal SoapMessageFormatter() {
			
		}
		
		internal IMessage FormatFault (SoapFault fault, IMethodCallMessage mcm)
		{
			ServerFault sf = fault.Detail as ServerFault;
			Exception e = null;
			
			if (sf != null) {
				if(_serverFaultExceptionField != null)
					e = (Exception) _serverFaultExceptionField.GetValue(sf);
			}
			if (e == null)
				e = new RemotingException (fault.FaultString);

			return new ReturnMessage((System.Exception)e, mcm);
		}
		
		// used by the client
		internal IMessage FormatResponse(ISoapMessage soapMsg, IMethodCallMessage mcm) 
		{
			IMessage rtnMsg;
			
			if(soapMsg.MethodName == "Fault") {
				// an exception was thrown by the server
				Exception e = new SerializationException();
				int i = Array.IndexOf(soapMsg.ParamNames, "detail");
				if(_serverFaultExceptionField != null)
					// todo: revue this 'cause it's not safe
					e = (Exception) _serverFaultExceptionField.GetValue(
							soapMsg.ParamValues[i]);
				
				rtnMsg = new ReturnMessage((System.Exception)e, mcm);
			}
			else {
				object rtnObject = null;
				RemMessageType messageType;
				
				// Get the output of the function if it is not *void*
				if(_methodCallInfo.ReturnType != typeof(void)){
					int index = Array.IndexOf(soapMsg.ParamNames, "return");
					rtnObject = soapMsg.ParamValues[index];
					if(rtnObject is IConvertible) 
						rtnObject = Convert.ChangeType(
								rtnObject, 
								_methodCallInfo.ReturnType);
				}
				
				object[] outParams = new object [_methodCallParameters.Length];
				int n=0;
				
				// check if there are *out* parameters
				foreach(ParameterInfo paramInfo in _methodCallParameters) {
					
					if(paramInfo.ParameterType.IsByRef || paramInfo.IsOut) {
						int index = Array.IndexOf(soapMsg.ParamNames, paramInfo.Name);
						object outParam = soapMsg.ParamValues[index];
						if(outParam is IConvertible)
							outParam = Convert.ChangeType (outParam, paramInfo.ParameterType.GetElementType());
						outParams[n] = outParam;
					}
					else
						outParams [n] = null;
					n++;
				}
				
				Header[] headers = new Header [2 + (soapMsg.Headers != null ? soapMsg.Headers.Length : 0)];
				headers [0] = new Header ("__Return", rtnObject);
				headers [1] = new Header ("__OutArgs", outParams);
				
				if (soapMsg.Headers != null)
					soapMsg.Headers.CopyTo (headers, 2);
					
				rtnMsg = new MethodResponse (headers, mcm);
			}
			return rtnMsg;
		}
		
		// used by the client
		internal SoapMessage BuildSoapMessageFromMethodCall(
				IMethodCallMessage mcm,
				out ITransportHeaders requestHeaders)
		{
			
			requestHeaders = new TransportHeaders();
			SoapMessage soapMsg = new SoapMessage();
			string uri = mcm.Uri;

			GetInfoFromMethodCallMessage(mcm);

			// Format the SoapMessage that will be used to create the RPC
			soapMsg.MethodName = mcm.MethodName;
			int count = mcm.ArgCount;
			ArrayList paramNames = new ArrayList(_methodCallParameters.Length);
			ArrayList paramTypes = new ArrayList(_methodCallParameters.Length);
			ArrayList paramValues = new ArrayList(_methodCallParameters.Length);
			
			// Add the function parameters to the SoapMessage class
			foreach(ParameterInfo paramInfo in _methodCallParameters) {
				if (!(paramInfo.IsOut && paramInfo.ParameterType.IsByRef)) {
					Type t = paramInfo.ParameterType;
					if (t.IsByRef) t = t.GetElementType ();
					paramNames.Add(paramInfo.Name);
					paramTypes.Add(t);
					paramValues.Add(mcm.Args[paramInfo.Position]);
				}
			}			
			soapMsg.ParamNames = (string[]) paramNames.ToArray(typeof(string));
			soapMsg.ParamTypes = (Type[]) paramTypes.ToArray(typeof(Type));
			soapMsg.ParamValues = (object[]) paramValues.ToArray(typeof(object));
			soapMsg.XmlNameSpace = SoapServices.GetXmlNamespaceForMethodCall(_methodCallInfo);
			soapMsg.Headers = BuildMessageHeaders (mcm);

			// Format the transport headers
			requestHeaders["Content-Type"] = "text/xml; charset=\"utf-8\"";
			requestHeaders["SOAPAction"] = "\""+
				SoapServices.GetSoapActionFromMethodBase(_methodCallInfo)+"\""; 
			requestHeaders[CommonTransportKeys.RequestUri] = mcm.Uri;
			
			return soapMsg;
			
		}
		
		// used by the server
		internal IMessage BuildMethodCallFromSoapMessage(SoapMessage soapMessage, string uri) 
		{
			ArrayList headersList = new ArrayList();
			Type[] signature = null;
			
			headersList.Add(new Header("__Uri", uri));
			headersList.Add(new Header("__MethodName", soapMessage.MethodName));
			string typeNamespace, assemblyName;
			bool b = SoapServices.DecodeXmlNamespaceForClrTypeNamespace(soapMessage.XmlNameSpace, out typeNamespace, out assemblyName);

			_serverType = RemotingServices.GetServerTypeForUri(uri);
			headersList.Add(new Header("__TypeName", _serverType.FullName, false));
			
			if (soapMessage.Headers != null) {
				foreach (Header h in soapMessage.Headers) {
					headersList.Add (h);
					if (h.Name == "__MethodSignature")
						signature = (Type[]) h.Value;
				}
			}
			
			_xmlNamespace = soapMessage.XmlNameSpace;
			RemMessageType messageType;
			
			BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			
			if (signature == null)
				_methodCallInfo = _serverType.GetMethod(soapMessage.MethodName, bflags); 
			else
				_methodCallInfo = _serverType.GetMethod(soapMessage.MethodName, bflags, null, signature, null); 

			// the *out* parameters aren't serialized
			// have to add them here
			_methodCallParameters = _methodCallInfo.GetParameters();
			object[] args = new object[_methodCallParameters.Length];
			int sn = 0;
			for (int n=0; n<_methodCallParameters.Length; n++)
			{
				ParameterInfo paramInfo = _methodCallParameters [n];
				Type paramType = (paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType);

				if (paramInfo.IsOut && paramInfo.ParameterType.IsByRef) {
					args [n] = GetNullValue (paramType);
				}
				else{
					object val = soapMessage.ParamValues[sn++];
					if(val is IConvertible) 
						args [n] = Convert.ChangeType (val, paramType);
					else
						args [n] = val;
				}
			}
			
			headersList.Add(new Header("__Args", args, false));
						
			Header[] headers = (Header[])headersList.ToArray(typeof(Header));

			// build the MethodCall from the headers
			MethodCall mthCall = new MethodCall(headers);
			return (IMessage)mthCall;
		}
		
		// used by the server
		internal object BuildSoapMessageFromMethodResponse(IMethodReturnMessage mrm, out ITransportHeaders responseHeaders)
		{
			responseHeaders = new TransportHeaders();

			if(mrm.Exception == null) {
				// *normal* function return
				
				SoapMessage soapMessage = new SoapMessage();
				
				// fill the transport headers
				responseHeaders["Content-Type"] = "text/xml; charset=\"utf-8\"";

				// build the SoapMessage
				ArrayList paramNames = new ArrayList();
				ArrayList paramValues = new ArrayList();
				ArrayList paramTypes = new ArrayList();
				soapMessage.MethodName = mrm.MethodName+"Response";
				
				Type retType = ((MethodInfo)mrm.MethodBase).ReturnType;
				
				if(retType != typeof(void)) {
					paramNames.Add("return");
					paramValues.Add(mrm.ReturnValue);
					if (mrm.ReturnValue != null)
						paramTypes.Add(mrm.ReturnValue.GetType());
					else
						paramTypes.Add(retType);
				}
				
				for(int i = 0; i < mrm.OutArgCount; i++){
					paramNames.Add(mrm.GetOutArgName(i));
					paramValues.Add(mrm.GetOutArg(i));
					if(mrm.GetOutArg(i) != null) paramTypes.Add(mrm.GetOutArg(i).GetType());
				}
				soapMessage.ParamNames = (string[]) paramNames.ToArray(typeof(string));
				soapMessage.ParamValues = (object[]) paramValues.ToArray(typeof(object));
				soapMessage.ParamTypes = (Type[]) paramTypes.ToArray(typeof(Type));
				soapMessage.XmlNameSpace = _xmlNamespace;
				soapMessage.Headers = BuildMessageHeaders (mrm);
				return soapMessage;
			}
			else {
				// an Exception was thrown while executing the function
				responseHeaders["__HttpStatusCode"] = "400";
				responseHeaders["__HttpReasonPhrase"] = "Bad Request";
				// fill the transport headers
				responseHeaders["Content-Type"] = "text/xml; charset=\"utf-8\"";
				ServerFault serverFault = CreateServerFault(mrm.Exception);
				return new SoapFault("Server", String.Format(" **** {0} - {1}", mrm.Exception.GetType().ToString(), mrm.Exception.Message), null, serverFault);
			}
		}
		
		internal SoapMessage CreateSoapMessage (bool isRequest)
		{
			if (isRequest) return new SoapMessage ();
			
			int n = 0;
			Type[] types = new Type [_methodCallParameters.Length + 1];
			
			if (_methodCallInfo.ReturnType != typeof(void)) {
				types[0] = _methodCallInfo.ReturnType;
				n++;
			}
				
			foreach(ParameterInfo paramInfo in _methodCallParameters)
			{
				if (paramInfo.ParameterType.IsByRef || paramInfo.IsOut)
				{
					Type t = paramInfo.ParameterType;
					if (t.IsByRef) t = t.GetElementType();
					types [n++] = t;
				}
			}
			SoapMessage sm = new SoapMessage ();
			sm.ParamTypes = types;
			return sm;
		}
		
		// used by the server when an exception is thrown
		// by the called function
		internal ServerFault CreateServerFault(Exception e) {
			// it's really strange here
			// a ServerFault object has a private System.Exception member called *exception*
			// (have a look at a MS Soap message when an exception occurs on the server)
			// but there is not public .ctor with an Exception as parameter...????....
			// (maybe an internal one). So I searched another way...
			ServerFault sf = (ServerFault) FormatterServices.GetUninitializedObject(typeof(ServerFault));
			MemberInfo[] mi = FormatterServices.GetSerializableMembers(typeof(ServerFault), new StreamingContext(StreamingContextStates.All));
			
			FieldInfo fi;
			object[] mv = new object[mi.Length];
			for(int i = 0; i < mi.Length; i++) {
				fi = mi[i] as FieldInfo;
				if(fi != null && fi.FieldType == typeof(Exception)) mv[i] = e;
			}
			sf = (ServerFault) FormatterServices.PopulateObjectMembers(sf, mi, mv);
			
			return sf;
		}

		internal void GetInfoFromMethodCallMessage(IMethodCallMessage mcm) {
			_serverType = Type.GetType(mcm.TypeName, true);
			
			if (mcm.MethodSignature != null) 
				_methodCallInfo = _serverType.GetMethod(mcm.MethodName, 
														BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
														null, (Type []) mcm.MethodSignature, null);
			else
				_methodCallInfo = _serverType.GetMethod(mcm.MethodName, 
														BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			_methodCallParameters = _methodCallInfo.GetParameters();
		}	
		
		Header[] BuildMessageHeaders (IMethodMessage msg)
		{
			ArrayList headers = new ArrayList (1);
			foreach (string key in msg.Properties.Keys) 
			{
				switch (key) {
					case "__Uri":
					case "__MethodName":
					case "__TypeName":
					case "__Args":
					case "__OutArgs":
					case "__Return":
					case "__MethodSignature":
					case "__CallContext":
						continue;
	
					default:
						object value = msg.Properties [key];
						if (value != null)
							headers.Add (new Header (key, value, false, "http://schemas.microsoft.com/clr/soap/messageProperties"));
						break;
				}
			}
			
			if (RemotingServices.IsMethodOverloaded (msg))
				headers.Add (new Header ("__MethodSignature", msg.MethodSignature, false, "http://schemas.microsoft.com/clr/soap/messageProperties"));
			
			if (msg.LogicalCallContext != null && msg.LogicalCallContext.HasInfo)
				headers.Add (new Header ("__CallContext", msg.LogicalCallContext, false, "http://schemas.microsoft.com/clr/soap/messageProperties"));
			
			if (headers.Count == 0) return null;
			return (Header[]) headers.ToArray (typeof(Header));
		}
		
		object GetNullValue (Type paramType)
		{
			switch (Type.GetTypeCode (paramType))
			{
				case TypeCode.Boolean: return false;
				case TypeCode.Byte: return (byte)0;
				case TypeCode.Char: return '\0';
				case TypeCode.Decimal: return (decimal)0;
				case TypeCode.Double: return (double)0;
				case TypeCode.Int16: return (short)0;
				case TypeCode.Int32: return (int)0;
				case TypeCode.Int64: return (long)0;
				case TypeCode.SByte: return (sbyte)0;
				case TypeCode.Single: return (float)0;
				case TypeCode.UInt16: return (ushort)0;
				case TypeCode.UInt32: return (uint)0;
				case TypeCode.UInt64: return (ulong)0;
				default: return null;
			}
		}
	}
}
