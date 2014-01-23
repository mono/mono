//
// System.Security.SecurityException.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

namespace System.Security {

	[Serializable]
 	[ComVisible (true)]
	public class SecurityException : SystemException {
		// Fields
		string permissionState;
		Type permissionType;
		private string _granted;
		private string _refused;
		private object _demanded;
		private IPermission _firstperm;
//		private IPermission _permfailed;
		private MethodInfo _method;
#if !MOBILE
		private Evidence _evidence;
#endif
		private SecurityAction _action;
		private object _denyset;
		private object _permitset;
		private AssemblyName _assembly;
		private string _url;
		private SecurityZone _zone;
		
		// Properties

		[ComVisible (false)]
		public SecurityAction Action {
			get { return _action; }
			set { _action = value; }
		}

		[ComVisible (false)]
		public object DenySetInstance {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _denyset; }
			set { _denyset = value; }
		}

		[ComVisible (false)]
		public AssemblyName FailedAssemblyInfo {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _assembly; }
			set { _assembly = value; }
		}

		[ComVisible (false)]
		public MethodInfo Method {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _method; }
			set { _method = value; }
		}

		[ComVisible (false)]
		public object PermitOnlySetInstance {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _permitset; }
			set { _permitset = value; }
		}

		public string Url {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _url; }
			set { _url = value; }
		}

		public SecurityZone Zone {
			get { return _zone; }
			set { _zone = value; }
		}

		[ComVisible (false)]
		public object Demanded {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _demanded; }
			set { _demanded = value; }
		}

		public IPermission FirstPermissionThatFailed {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _firstperm; }
			set { _firstperm = value; }
		}

		public string PermissionState {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return permissionState; }
			set { permissionState = value; }
		}

		public Type PermissionType {
			get { return permissionType; }
			set { permissionType = value; }
		}

		public string GrantedSet {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _granted; }
			set { _granted = value; }
		}

		public string RefusedSet {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
			get { return _refused; }
			set { _refused = value; }
		}

		// Constructors

		public SecurityException ()
			: this (Locale.GetText ("A security error has been detected."))
		{
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
			// depending on the security policy the info about PermissionState may
			// not be available (but the serializable must work)
			SerializationInfoEnumerator e = info.GetEnumerator ();
			while (e.MoveNext ()) {
				if (e.Name == "PermissionState") {
					permissionState = (string) e.Value;
					break;
				}
			}
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

		public SecurityException (string message, object deny, object permitOnly, MethodInfo method, 
			object demanded, IPermission permThatFailed)
			: base (message)
		{
			base.HResult = unchecked ((int)0x8013150A);
			_denyset = deny;
			_permitset = permitOnly;
			_method = method;
			_demanded = demanded;
			_firstperm = permThatFailed;
		}
#if !MOBILE
		public SecurityException (string message, AssemblyName assemblyName, PermissionSet grant, 
			PermissionSet refused, MethodInfo method, SecurityAction action, object demanded, 
			IPermission permThatFailed, Evidence evidence)
			: base (message)
		{
			base.HResult = unchecked ((int)0x8013150A);
			_assembly = assemblyName;
			_granted = (grant == null) ? String.Empty : grant.ToString ();
			_refused = (refused == null) ? String.Empty : refused.ToString ();
			_method = method;
			_action = action;
			_demanded = demanded;
			_firstperm = permThatFailed;
			if (_firstperm != null)
				permissionType = _firstperm.GetType ();
			_evidence = evidence;
		}
#endif
		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			try {
				info.AddValue ("PermissionState", permissionState);
			}
			catch (SecurityException) {
				// serialize only if permitted to do so
			}
		}

		public override string ToString ()
		{
#if MOBILE
			return base.ToString ();
#else
			StringBuilder sb = new StringBuilder (base.ToString ());
			try {
				if (permissionType != null) {
					sb.AppendFormat ("{0}Type: {1}", Environment.NewLine, PermissionType);
				}
				if (_method != null) {
					// method string representation doesn't include the type
					string m = _method.ToString ();
					int ret = m.IndexOf (" ") + 1;
					sb.AppendFormat ("{0}Method: {1} {2}.{3}", Environment.NewLine, 
						_method.ReturnType.Name, _method.ReflectedType, m.Substring (ret));
				}
				if (permissionState != null) {
					sb.AppendFormat ("{0}State: {1}", Environment.NewLine, PermissionState);
				}
				if ((_granted != null) && (_granted.Length > 0)) {
					sb.AppendFormat ("{0}Granted: {1}", Environment.NewLine, GrantedSet);
				}
				if ((_refused != null) && (_refused.Length > 0)) {
					sb.AppendFormat ("{0}Refused: {1}", Environment.NewLine, RefusedSet);
				}
				if (_demanded != null) {
					sb.AppendFormat ("{0}Demanded: {1}", Environment.NewLine, Demanded);
				}
				if (_firstperm != null) {
					sb.AppendFormat ("{0}Failed Permission: {1}", Environment.NewLine, FirstPermissionThatFailed);
				}
				if (_evidence != null) {
					sb.AppendFormat ("{0}Evidences:", Environment.NewLine);
					foreach (object o in _evidence) {
						// Hash evidence is way much too verbose to be useful to anyone
						if (!(o is Hash))
							sb.AppendFormat ("{0}\t{1}", Environment.NewLine, o);
					}
				}
			}
			catch (SecurityException) {
				// some informations can't be displayed
			}
			return sb.ToString ();
#endif
		}
	}
}
