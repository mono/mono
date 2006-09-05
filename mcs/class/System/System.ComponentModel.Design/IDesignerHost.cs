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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
