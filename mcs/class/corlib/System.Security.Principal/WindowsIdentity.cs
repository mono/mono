//
// System.Security.Principal.WindowsIdentity
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Security.Principal {

	[Serializable]
#if NET_1_0
	public class WindowsIdentity : IIdentity, IDeserializationCallback {
#else
	public class WindowsIdentity : IIdentity, IDeserializationCallback, ISerializable {
#endif
		private IntPtr _token;
		private string _type;
		private WindowsAccountType _account;
		private bool _authenticated;
		private string _name;

		public WindowsIdentity (IntPtr userToken) 
			: this (userToken, "NTLM", WindowsAccountType.Normal, false) {}

		public WindowsIdentity (IntPtr userToken, string type) 
			: this (userToken, type, WindowsAccountType.Normal, false) {}

		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType)
			: this (userToken, type, acctType, false) {}

		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
		{
			if (userToken == IntPtr.Zero)
				throw new ArgumentException ("Invalid token");

			_token = userToken;
			_type = type;
			_account = acctType;
			_authenticated = isAuthenticated;
			_name = null;
		}
#if !NET_1_0
		public WindowsIdentity (string sUserPrincipalName) 
			: this (sUserPrincipalName, null) {}

		[MonoTODO]
		public WindowsIdentity (string sUserPrincipalName, string type)
		{
			if (sUserPrincipalName == null)
				throw new NullReferenceException ("sUserPrincipalName");

			throw new ArgumentException ("only for Windows Server 2003 +");
		}

		[MonoTODO]
		public WindowsIdentity (SerializationInfo info, StreamingContext context) {}
#endif
		[MonoTODO]
		~WindowsIdentity ()
		{
			_token = IntPtr.Zero;
		}

		// static methods

		public static WindowsIdentity GetAnonymous ()
		{
			WindowsIdentity id = new WindowsIdentity ((IntPtr)1, String.Empty, WindowsAccountType.Anonymous, false);
			// special case
			id._token = IntPtr.Zero;
			id._name = String.Empty;
			return id;
		}

		[MonoTODO]
		public static WindowsIdentity GetCurrent ()
		{
			throw new NotImplementedException ();
		}

		// methods

		public virtual WindowsImpersonationContext Impersonate ()
		{
			return new WindowsImpersonationContext (_token);
		}

		public static WindowsImpersonationContext Impersonate (IntPtr userToken)
		{
			return new WindowsImpersonationContext (userToken);
		}

		// properties

		public virtual string AuthenticationType
		{
			get { return _type; }
		}

		public virtual bool IsAnonymous
		{
			get { return (_account == WindowsAccountType.Anonymous); }
		}

		public virtual bool IsAuthenticated
		{
			get { return _authenticated; }
		}

		public virtual bool IsGuest
		{
			get { return (_account == WindowsAccountType.Guest); }
		}

		public virtual bool IsSystem
		{
			get { return (_account == WindowsAccountType.System); }
		}

		[MonoTODO ("resolve missing")]
		public virtual string Name
		{
			get {
				if (_name == null) {
					// TODO: resolve name from token
					throw new NotImplementedException ();
				}
				return _name; 
			}
		}

		public virtual IntPtr Token
		{
			get { return _token; }
		}

		[MonoTODO]
		void IDeserializationCallback.OnDeserialization (object sender)
		{
			throw new NotImplementedException ();
		}
#if !NET_1_0
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			throw new NotImplementedException ();
		}
#endif
	}
}

