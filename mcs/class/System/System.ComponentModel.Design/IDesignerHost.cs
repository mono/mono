//
// System.ComponentModel.Design.IDesignerHost.cs
//
// Authors:
//   Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System.ComponentModel;
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

		void DestroyComponent (IComponent component);

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
