//
// System.Security.Principal.WindowsIdentity
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Security.Principal
{
	[Serializable]
	public class WindowsIdentity : IIdentity, IDeserializationCallback
	{
		public WindowsIdentity (IntPtr userToken)
		{
		}

		[MonoTODO]
		public WindowsIdentity (IntPtr userToken, string type)
		{
		}

		[MonoTODO]
		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType)
		{
		}

		[MonoTODO]
		public WindowsIdentity (IntPtr userToken,
					string type,
					WindowsAccountType acctType,
					bool isAuthenticated)
		{
		}

		[MonoTODO]
		~WindowsIdentity ()
		{
		}

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

		[MonoTODO]
		public virtual WindowsImpersonationContext Impersonate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static WindowsImpersonationContext Impersonate (IntPtr userToken)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string AuthenticationType
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool IsAnonymous
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool IsAuthenticated
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool IsGuest
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool IsSystem
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual string Name
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual IntPtr Token
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		void IDeserializationCallback.OnDeserialization (object sender)
		{
			throw new NotImplementedException ();
		}
	}
}

