// 
// System.Web.Services.Protocols.HttpPostWebServiceHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;

namespace System.Web.Services.Protocols 
{
	internal class HttpPostWebServiceHandler: HttpSimpleWebServiceHandler 
	{
		public HttpPostWebServiceHandler (Type type): base (type, typeof(HttpPostTypeStubInfo))
		{
		}
	}
}
