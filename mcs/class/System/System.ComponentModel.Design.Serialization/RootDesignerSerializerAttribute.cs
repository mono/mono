// System.ComponentModel.Design.Serialization.RootDesignerSerializerAttribute.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

namespace System.ComponentModel.Design.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class RootDesignerSerializerAttribute : Attribute
	{
		private string serializer;
		private string baseserializer;
		private Type basetypeserializer;
		private Type serializertype;
		private bool reload;
		
		public RootDesignerSerializerAttribute (string serializerTypeName, string baseSerializerTypeName, bool reloadable) {
			this.serializer = serializerTypeName;
			this.baseserializer = baseSerializerTypeName;
			this.reload = reloadable;
		}

		public RootDesignerSerializerAttribute (string serializerTypeName, Type baseSerializerType, bool reloadable) {
			this.serializer = serializerTypeName;
			this.basetypeserializer = baseSerializerType;
			this.reload = reloadable;
		}

		public RootDesignerSerializerAttribute (Type serializerType, Type baseSerializerType, bool reloadable) {
			this.serializertype = serializerType;
			this.basetypeserializer = baseSerializerType;
			this.reload = reloadable;
		}

		public bool Reloadable {
			get {
				return this.reload;
			}
			
			set {
				this.reload = value;
			}
		}

		public string SerializerBaseTypeName {
			get {
				return this.baseserializer;
			}

			set {
				this.baseserializer = value;
			}
		}

		public string SerializerTypeName {
			get {
				return this.serializer;
			}
			
			set {
				serializer = value;
			}
		}

		[MonoTODO]
		public override object TypeId {
			get { throw new NotImplementedException ();}
		}

		[MonoTODO]
		public override int GetHashCode() 
		{
			throw new NotImplementedException();
		}
	}
}
