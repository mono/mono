//
// System.ComponentModel.Design.IInheritanceService.cs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.ComponentModel;

namespace System.ComponentModel.Design
{
	public interface IInheritanceService
	{
		void AddInheritedComponents (IComponent component, IContainer container);

		InheritanceAttribute GetInheritanceAttribute (IComponent component);
	}
}
