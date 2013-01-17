//
// System.Net.WebPermissionAttribute.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Andreas Nahr
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Security;
using System.Security.Permissions;

namespace System.Net {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class 
		 | AttributeTargets.Struct | AttributeTargets.Constructor 
		 | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]	
	[Serializable]
	public sealed class WebPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		object m_accept;
		object m_connect;

		// Constructors
		public WebPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		// Properties

		public string Accept {
			get {
				if (m_accept == null)
					return null;
				return (m_accept as WebPermissionInfo).Info; 
			}
			set { 
				if (m_accept != null)
					AlreadySet ("Accept", "Accept");
				m_accept = new WebPermissionInfo (WebPermissionInfoType.InfoString, value);
			}
		}

		public string AcceptPattern {
			get {
				if (m_accept == null)
					return null;
				return (m_accept as WebPermissionInfo).Info; 
			}
			set { 
				if (m_accept != null)
					AlreadySet ("Accept", "AcceptPattern");
				if (value == null) 
					throw new ArgumentNullException ("AcceptPattern");

				m_accept = new WebPermissionInfo (WebPermissionInfoType.InfoUnexecutedRegex , value); 
			}
		}

		public string Connect {
			get {
				if (m_connect == null)
					return null;
				return (m_connect as WebPermissionInfo).Info; 
			}
			set { 
				if (m_connect != null)
					AlreadySet ("Connect", "Connect");
				m_connect = new WebPermissionInfo (WebPermissionInfoType.InfoString, value);
			}
		}

		public string ConnectPattern {
			get {
				if (m_connect == null)
					return null;
				return (m_connect as WebPermissionInfo).Info; 
			}
			set { 
				if (m_connect != null)
					AlreadySet ("Connect", "ConnectConnectPattern");
				if (value == null) 
					throw new ArgumentNullException ("ConnectPattern");

				m_connect = new WebPermissionInfo (WebPermissionInfoType.InfoUnexecutedRegex , value);
			}
		}

		// Methods

		public override IPermission CreatePermission () 
		{
			if (this.Unrestricted)
				return new WebPermission (PermissionState.Unrestricted);

			WebPermission newPermission = new WebPermission ();
			if (m_accept != null) {
				newPermission.AddPermission (NetworkAccess.Accept, (WebPermissionInfo) m_accept);
			}
			if (m_connect != null) {
				newPermission.AddPermission (NetworkAccess.Connect, (WebPermissionInfo) m_connect);
			}
			return newPermission;
		}

		// helpers

		internal void AlreadySet (string parameter, string property)
		{
			string msg = Locale.GetText ("The parameter '{0}' can be set only once.");
			throw new ArgumentException (String.Format (msg, parameter), property);
		}
	}
} 
