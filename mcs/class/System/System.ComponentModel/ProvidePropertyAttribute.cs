//
// System.ComponentModel.ProvidePropertyAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
        public sealed class ProvidePropertyAttribute : Attribute
	{
		[MonoTODO]
		public ProvidePropertyAttribute (string propertyName,
						 string receiverTypeName)
		{
		}

		[MonoTODO]
		public ProvidePropertyAttribute (string propertyName, 
						 Type receiverType)
		{
		}

		public string PropertyName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public string ReceiverTypeName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public override object TypeId {
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
		~ProvidePropertyAttribute()
		{
		}
	}
}
