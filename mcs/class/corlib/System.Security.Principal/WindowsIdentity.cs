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

		public WindowsIdentity (IntPtr userToken)
		{
			_token = userToken;
			_type = "NTLM";
			_account = WindowsAccountType.Normal;
			_authenticated = false;
		}

		public WindowsIdentity (IntPtr userToken, string type)
		{
			_token = userToken;
			_type = type;
			_account = WindowsAccountType.Normal;
			_authenticated = false;
		}

		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType)
		{
			_token = userToken;
			_type = type;
			_account = acctType;
			_authenticated = false;
		}

		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
		{
			_token = userToken;
			_type = type;
			_account = acctType;
			_authenticated = isAuthenticated;
		}
#if !NET_1_0
		[MonoTODO]
		public WindowsIdentity (string sUserPrincipalName) 
		{
			throw new ArgumentException ("only for Windows Server 2003 +");
		}

		[MonoTODO]
		public WindowsIdentity (string sUserPrincipalName, string type)
		{
			throw new ArgumentException ("only for Windows Server 2003 +");
		}

		[MonoTODO]
		public WindowsIdentity (SerializationInfo info, StreamingContext context) {}
#endif
		[MonoTODO]
		~WindowsIdentity ()
		{
			_token = (IntPtr) 0;
		}

		// methods

		[MonoTODO]
		public static WindowsIdentity GetAnonymous ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static WindowsIdentity GetCurrent ()
		{
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		public virtual string Name
		{
			get {
				throw new NotImplementedException ();
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

