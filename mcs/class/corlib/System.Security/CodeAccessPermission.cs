//
// System.Security.CodeAccessPermission.cs
//
// Authors:
//	Miguel de Icaza (miguel@ximian.com)
//	Nick Drochak, ndrochak@gol.com
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) Ximian, Inc. http://www.ximian.com
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System.Globalization;
using System.Security.Permissions;
using System.Text;

namespace System.Security {

	[Serializable]
	public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk {

		protected CodeAccessPermission () {}

		// LAMESPEC: Documented as virtual
		[MonoTODO("SecurityStackFrame not ready")]
		public void Assert ()
		{
			// throw a SecurityException if Assertion is denied
			new SecurityPermission (SecurityPermissionFlag.Assertion).Demand ();
//			SecurityStackFrame.Current.Assert = this;
		}

		public abstract IPermission Copy ();

		// LAMESPEC: Documented as virtual
		[MonoTODO("MS contralize demands, but I think we should branch back into indivual permission classes.")]
		public void Demand ()
		{
			IBuiltInPermission perm = (this as IBuiltInPermission);
			if (perm == null)
				throw new SecurityException (Locale.GetText ("Not a IBuiltInPermission and Demand isn't overridden"));

			// TODO : Loop the stack
			switch (perm.GetTokenIndex ()) {
				case 0: // EnvironmentPermission
					// TODO
					break;
				case 1: // FileDialogPermission
					// TODO
					break;
				case 2: // FileIOPermission
					// TODO
					break;
				case 3: // IsolatedStorageFilePermission
					// TODO
					break;
				case 4: // ReflectionPermission
					// TODO
					break;
				case 5: // RegistryPermission
					// TODO
					break;
				case 6: // SecurityPermission
					// TODO
					break;
				case 7: // UIPermission
					// TODO
					break;
				case 8: // PrincipalPermission
					// TODO
					break;
				case 9: // PublisherIdentityPermission
					// TODO
					break;
				case 10: // SiteIdentityPermission
					// TODO
					break;
				case 11: // StrongNameIdentityPermission
					// TODO
					break;
				case 12: // UrlIdentityPermission
					// TODO
					break;
				case 13: // ZoneIdentityPermission
					// TODO
					break;
				default:
					string message = String.Format (Locale.GetText ("Unknown IBuiltInPermission #{0}"), perm.GetTokenIndex ());
					throw new SecurityException (message);
			}
		}

		// LAMESPEC: Documented as virtual
		[MonoTODO("SecurityStackFrame not ready")]
		public void Deny ()
		{
//			SecurityStackFrame.Current.Deny = this;
		}

		public abstract void FromXml (SecurityElement elem);

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

		// LAMESPEC: Documented as virtual
		[MonoTODO("SecurityStackFrame not ready")]
		public void PermitOnly ()
		{
//			SecurityStackFrame.Current.PermitOnly = this;
		}

		[MonoTODO("SecurityStackFrame not ready")]
		public static void RevertAll ()
		{
//			SecurityStackFrame.Current.RevertAll ();
		}

		[MonoTODO("SecurityStackFrame not ready")]
		public static void RevertAssert () 
		{
//			SecurityStackFrame.Current.RevertAssert ();
		}

		[MonoTODO("SecurityStackFrame not ready")]
		public static void RevertDeny ()
		{
//			SecurityStackFrame.Current.RevertDeny ();
		}

		[MonoTODO("SecurityStackFrame not ready")]
		public static void RevertPermitOnly () 
		{
//			SecurityStackFrame.Current.RevertPermitOnly ();
		}

		// snippet moved from FileIOPermission (nickd) to be reused in all derived classes
		internal SecurityElement Element (object o, int version) 
		{
			SecurityElement se = new SecurityElement ("IPermission");
			Type type = this.GetType ();
			StringBuilder asmName = new StringBuilder (type.Assembly.ToString ());
			asmName.Replace ('\"', '\'');
			se.AddAttribute ("class", type.FullName + ", " + asmName);
			se.AddAttribute ("version", version.ToString ());
			return se;
		}
	}
}
