// System.ComponentModel.Design.Serialization.INameCreationService.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.Collections;
using System.Web.UI.Design;

namespace System.ComponentModel.Design.Serialization
{
	public interface INameCreationService
	{
		string CreateName (IContainer container, Type dataType);

		bool IsValidName (string name);

		void ValidateName (string name);
	}
}
