// 
// System.Web.Services.Protocols.HttpSimpleWebServiceHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
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
		
		public HttpSimpleWebServiceHandler (Type type, string protocolName): base (type)
		{
			_typeInfo = (HttpSimpleTypeStubInfo) TypeStubManager.GetTypeStub (type, protocolName);
		}
		
		protected HttpSimpleTypeStubInfo TypeStub
		{
			get { return _typeInfo; }
		}
		
		public override void ProcessRequest (HttpContext context)
		{
			Context = context;
			string name = context.Request.PathInfo;
			if (name.StartsWith ("/")) name = name.Substring (1);
			
			Stream respStream = null;
			Exception error = null;
			try
			{
				HttpSimpleMethodStubInfo method = (HttpSimpleMethodStubInfo) _typeInfo.GetMethod (name);
				if (method == null) throw new InvalidOperationException ("Method " + name + " not defined in service " + ServiceType.Name);
			
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
			{
				try
				{
					context.Response.ContentType = "text/plain";
					context.Response.StatusCode = 500;
					context.Response.Write (error.Message);
				}
				catch {}
			}
			
			if (respStream != null)
				respStream.Close ();
		}
		
		object Invoke (LogicalMethodInfo method, object[] parameters)
		{
			try
			{
				object server = CreateServerInstance ();
				object[] res = method.Invoke (server, parameters);
				if (!method.IsVoid) return res[0];
				else return null;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}
