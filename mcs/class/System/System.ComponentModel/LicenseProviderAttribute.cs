//
// System.ComponentModel.LicenseProviderAttribute
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
	public sealed class LicenseProviderAttribute : Attribute
	{
		private Type Provider;

		public static readonly LicenseProviderAttribute Default = new LicenseProviderAttribute ();

		public LicenseProviderAttribute()
		{
			this.Provider = null;
		}

		public LicenseProviderAttribute (string typeName)
		{
			this.Provider = Type.GetType (typeName, false);
		}

		public LicenseProviderAttribute (Type type)
		{
			this.Provider = type;
		}

		public Type LicenseProvider {
			get { return Provider; }
		}

		public override object TypeId {
		get {
			// Seems to be MS implementation
			return (base.ToString() + Provider.ToString());
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is LicenseProviderAttribute))
				return false;
			if (obj == this)
				return true;
			return ((LicenseProviderAttribute) obj).LicenseProvider == Provider;
		}

		public override int GetHashCode()
		{
			return Provider.GetHashCode ();
		}
	}
}

