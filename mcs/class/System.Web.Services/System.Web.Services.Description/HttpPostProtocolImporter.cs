// 
// System.Web.Services.Description.HttpPostProtocolImporter.cs
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
	internal class HttpPostProtocolImporter : HttpSimpleProtocolImporter
	{
		#region Constructors
	
		public HttpPostProtocolImporter ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "HttpPost"; }
		}

		#endregion // Properties

		#region Methods
		
		protected override CodeTypeDeclaration BeginClass ()
		{
			CodeTypeDeclaration codeClass = base.BeginClass ();
			CodeTypeReference ctr = new CodeTypeReference ("System.Web.Services.Protocols.HttpPostClientProtocol");
			codeClass.BaseTypes.Add (ctr);
			return codeClass;
		}

		protected override Type GetInMimeFormatter ()
		{
			MimeContentBinding bin = OperationBinding.Input.Extensions.Find (typeof(MimeContentBinding)) as MimeContentBinding;
			if (bin == null) throw new Exception ("Http mime:content binding not found");
			if (bin.Type != "application/x-www-form-urlencoded") throw new Exception ("Encoding of mime:content binding not supported");
			return typeof (HtmlFormParameterWriter);
		}

		protected override bool IsBindingSupported ()
		{
			HttpBinding bin = (HttpBinding) Binding.Extensions.Find (typeof(HttpBinding));
			if (bin == null) return false;
			return bin.Verb == "POST";
		}
		
		#endregion
	}
}
