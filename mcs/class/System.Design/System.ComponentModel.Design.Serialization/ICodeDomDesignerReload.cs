// System.ComponentModel.Design.Serialization.ICodeDomDesignerReload.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	public interface ICodeDomDesignerReload
	{
		bool ShouldReloadDesigner (CodeCompileUnit newTree);
	}
}
