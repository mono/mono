//
// System.ComponentModel.Design.IEventBindingService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;
using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
        public interface IEventBindingService
	{
		string CreateUniqueMethodName (IComponent component,
					       EventDescriptor e);
		ICollection GetCompatibleMethods (EventDescriptor e);
		EventDescriptor GetEvent (PropertyDescriptor property);
		PropertyDescriptorCollection GetEventProperties (
                                             EventDescriptorCollection events);
		PropertyDescriptor GetEventProperty (EventDescriptor e);
		bool ShowCode();
		bool ShowCode (int lineNumber);
		bool ShowCode (IComponent component, EventDescriptor e);
	}
}
