// System.ComponentModel.Design.Serialization.IDesignerSerializationProvider.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerSerializationProvider
	{
		object GetSerializer (IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType);
	}
}
