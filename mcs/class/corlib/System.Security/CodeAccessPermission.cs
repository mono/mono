//
// System.Security.CodeAccessPermission.cs
//
// Authors:
//	Miguel de Icaza (miguel@ximian.com)
//	Nick Drochak, ndrochak@gol.com
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc. http://www.ximian.com
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Security {

	[Serializable]
	[SecurityPermission (SecurityAction.InheritanceDemand, ControlEvidence = true, ControlPolicy = true)]
#if NET_2_0
	[ComVisible (true)]
#endif
	[MonoTODO ("CAS support is experimental (and unsupported).")]
	public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk {


		protected CodeAccessPermission ()
		{
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public void Assert ()
		{
			#if UNITY_DISABLE_CORECLR
			new PermissionSet (this).Assert ();
			#endif
		}

		internal bool CheckAssert (CodeAccessPermission asserted)
		{
			if (asserted == null)
				return false;
			if (asserted.GetType () != this.GetType ())
				return false;
			return IsSubsetOf (asserted);
		}

		internal bool CheckDemand (CodeAccessPermission target)
		{
			if (target == null)
				return false;
			if (target.GetType () != this.GetType ())
				return false;
			return IsSubsetOf (target);
		}

		internal bool CheckDeny (CodeAccessPermission denied)
		{
			if (denied == null)
				return true;
			Type t = denied.GetType ();
			if (t != this.GetType ())
				return true;
			IPermission inter = Intersect (denied);
			if (inter == null)
				return true;
			// sadly that's not enough :( at this stage we must also check
			// if an empty (PermissionState.None) is a subset of the denied
			// (which is like a empty intersection looks like for flag based
			// permissions, e.g. AspNetHostingPermission).
			return denied.IsSubsetOf (PermissionBuilder.Create (t));
		}

		internal bool CheckPermitOnly (CodeAccessPermission target)
		{
			if (target == null)
				return false;
			if (target.GetType () != this.GetType ())
				return false;
			return IsSubsetOf (target);
		}

		public abstract IPermission Copy ();

		public void Demand ()
		{
		// UNITY_DISABLE_CORECLR should never be defined ;)
		#if UNITY_DISABLE_CORECLR
			// note: here we're sure it's a CAS demand
			#if !DISABLE_SECURITY
			if (!SecurityManager.SecurityEnabled)
				return;

			// skip frames until we get the caller (of our caller)
			new PermissionSet (this).CasOnlyDemand (3);
			#endif
		#endif 
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public void Deny ()
		{
		#if UNITY_DISABLE_CORECLR
			new PermissionSet (this).Deny ();
		#endif
		}

#if NET_2_0
		[ComVisible (false)]
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (obj.GetType () != this.GetType ())
				return false;
			CodeAccessPermission cap = (obj as CodeAccessPermission);
			return (IsSubsetOf (cap) && cap.IsSubsetOf (this));
		}
#endif

		public abstract void FromXml (SecurityElement elem);

#if NET_2_0
		[ComVisible (false)]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
#endif

		public abstract IPermission Intersect (IPermission target);

		public abstract bool IsSubsetOf (IPermission target);

		public override string ToString ()
		{
			SecurityElement elem = ToXml ();
			return elem.ToString ();
		}

		public abstract SecurityElement ToXml ();

		public virtual IPermission Union (IPermission other)
		{
			if (null != other)
				throw new System.NotSupportedException (); // other is not null.
			return null;
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public void PermitOnly ()
		{
			#if UNITY_DISABLE_CORECLR
			new PermissionSet (this).PermitOnly ();
			#endif
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public static void RevertAll ()
		{
			#if UNITY_DISABLE_CORECLR
			#if !DISABLE_SECURITY
			if (!SecurityManager.SecurityEnabled)
				return;

			SecurityFrame sf = new SecurityFrame (1);
			bool revert = false;
			if ((sf.Assert != null) && !sf.Assert.DeclarativeSecurity) {
				revert = true;
				throw new NotSupportedException ("Currently only declarative Assert are supported.");
			}
			if ((sf.Deny != null) && !sf.Deny.DeclarativeSecurity) {
				revert = true;
				throw new NotSupportedException ("Currently only declarative Deny are supported.");
			}
			if ((sf.PermitOnly != null) && !sf.PermitOnly.DeclarativeSecurity) {
				revert = true;
				throw new NotSupportedException ("Currently only declarative PermitOnly are supported.");
			}

			if (!revert) {
				string msg = Locale.GetText ("No stack modifiers are present on the current stack frame.");
				// FIXME: we don't (yet) support imperative stack modifiers
				msg += Environment.NewLine + "Currently only declarative stack modifiers are supported.";
				throw new ExecutionEngineException (msg);
			}
			#endif
			#endif
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public static void RevertAssert ()
		{
			#if UNITY_DISABLE_CORECLR
			#if !DISABLE_SECURITY
			if (!SecurityManager.SecurityEnabled)
				return;

			SecurityFrame sf = new SecurityFrame (1);
			if ((sf.Assert != null) && !sf.Assert.DeclarativeSecurity) {
				throw new NotSupportedException ("Currently only declarative Assert are supported.");
			} else {
				// we can't revert declarative security (or an empty frame) imperatively
				ThrowExecutionEngineException (SecurityAction.Assert);
			}
			#endif
			#endif
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public static void RevertDeny ()
		{
			#if UNITY_DISABLE_CORECLR
			#if !DISABLE_SECURITY
			if (!SecurityManager.SecurityEnabled)
				return;

			SecurityFrame sf = new SecurityFrame (1);
			if ((sf.Deny != null) && !sf.Deny.DeclarativeSecurity) {
				throw new NotSupportedException ("Currently only declarative Deny are supported.");
			} else {
				// we can't revert declarative security (or an empty frame) imperatively
				ThrowExecutionEngineException (SecurityAction.Deny);
			}
			#endif
			#endif
		}

		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
		public static void RevertPermitOnly ()
		{
			#if UNITY_DISABLE_CORECLR
			#if !DISABLE_SECURITY
			if (!SecurityManager.SecurityEnabled)
				return;

			SecurityFrame sf = new SecurityFrame (1);
			if ((sf.PermitOnly != null) && sf.PermitOnly.DeclarativeSecurity) {
				throw new NotSupportedException ("Currently only declarative PermitOnly are supported.");
			} else {
				// we can't revert declarative security (or an empty frame) imperatively
				ThrowExecutionEngineException (SecurityAction.PermitOnly);
			}
			#endif
			#endif
		}

		// Internal helpers methods

		// snippet moved from FileIOPermission (nickd) to be reused in all derived classes
		internal SecurityElement Element (int version) 
		{
			SecurityElement se = new SecurityElement ("IPermission");
			Type type = this.GetType ();
			se.AddAttribute ("class", type.FullName + ", " + type.Assembly.ToString ().Replace ('\"', '\''));
			se.AddAttribute ("version", version.ToString ());
			return se;
		}

		internal static PermissionState CheckPermissionState (PermissionState state, bool allowUnrestricted)
		{
			string msg;
			switch (state) {
			case PermissionState.None:
				break;
			case PermissionState.Unrestricted:
#if NET_2_0
				// unrestricted permissions are possible for identiy permissions
#else
				if (!allowUnrestricted) {
					msg = Locale.GetText ("Unrestricted isn't not allowed for identity permissions.");
					throw new ArgumentException (msg, "state");
				}
#endif
				break;
			default:
				msg = String.Format (Locale.GetText ("Invalid enum {0}"), state);
				throw new ArgumentException (msg, "state");
			}
			return state;
		}

		internal static int CheckSecurityElement (SecurityElement se, string parameterName, int minimumVersion, int maximumVersion) 
		{
			if (se == null)
				throw new ArgumentNullException (parameterName);

			// Tag is case-sensitive
			if (se.Tag != "IPermission") {
				string msg = String.Format (Locale.GetText ("Invalid tag {0}"), se.Tag);
				throw new ArgumentException (msg, parameterName);
			}

			// Note: we do not care about the class attribute at 
			// this stage (in fact we don't even if the class 
			// attribute is present or not). Anyway the object has
			// already be created, with success, if we're loading it

			// we assume minimum version if no version number is supplied
			int version = minimumVersion;
			string v = se.Attribute ("version");
			if (v != null) {
				try {
					version = Int32.Parse (v);
				}
				catch (Exception e) {
					string msg = Locale.GetText ("Couldn't parse version from '{0}'.");
					msg = String.Format (msg, v);
					throw new ArgumentException (msg, parameterName, e);
				}
			}

			if ((version < minimumVersion) || (version > maximumVersion)) {
				string msg = Locale.GetText ("Unknown version '{0}', expected versions between ['{1}','{2}'].");
				msg = String.Format (msg, version, minimumVersion, maximumVersion);
				throw new ArgumentException (msg, parameterName);
			}
			return version;
		}

		// must be called after CheckSecurityElement (i.e. se != null)
		internal static bool IsUnrestricted (SecurityElement se) 
		{
			string value = se.Attribute ("Unrestricted");
			if (value == null)
				return false;
			return (String.Compare (value, Boolean.TrueString, true, CultureInfo.InvariantCulture) == 0);
		}

		internal bool ProcessFrame (SecurityFrame frame)
		{ 
			// 1. CheckPermitOnly
			if (frame.PermitOnly != null) {
				// the demanded permission must be in one of the permitted...
				bool permit = frame.PermitOnly.IsUnrestricted ();
				if (!permit) {
					// check individual permissions
					foreach (IPermission p in frame.PermitOnly) {
						if (CheckPermitOnly (p as CodeAccessPermission)) {
							permit = true;
							break;
						}
					}
				}
				if (!permit) {
					// ...or else we throw
					ThrowSecurityException (this, "PermitOnly", frame, SecurityAction.Demand, null);
				}
			}

			// 2. CheckDeny
			if (frame.Deny != null) {
				// special case where everything is denied (i.e. no child to be processed)
				if (frame.Deny.IsUnrestricted ())
					ThrowSecurityException (this, "Deny", frame, SecurityAction.Demand, null);
				foreach (IPermission p in frame.Deny) {
					if (!CheckDeny (p as CodeAccessPermission))
						ThrowSecurityException (this, "Deny", frame, SecurityAction.Demand, p);
				}
			}

			// 3. CheckAssert
			if (frame.Assert != null) {
				if (frame.Assert.IsUnrestricted ())
					return true; // remove permission and continue stack walk
				foreach (IPermission p in frame.Assert) {
					if (CheckAssert (p as CodeAccessPermission)) {
						return true; // remove permission and continue stack walk
					}
				}
			}

			// continue the stack walk
			return false; 
		}

		internal static void ThrowInvalidPermission (IPermission target, Type expected) 
		{
			string msg = Locale.GetText ("Invalid permission type '{0}', expected type '{1}'.");
			msg = String.Format (msg, target.GetType (), expected);
			throw new ArgumentException (msg, "target");
		}

		internal static void ThrowExecutionEngineException (SecurityAction stackmod)
		{
			string msg = Locale.GetText ("No {0} modifier is present on the current stack frame.");
			// FIXME: we don't (yet) support imperative stack modifiers
			msg += Environment.NewLine + "Currently only declarative stack modifiers are supported.";
			throw new ExecutionEngineException (String.Format (msg, stackmod));
		}

		internal static void ThrowSecurityException (object demanded, string message, SecurityFrame frame,
			SecurityAction action, IPermission failed)
		{
#if !DISABLE_SECURITY
#if NET_2_1
			throw new SecurityException (message);
#else
			
			Assembly a = frame.Assembly;
			throw new SecurityException (Locale.GetText (message), 
				a.UnprotectedGetName (), a.GrantedPermissionSet, 
				a.DeniedPermissionSet, frame.Method, action, demanded, 
				failed, a.UnprotectedGetEvidence ());
#endif
#else
			throw new SystemException(message);
#endif
		}
	}
}
