// 
// System.Web.Services.Protocols.HttpGetWebServiceHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;

namespace System.Web.Services.Protocols 
{
	internal class HttpGetWebServiceHandler: HttpSimpleWebServiceHandler 
	{
		public HttpGetWebServiceHandler (Type type): base (type, typeof(HttpGetTypeStubInfo))
		{
		}
	}
}
