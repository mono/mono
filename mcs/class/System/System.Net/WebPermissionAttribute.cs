//
// System.Net.WebPermissionAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.Security;
using System.Security.Permissions;using System.Text.RegularExpressions;

namespace System.Net
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class 
		 | AttributeTargets.Struct | AttributeTargets.Constructor 
		 | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]	
	[Serializable]
	public sealed class WebPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Fields
		object m_accept;
		object m_connect;

		// Constructors
		public WebPermissionAttribute (SecurityAction action) : base (action)
		{
		}

		// Properties

		public string Accept {
			get { return m_accept.ToString (); }
			set { 
				if (m_accept != null)
					throw new ArgumentException ("The parameter 'Accept' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Accept' cannot be null.");
				m_accept = value;
			}
		}

		public string AcceptPattern {
			get { return m_accept.ToString (); }
			set { 
				if (m_accept != null)
					throw new ArgumentException ("The parameter 'Accept' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Accept' cannot be null.");
				m_accept = new Regex (value, RegexOptions.IgnoreCase);
			}
		}

		public string Connect {
			get { return m_connect.ToString (); }
			set { 
				if (m_connect != null)
					throw new ArgumentException ("The parameter 'Connect' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Connect' cannot be null.");
				m_connect = value;
			}
		}
		public string ConnectPattern {
			get { return m_connect.ToString (); }
			set { 
				if (m_connect != null)
					throw new ArgumentException ("The parameter 'Connect' can be set only once.");
				if (value == null) 
					throw new ArgumentException ("The parameter 'Connect' cannot be null.");
				m_connect = new Regex (value, RegexOptions.IgnoreCase);
			}
		}

		// Methods

		public override IPermission CreatePermission () 
		{
			if (this.Unrestricted)
				return new WebPermission (PermissionState.Unrestricted);
			WebPermission newPermission = new WebPermission ();
			if (m_accept != null)
			{
				if (m_accept is Regex)
					newPermission.AddPermission (NetworkAccess.Accept, (Regex)m_accept);
				else
					newPermission.AddPermission (NetworkAccess.Accept, (string)m_accept);
			}
			if (m_connect != null)
			{
				if (m_connect is Regex)
					newPermission.AddPermission (NetworkAccess.Connect, (Regex)m_connect);
				else
					newPermission.AddPermission (NetworkAccess.Connect, (string)m_connect);
			}
			return newPermission;
		}
	}
} 

