// System.ComponentModel.Design.Serialization.ResolvedNameEventHandler.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.Web.UI.Design;

namespace System.ComponentModel.Design.Serialization
{
	[Serializable]
	public delegate void ResolveNameEventHandler (object sender, ResolveNameEventArgs e);
}
