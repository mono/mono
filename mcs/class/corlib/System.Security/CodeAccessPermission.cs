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
	[ComVisible (true)]
	[MonoTODO ("CAS support is experimental (and unsupported).")]
	public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk {


		protected CodeAccessPermission ()
		{
		}

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public void Assert ()
		{
			new PermissionSet (this).Assert ();
		}

		public abstract IPermission Copy ();

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#endif
		public void Demand ()
		{
			// note: here we're sure it's a CAS demand
			if (!SecurityManager.SecurityEnabled)
				return;

			// skip frames until we get the caller (of our caller)
			new PermissionSet (this).CasOnlyDemand (3);
		}

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public void Deny ()
		{
			new PermissionSet (this).Deny ();
		}

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

		public abstract void FromXml (SecurityElement elem);

		[ComVisible (false)]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

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

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public void PermitOnly ()
		{
			new PermissionSet (this).PermitOnly ();
		}

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public static void RevertAll ()
		{
			if (!SecurityManager.SecurityEnabled)
				return;
			throw new NotImplementedException ();
		}

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public static void RevertAssert ()
		{
			if (!SecurityManager.SecurityEnabled)
				return;
			throw new NotImplementedException ();
		}

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public static void RevertDeny ()
		{
			if (!SecurityManager.SecurityEnabled)
				return;
			throw new NotImplementedException ();
		}

#if MOBILE
		[Conditional ("MONO_FEATURE_CAS")]
#else
		[MonoTODO ("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
#endif
		public static void RevertPermitOnly ()
		{
			if (!SecurityManager.SecurityEnabled)
				return;
			throw new NotImplementedException ();
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
				// unrestricted permissions are possible for identiy permissions
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

		internal static void ThrowInvalidPermission (IPermission target, Type expected) 
		{
			string msg = Locale.GetText ("Invalid permission type '{0}', expected type '{1}'.");
			msg = String.Format (msg, target.GetType (), expected);
			throw new ArgumentException (msg, "target");
		}

#if MOBILE
		// Workaround for CS0629
		void IStackWalk.Assert ()
		{
		}

		void IStackWalk.Deny ()
		{
		}

		void IStackWalk.PermitOnly ()
		{
		}

		void IStackWalk.Demand ()
		{
		}

		void IPermission.Demand ()
		{
		}
#endif
	}
}
