//
// System.ComponentModel.Design.IComponentChangeService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
        public interface IComponentChangeService
	{
		void OnComponentChanged (object component,
					 MemberDescriptor member,
					 object oldValue,
					 object newValue);
		void OnComponentChanging (object component, 
					  MemberDescriptor member);
		event ComponentEventHandler ComponentAdded;
		event ComponentEventHandler ComponentAdding;
		event ComponentChangedEventHandler ComponentChanged;
		event ComponentChangingEventHandler ComponentChanging;
		event ComponentEventHandler ComponentRemoved;
		event ComponentEventHandler ComponentRemoving;
		event ComponentRenameEventHandler ComponentRename;
	}
}
