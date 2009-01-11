//
// LdapConnection.cs
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
using System.Security.Permissions;

namespace System.DirectoryServices.Protocols
{
	[MonoTODO]
	public class LdapConnection : DirectoryConnection, IDisposable
	{
		public LdapConnection (LdapDirectoryIdentifier identifier)
		{
			throw new NotImplementedException ();
		}

		public LdapConnection (string server)
		{
			throw new NotImplementedException ();
		}

		public LdapConnection (LdapDirectoryIdentifier identifier, NetworkCredential credential)
		{
			throw new NotImplementedException ();
		}

		public LdapConnection (LdapDirectoryIdentifier identifier, NetworkCredential credential, AuthType authType)
		{
			throw new NotImplementedException ();
		}

		~LdapConnection ()
		{
		}

		public AuthType AuthType { get; set; }
		public bool AutoBind { get; set; }
		[MonoTODO]
		public override NetworkCredential Credential {
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public LdapSessionOptions SessionOptions {
			get { throw new NotImplementedException (); }
		}
		public override TimeSpan Timeout { get; set; }


		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public void Abort (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public IAsyncResult BeginSendRequest (DirectoryRequest request, PartialResultProcessing partialMode, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public IAsyncResult BeginSendRequest (DirectoryRequest request, TimeSpan requestTimeout, PartialResultProcessing partialMode, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public void Bind ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public void Bind (NetworkCredential newCredential)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public DirectoryResponse EndSendRequest (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		public PartialResultsCollection GetPartialResults (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public override DirectoryResponse SendRequest (DirectoryRequest request)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public DirectoryResponse SendRequest (DirectoryRequest request, TimeSpan requestTimeout)
		{
			throw new NotImplementedException ();
		}
	}
}
