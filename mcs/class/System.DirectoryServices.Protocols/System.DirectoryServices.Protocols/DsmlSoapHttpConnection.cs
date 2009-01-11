//
// DsmlSoapHttpConnection.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.
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
using System.DirectoryServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Permissions;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlSoapHttpConnection : DsmlSoapConnection
	{
		public DsmlSoapHttpConnection (DsmlDirectoryIdentifier identifier)
		{
			throw new NotImplementedException ();
		}

		public DsmlSoapHttpConnection (Uri uri)
		{
			throw new NotImplementedException ();
		}

		public DsmlSoapHttpConnection (DsmlDirectoryIdentifier identifier, NetworkCredential credential)
		{
			throw new NotImplementedException ();
		}

		public DsmlSoapHttpConnection (DsmlDirectoryIdentifier identifier, NetworkCredential credential, AuthType authType)
		{
			throw new NotImplementedException ();
		}

		public AuthType AuthType { get; set; }
		[MonoTODO]
		public override string SessionId {
			get { throw new NotImplementedException (); }
		}
		public string SoapActionHeader { get; set; }
		public override TimeSpan Timeout { get; set; }

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public void Abort (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		[NetworkInformationPermission (SecurityAction.Assert, Unrestricted = true)]
		[WebPermission (SecurityAction.Assert, Unrestricted = true)]
		public IAsyncResult BeginSendRequest (DsmlRequestDocument request, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[NetworkInformationPermission (SecurityAction.Assert, Unrestricted = true)]
		[WebPermission (SecurityAction.Assert, Unrestricted = true)]
		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public override void BeginSession ()
		{
			throw new NotImplementedException ();
		}

		public DsmlResponseDocument EndSendRequest (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		[NetworkInformationPermission (SecurityAction.Assert, Unrestricted = true)]
		[WebPermission (SecurityAction.Assert, Unrestricted = true)]
		public override void EndSession ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public override DirectoryResponse SendRequest (DirectoryRequest request)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public DsmlResponseDocument SendRequest (DsmlRequestDocument request)
		{
			throw new NotImplementedException ();
		}
	}
}
