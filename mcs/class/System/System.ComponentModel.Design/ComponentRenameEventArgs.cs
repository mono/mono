//
// System.ComponentModel.Design.ComponentRenameEventArgs
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
        public class ComponentRenameEventArgs : EventArgs
	{
		[MonoTODO]
		public ComponentRenameEventArgs (object component,
						 string oldName,
						 string newName)
		{
		}

		public object Component {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public virtual string NewName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public virtual string OldName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		~ComponentRenameEventArgs()
		{
		}
		
	}
}
