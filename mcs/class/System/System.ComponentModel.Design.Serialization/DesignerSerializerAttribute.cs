//
// System.ComponentModel.Design.Serialization.DesignerSerializerAttribute.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel.Design.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class DesignerSerializerAttribute : Attribute
	{

		private string serializerTypeName;
		private string baseSerializerTypeName;

		public DesignerSerializerAttribute (string serializerTypeName,
			string baseSerializerTypeName)
		{
			this.serializerTypeName = serializerTypeName;
			this.baseSerializerTypeName = baseSerializerTypeName;
		}

		public DesignerSerializerAttribute (string serializerTypeName, Type baseSerializerType)
			: this (serializerTypeName, baseSerializerType.AssemblyQualifiedName)
		{
		}

		public DesignerSerializerAttribute (Type serializerType, Type baseSerializerType)
			: this (serializerType.AssemblyQualifiedName, baseSerializerType.AssemblyQualifiedName)
		{
		}

		public string SerializerBaseTypeName {
			get { return baseSerializerTypeName; }
		}

		public string SerializerTypeName {
			get { return serializerTypeName; }
		}

		public override object TypeId {
			get { return string.Concat (this.ToString(), baseSerializerTypeName); }
		}
	}
}
