// 
// System.Web.Services.Description.HttpGetProtocolImporter.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
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
