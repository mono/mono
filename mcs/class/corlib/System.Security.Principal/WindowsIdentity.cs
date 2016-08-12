//
// System.Security.Principal.WindowsIdentity
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Claims;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Principal {

	[Serializable]
	[ComVisible (true)]
	public class WindowsIdentity :
	System.Security.Claims.ClaimsIdentity,
	IIdentity, IDeserializationCallback, ISerializable, IDisposable {
		private IntPtr _token;
		private string _type;
		private WindowsAccountType _account;
		private bool _authenticated;
		private string _name;
		private SerializationInfo _info;

		static private IntPtr invalidWindows = IntPtr.Zero;

		[NonSerialized]
		public new const string DefaultIssuer = "AD AUTHORITY";

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (IntPtr userToken) 
			: this (userToken, null, WindowsAccountType.Normal, false)
		{
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (IntPtr userToken, string type) 
			: this (userToken, type, WindowsAccountType.Normal, false)
		{
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType)
			: this (userToken, type, acctType, false)
		{
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
		{
			_type = type;
			_account = acctType;
			_authenticated = isAuthenticated;
			_name = null;
			// last - as it can override some fields
			SetToken (userToken);
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (string sUserPrincipalName) 
			: this (sUserPrincipalName, null)
		{
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (string sUserPrincipalName, string type)
		{
			if (sUserPrincipalName == null)
				throw new NullReferenceException ("sUserPrincipalName");

			// TODO: Windows 2003 compatibility should be done in runtime
			IntPtr token = GetUserToken (sUserPrincipalName);
			if ((!Environment.IsUnix) && (token == IntPtr.Zero)) {
				throw new ArgumentException ("only for Windows Server 2003 +");
			}

			_authenticated = true;
			_account = WindowsAccountType.Normal;
			_type = type;
			// last - as it can override some fields
			SetToken (token);
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public WindowsIdentity (SerializationInfo info, StreamingContext context)
		{
			_info = info;
		}

		internal WindowsIdentity (ClaimsIdentity claimsIdentity, IntPtr userToken)
			: base (claimsIdentity)
		{
			if (userToken != IntPtr.Zero && userToken.ToInt64() > 0)
			{
				SetToken (userToken);
			}
		}

		[ComVisible (false)]
		public void Dispose ()
		{
			_token = IntPtr.Zero;
		}
		
		[ComVisible (false)]
		protected virtual void Dispose (bool disposing)
		{
			_token = IntPtr.Zero;
		}
		// static methods

		public static WindowsIdentity GetAnonymous ()
		{
			WindowsIdentity id = null;
			if (Environment.IsUnix) {
				id = new WindowsIdentity ("nobody");
				// special case
				id._account = WindowsAccountType.Anonymous;
				id._authenticated = false;
				id._type = String.Empty;
			}
			else {
				id = new WindowsIdentity (IntPtr.Zero, String.Empty, WindowsAccountType.Anonymous, false);
				// special case (don't try to resolve the name)
				id._name = String.Empty;
			}
			return id;
		}

		public static WindowsIdentity GetCurrent ()
		{
			return new WindowsIdentity (GetCurrentToken (), null, WindowsAccountType.Normal, true);
		}
		[MonoTODO ("need icall changes")]
		public static WindowsIdentity GetCurrent (bool ifImpersonating)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("need icall changes")]
		public static WindowsIdentity GetCurrent (TokenAccessLevels desiredAccess)
		{
			throw new NotImplementedException ();
		}
		// methods

		public virtual WindowsImpersonationContext Impersonate ()
		{
			return new WindowsImpersonationContext (_token);
		}

		[SecurityPermission (SecurityAction.Demand, ControlPrincipal=true)]
		public static WindowsImpersonationContext Impersonate (IntPtr userToken)
		{
			return new WindowsImpersonationContext (userToken);
		}

		[SecuritySafeCritical]
		public static void RunImpersonated (SafeAccessTokenHandle safeAccessTokenHandle, Action action)
		{
			throw new NotImplementedException ();
		}

		[SecuritySafeCritical]
		public static T RunImpersonated<T> (SafeAccessTokenHandle safeAccessTokenHandle, Func<T> func)
		{
			throw new NotImplementedException ();
		}

		// properties
		sealed override
		public string AuthenticationType {
			get { return _type; }
		}

		public virtual bool IsAnonymous
		{
			get { return (_account == WindowsAccountType.Anonymous); }
		}

		override
		public bool IsAuthenticated
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

		override
		public string Name
		{
			get {
				if (_name == null) {
					// revolve name (runtime)
					_name = GetTokenName (_token);
				}
				return _name; 
			}
		}

		public virtual IntPtr Token
		{
			get { return _token; }
		}
		[MonoTODO ("not implemented")]
		public IdentityReferenceCollection Groups {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("not implemented")]
		[ComVisible (false)]
		public TokenImpersonationLevel ImpersonationLevel {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("not implemented")]
		[ComVisible (false)]
		public SecurityIdentifier Owner {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("not implemented")]
		[ComVisible (false)]
		public SecurityIdentifier User {
			get { throw new NotImplementedException (); }
		}
		void IDeserializationCallback.OnDeserialization (object sender)
		{
			_token = (IntPtr) _info.GetValue ("m_userToken", typeof (IntPtr));
			// can't trust this alone - we must validate the token
			_name = _info.GetString ("m_name");
			if (_name != null) {
				// validate token by comparing names
				string name = GetTokenName (_token);
				if (name != _name)
					throw new SerializationException ("Token-Name mismatch.");
			}
			else {
				// validate token by getting name
				_name = GetTokenName (_token);
				if (_name == null)
					throw new SerializationException ("Token doesn't match a user.");
			}
			_type = _info.GetString ("m_type");
			_account = (WindowsAccountType) _info.GetValue ("m_acctType", typeof (WindowsAccountType));
			_authenticated = _info.GetBoolean ("m_isAuthenticated");
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			info.AddValue ("m_userToken", _token);
			// can be null when not resolved
			info.AddValue ("m_name", _name);
			info.AddValue ("m_type", _type);
			info.AddValue ("m_acctType", _account);
			info.AddValue ("m_isAuthenticated", _authenticated);
		}

		internal ClaimsIdentity CloneAsBase ()
		{
			return base.Clone();
		}

		internal IntPtr GetTokenInternal ()
		{
			return _token;
		}

		private void SetToken (IntPtr token) 
		{
			if (Environment.IsUnix) {

				_token = token;
				// apply defaults
				if (_type == null)
					_type = "POSIX";
				// override user choice in this specific case
				if (_token == IntPtr.Zero)
					_account = WindowsAccountType.System;
			}
			else {
				if ((token == invalidWindows) && (_account != WindowsAccountType.Anonymous))
					throw new ArgumentException ("Invalid token");

				_token = token;
				// apply defaults
				if (_type == null)
					_type = "NTLM";
			}
		}

		public SafeAccessTokenHandle AccessToken {
			get { throw new NotImplementedException (); }
		}

		// see mono/mono/metadata/security.c for implementation

		// Many people use reflection to get a user's roles - so many 
		// that's it's hard to say it's an "undocumented" feature -
		// so we also implement it in Mono :-/
		// http://www.dotnet247.com/247reference/msgs/39/195403.aspx
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string[] _GetRoles (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static IntPtr GetCurrentToken ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string GetTokenName (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static IntPtr GetUserToken (string username);
	}
}
