// System.ComponentModel.Design.Serialization.IDesignerSerializationService.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerSerializationService
	{
		ICollection Deserialize (object serializationData);

		object Serialize (ICollection objects);
	}
}
