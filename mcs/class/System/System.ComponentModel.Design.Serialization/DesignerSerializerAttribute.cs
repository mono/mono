//
// System.ComponentModel.Design.Serialization.DesignerSerializerAttribute.cs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design.Serialization
{
	[AttributeUsage(AttributeTargets.Class |
			AttributeTargets.Interface)]
        public sealed class DesignerSerializerAttribute : Attribute
	{
		[MonoTODO]
		public DesignerSerializerAttribute (string serializerTypeName,
						    string baseSerializerTypeName)
		{
		}

		[MonoTODO]
		public DesignerSerializerAttribute (string serializerTypeName,
						    Type baseSerializerType)
		{
		}

		[MonoTODO]
		public DesignerSerializerAttribute (Type serializerType,
						    Type baseSerializerType)
		{
		}

		public string SerializerBaseTypeName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public string SerializerTypeName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public override object TypeId {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}
	}
}
