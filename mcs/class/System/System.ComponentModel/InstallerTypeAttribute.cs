//
// System.ComponentModel.InstallerTypeAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
        public class InstallerTypeAttribute : Attribute
	{
		[MonoTODO]
		public InstallerTypeAttribute (string typeName)
		{
		}

		[MonoTODO]
		public InstallerTypeAttribute (Type installerType)
		{
		}

		public virtual Type InstallerType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~InstallerTypeAttribute()
		{
		}
	}
}
