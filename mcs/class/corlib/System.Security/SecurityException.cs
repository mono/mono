//
// System.Security.SecurityException.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

namespace System.Security {

	[Serializable]
#if NET_2_0
	public class SecurityException : SystemException, _Exception {
#else
	public class SecurityException : SystemException {
#endif
		// Fields
		string permissionState;
		Type permissionType;
		private string _granted;
		private string _refused;
#if NET_2_0
		private SecurityAction _action;
		private object _demanded;
		private object _denyset;
		private object _permitset;
		private AssemblyName _assembly;
		private IPermission _firstperm;
		private IPermission _permfailed;
		private MethodInfo _method;
		private string _url;
		private SecurityZone _zone;
#endif

		// Properties

#if NET_2_0
		public SecurityAction Action {
			get { return _action; }
			set { _action = value; }
		}

		public object Demanded {
			get { return _demanded; }
			set { _demanded = value; }
		}

		public object DenySetInstance {
			get { return _denyset; }
			set { _denyset = value; }
		}

		public AssemblyName FailedAssemblyInfo {
			get { return _assembly; }
			set { _assembly = value; }
		}

		public IPermission FirstPermissionThatFailed {
			get { return _firstperm; }
			set { _firstperm = value; }
		}

		public MethodInfo Method {
			get { return _method; }
			set { _method = value; }
		}

		[Obsolete]
		public IPermission PermissionThatFailed {
			get { return _permfailed; }
			set { _permfailed = value; }
		}

		public object PermitOnlySetInstance {
			get { return _permitset; }
			set { _permitset = value; }
		}

		public string Url {
			get { return _url; }
			set { _url = value; }
		}

		public SecurityZone Zone {
			get { return _zone; }
			set { _zone = value; }
		}
#endif

		public string PermissionState {
			get { return permissionState; }
#if NET_2_0
			set { permissionState = value; }
#endif
		}

		public Type PermissionType {
			get { return permissionType; }
#if NET_2_0
			set { permissionType = value; }
#endif
		}

#if NET_1_1
		public string GrantedSet {
			get { return _granted; }
#if NET_2_0
			set { _granted = value; }
#endif
		}

		public string RefusedSet {
			get { return _refused; }
#if NET_2_0
			set { _refused = value; }
#endif
		}
#endif
		// Constructors

		public SecurityException ()
			: base (Locale.GetText ("A security error has been detected."))
		{
			base.HResult = unchecked ((int)0x8013150A);
		}

		public SecurityException (string message) 
			: base (message)
		{
			base.HResult = unchecked ((int)0x8013150A);
		}
		
		protected SecurityException (SerializationInfo info, StreamingContext context) 
			: base (info, context)
		{
			base.HResult = unchecked ((int)0x8013150A);
			permissionState = info.GetString ("PermissionState");
		}
		
		public SecurityException (string message, Exception inner) 
			: base (message, inner)
		{
			base.HResult = unchecked ((int)0x8013150A);
		}
		
		public SecurityException (string message, Type type) 
			:  base (message) 
		{
			base.HResult = unchecked ((int)0x8013150A);
			permissionType = type;
		}
		
		public SecurityException (string message, Type type, string state) 
			: base (message) 
		{
			base.HResult = unchecked ((int)0x8013150A);
			permissionType = type;
			permissionState = state;
		}

		internal SecurityException (string message, PermissionSet granted, PermissionSet refused) 
			: base (message)
		{
			base.HResult = unchecked ((int)0x8013150A);
			_granted = granted.ToString ();
			_refused = refused.ToString ();
		}

#if NET_2_0
		public SecurityException (string message, object deny, object permitOnly, MethodInfo method, 
			object demanded, IPermission permThatFailed)
			: base (message)
		{
			_denyset = deny;
			_permitset = permitOnly;
			_method = method;
			_demanded = demanded;
			_permfailed = permThatFailed;
		}

		public SecurityException (string message, AssemblyName assemblyName, PermissionSet grant, 
			PermissionSet refused, MethodInfo method, SecurityAction action, object demanded, 
			IPermission permThatFailed, Evidence evidence)
			: base (message)
		{
			_assembly = assemblyName;
			_granted = grant.ToString ();
			_refused = refused.ToString ();
			_method = method;
			_action = action;
			_demanded = demanded;
			_permfailed = permThatFailed;
			// FIXME ? evidence ?
		}
#endif

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("PermissionState", permissionState);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder (base.ToString ());
			if (permissionState != null) {
				sb.Append (Environment.NewLine);
				sb.Append ("State: ");
				sb.Append (permissionState);
			}
			if (permissionType != null) {
				sb.Append (Environment.NewLine);
				sb.Append ("Type: ");
				sb.Append (permissionType.ToString ());
			}
#if NET_1_1
			if (_granted != null) {
				sb.Append (Environment.NewLine);
				sb.Append ("Granted: ");
				sb.Append (_granted.ToString ());
			}
			if (_refused != null) {
				sb.Append (Environment.NewLine);
				sb.Append ("Refused: ");
				sb.Append (_refused.ToString ());
			}
#endif
			return sb.ToString ();
		}
	}
}
