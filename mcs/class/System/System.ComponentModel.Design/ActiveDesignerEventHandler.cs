// System.ComponentModel.Design.ActiveDesignerEventHandler.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.Serialization;

namespace System.ComponentModel.Design
{
	[Serializable]
	public delegate void ActiveDesignerEventHandler (object sender, ActiveDesignerEventArgs e);
}
