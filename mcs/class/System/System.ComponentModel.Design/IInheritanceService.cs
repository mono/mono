//
// System.ComponentModel.Design.IInheritanceService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
        public interface IInheritanceService
	{
		void AddInheritedComponents (IComponent component,
					     IContainer container);

		InheritanceAttribute GetInheritanceAttribute (IComponent component);
	}
}
