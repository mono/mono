//
// System.ComponentModel.InstallerTypeAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public class InstallerTypeAttribute : Attribute
	{
		private Type installer;

		public InstallerTypeAttribute (string typeName)
		{
			// MS behavior
			this.installer = Type.GetType (typeName, false);
		}

		public InstallerTypeAttribute (Type installerType)
		{
			this.installer = installerType;
		}

		public virtual Type InstallerType {
			get { return installer; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is InstallerTypeAttribute))
				return false;
			if (obj == this)
				return true;
			return ((InstallerTypeAttribute) obj).InstallerType == installer;
		}

		public override int GetHashCode()
		{
			return installer.GetHashCode ();
		}
	}
}

