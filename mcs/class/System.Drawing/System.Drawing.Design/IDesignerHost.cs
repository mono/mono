// System.Drawing.Design.IDesignerHost.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 	
// (C) Alejandro Sánchez Acosta
// 

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace System.Drawing.Design
{
	[ComVisible(true)]
	public interface IDesignerHost : IServiceContainer, IServiceProvider
	{
		IContainer Container {get;}

		bool InTransaction {get;}

		bool Loading {get;}

		IComponent RootComponent {get;}

		string RootComponentClassName {get;}

		string TransactionDescription {get;}

		void Activate();

		IComponent CreateComponent (Type componentClass);

		IComponent CreateComponent (Type componentClass, string name);

		DesignerTransaction CreateTransaction ();

		DesignerTransaction CreateTransaction (string description);

		void DestroyComponent (IComponent component);

		IDesigner GetDesigner (IComponent component);

		Type GetType (string typeName);		
	}
}

