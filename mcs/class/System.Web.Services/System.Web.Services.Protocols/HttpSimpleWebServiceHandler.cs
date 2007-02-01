// 
// System.Web.Services.Protocols.HttpSimpleWebServiceHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
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
using System.Reflection;
using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols 
{
	internal class HttpSimpleWebServiceHandler: WebServiceHandler 
	{
		HttpSimpleTypeStubInfo _typeInfo;
		HttpSimpleMethodStubInfo method;
		
		public HttpSimpleWebServiceHandler (Type type, string protocolName)
		: base (type)
		{
			_typeInfo = (HttpSimpleTypeStubInfo) TypeStubManager.GetTypeStub (type, protocolName);
		}
		
		protected HttpSimpleTypeStubInfo TypeStub
		{
			get { return _typeInfo; }
		}
		
		internal override MethodStubInfo GetRequestMethod (HttpContext context)
		{
			string name = context.Request.PathInfo;
			if (name.StartsWith ("/"))
				name = name.Substring (1);

			method = (HttpSimpleMethodStubInfo) _typeInfo.GetMethod (name);
			if (method == null) 
				WriteError (context, "Method " + name + " not defined in service " + ServiceType.Name);
			
			return method;
		}
		
		public override void ProcessRequest (HttpContext context)
		{
			Context = context;
			Stream respStream = null;
			Exception error = null;
			
			try
			{
				if (method == null) 
					method = (HttpSimpleMethodStubInfo) GetRequestMethod (context);

				if (method.MethodInfo.EnableSession)
					Session = context.Session;
			
				MimeParameterReader parameterReader = (MimeParameterReader) method.ParameterReaderType.Create ();
				object[] parameters = parameterReader.Read (context.Request);
		
				MimeReturnWriter returnWriter = (MimeReturnWriter) method.ReturnWriterType.Create ();
				object result = Invoke (method.MethodInfo, parameters);
				respStream = context.Response.OutputStream;
				returnWriter.Write (context.Response, respStream, result);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			
			if (error != null)
				WriteError (context, error.ToString ());
			
			if (respStream != null)
				respStream.Close ();
		}
		
		void WriteError (HttpContext context, string msg)
		{
			try
			{
				context.Response.ContentType = "text/plain";
				context.Response.StatusCode = 500;
				context.Response.Write (msg);
				context.Response.End ();
			}
			catch {}
		}
		
		object Invoke (LogicalMethodInfo method, object[] parameters)
		{
			try
			{
				object server = CreateServerInstance ();
				try {
					object[] res = method.Invoke (server, parameters);
					if (!method.IsVoid) return res[0];
					else return null;
				}
				finally {
					IDisposable disp = server as IDisposable;
					if (disp != null)
						disp.Dispose();
				}
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}
