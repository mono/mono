// 
// System.Web.Services.Description.HttpGetProtocolImporter.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
//

using System;
using System.CodeDom;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Configuration;

namespace System.Web.Services.Description 
{
	internal class HttpGetProtocolImporter : HttpSimpleProtocolImporter
	{
		#region Constructors
	
		public HttpGetProtocolImporter ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "HttpGet"; }
		}

		#endregion // Properties

		#region Methods
		
		protected override CodeTypeDeclaration BeginClass ()
		{
			CodeTypeDeclaration codeClass = base.BeginClass ();
			CodeTypeReference ctr = new CodeTypeReference ("System.Web.Services.Protocols.HttpGetClientProtocol");
			codeClass.BaseTypes.Add (ctr);
			return codeClass;
		}
		
		protected override Type GetInMimeFormatter ()
		{
			HttpUrlEncodedBinding bin = OperationBinding.Input.Extensions.Find (typeof(HttpUrlEncodedBinding)) as HttpUrlEncodedBinding;
			if (bin == null) throw new Exception ("Http urlEncoded binding not found");
			return typeof (UrlParameterWriter);
		}

		protected override bool IsBindingSupported ()
		{
			HttpBinding bin = (HttpBinding) Binding.Extensions.Find (typeof(HttpBinding));
			if (bin == null) return false;
			return bin.Verb == "GET";
		}
		
		#endregion
	}
}
