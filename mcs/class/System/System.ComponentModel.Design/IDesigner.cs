// System.ComponentModel.Design.IDesigner.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public interface IDesigner : IDisposable
	{
		IComponent Component {get;}

		DesignerVerbCollection Verbs {get;}

		void DoDefaultAction ();

		void Initialize (IComponent component);		
	}
}
