//
// System.Security.CodeAccessPermission.cs
//
// Authors:
//	Miguel de Icaza (miguel@ximian.com)
//	Nick Drochak, ndrochak@gol.com
//
// (C) Ximian, Inc. http://www.ximian.com
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
//

using System.Text;

namespace System.Security {

	[Serializable]
	public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk {

		protected CodeAccessPermission () {}

		[MonoTODO()]
		public void Assert () {}

		public abstract IPermission Copy ();

		[MonoTODO()]
		public void Demand () {}

		[MonoTODO()]
		public void Deny () {}

		public abstract void FromXml (SecurityElement elem);

		public abstract IPermission Intersect (IPermission target);

		public abstract bool IsSubsetOf (IPermission target);

		public override string ToString()
		{
			SecurityElement elem = ToXml ();
			return elem == null ? null : elem.ToString ();
		}

		public abstract SecurityElement ToXml ();

		[MonoTODO("Incomplete")]
		public virtual IPermission Union (IPermission other)
		{
			if (!(other is System.Security.CodeAccessPermission))
				throw new System.ArgumentException(); // other is not of type System.Security.CodeAccessPermission.
			if (null != other)
				throw new System.NotSupportedException(); // other is not null.
			return null;
		}

		[MonoTODO()]
		public void PermitOnly () {}

		[MonoTODO()]
		public static void RevertAll () {}

		[MonoTODO()]
		public static void RevertAssert () {}

		[MonoTODO()]
		public static void RevertDeny () {}

		[MonoTODO()]
		public static void RevertPermitOnly () {}

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
