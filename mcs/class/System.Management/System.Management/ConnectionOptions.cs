//
// System.Management.ConnectionOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class ConnectionOptions : ManagementOptions, ICloneable
	{
		[MonoTODO]
		public ConnectionOptions ()
		{
		}

		[MonoTODO]
		public ConnectionOptions (string locale,
					  string username,
					  string password,
					  string authority,
					  ImpersonationLevel impersonation,
					  AuthenticationLevel authentication,
					  bool enablePrivileges,
					  ManagementNamedValueCollection context,
					  TimeSpan timeout)
		{
		}

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public AuthenticationLevel Authentication
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Authority
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool EnablePrivileges
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ImpersonationLevel Impersonation
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Locale
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Password
		{
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Username
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

