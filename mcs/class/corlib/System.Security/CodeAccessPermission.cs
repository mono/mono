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
using System.Security.Permissions;
using System.Text;

namespace System.Security {

	[Serializable]
	public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk {

		protected CodeAccessPermission ()
		{
		}

		// LAMESPEC: Documented as virtual
		[MonoTODO]
		public void Assert ()
		{
			// Not everyone can assert freely so we must check for
			// System.Security.Permissions.SecurityPermissionFlag.Assertion
			new SecurityPermission (SecurityPermissionFlag.Assertion).Demand ();

			// TODO: Only one Assert can be active in a stack frame
			// throw new SecurityException (Locale.GetText (
			//	"Only one Assert can be active in a stack frame"));
		}

#if NET_2_0
		public 
#else
		internal
#endif
		virtual bool CheckAssert (CodeAccessPermission asserted)
		{
			if (asserted == null)
				return false;
			if (asserted.GetType() != this.GetType ())
				return false;
			return IsSubsetOf (asserted);
		}

#if NET_2_0
		public 
#else
		internal
#endif
		virtual bool CheckDemand (CodeAccessPermission target)
		{
			if (target == null)
				return false;
			if (target.GetType () != this.GetType ())
				return false;
			return IsSubsetOf (target);
		}

#if NET_2_0
		public 
#else
		internal
#endif
		virtual bool CheckDeny (CodeAccessPermission denied)
		{
			if (denied == null)
				return true;
			if (denied.GetType () != this.GetType ())
				return true;
			return (Intersect (denied) == null);
		}

#if NET_2_0
		public 
#else
		internal
#endif
		virtual bool CheckPermitOnly (CodeAccessPermission target)
		{
			if (target == null)
				return false;
			if (target.GetType () != this.GetType ())
				return false;
			return IsSubsetOf (target);
		}

		public abstract IPermission Copy ();

		// LAMESPEC: Documented as virtual
		[MonoTODO ("MS centralize demands for IBuiltInPermission, but I think we should branch back into indivual permission classes.")]
		public void Demand ()
		{
			IBuiltInPermission perm = (this as IBuiltInPermission);
			if (perm == null)
				return; // not sure about this :(

			bool result = false;

			// TODO : Loop the stack
			// TODO : Loop all permission on the current frame
			result = CheckDemand (this);
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

			if (!result) {
				throw new SecurityException (Locale.GetText (
					"Demand failed."));
			}
		}

		// LAMESPEC: Documented as virtual
		[MonoTODO]
		public void Deny ()
		{
		}

#if NET_2_0
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (obj.GetType () != this.GetType ())
				return false;
			// TODO: compare
			return true;
		}
#endif

		public abstract void FromXml (SecurityElement elem);

#if NET_2_0
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

		// LAMESPEC: Documented as virtual
		[MonoTODO]
		public void PermitOnly ()
		{
		}

		[MonoTODO]
		public static void RevertAll ()
		{
		}

		[MonoTODO]
		public static void RevertAssert ()
		{
		}

		[MonoTODO]
		public static void RevertDeny ()
		{
		}

		[MonoTODO]
		public static void RevertPermitOnly ()
		{
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
