//
// System.ComponentModel.LicenseProviderAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
        public sealed class LicenseProviderAttribute : Attribute
	{
		[MonoTODO]
		public LicenseProviderAttribute()
		{
		}

		[MonoTODO]
		public LicenseProviderAttribute (string typeName)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public LicenseProviderAttribute (Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public Type LicenseProvider {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}
		
		public override object TypeId {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~LicenseProviderAttribute()
		{
		}
	}
}
