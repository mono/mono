// System.ComponentModel.Design.IDesignerHost.cs
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

		IDesigner GetDesigner (IComponent component);

		Type GetType (string typeName);
		
		event EventHandler Activated;

		event EventHandler Deactivated;

		event EventHandler LoadComplete;

		event DesignerTransactionCloseEventHandler TransactionClosed;

		event DesignerTransactionCloseEventHandler TransactionClosing;

		event EventHandler TransactionOpened;

		event EventHandler TransactionOpening;
	}
}
