//
// System.ComponentModel.Design.Serialization.RootDesignerSerializerAttribute.cs
//
// Authors:
//   Alejandro Sánchez Acosta (raciel@gnome.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel.Design.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class RootDesignerSerializerAttribute : Attribute
	{
		private string serializer;
		private string baseserializer;
		private bool reload;
		
		public RootDesignerSerializerAttribute (string serializerTypeName, string baseSerializerTypeName, bool reloadable) {
			this.serializer = serializerTypeName;
			this.baseserializer = baseSerializerTypeName;
			this.reload = reloadable;
		}

		public RootDesignerSerializerAttribute (string serializerTypeName, Type baseSerializerType, bool reloadable)
			: this (serializerTypeName, baseSerializerType.AssemblyQualifiedName, reloadable)
		{
		}

		public RootDesignerSerializerAttribute (Type serializerType, Type baseSerializerType, bool reloadable) 
			: this (serializerType.AssemblyQualifiedName, baseSerializerType.AssemblyQualifiedName, reloadable)
		{
		}

		public bool Reloadable {
			get {
				return this.reload;
			}
		}

		public string SerializerBaseTypeName {
			get {
				return this.baseserializer;
			}
		}

		public string SerializerTypeName {
			get {
				return this.serializer;
			}
		}

		public override object TypeId {
			get { return string.Concat (this.ToString(), baseserializer);}
		}
	}
}
