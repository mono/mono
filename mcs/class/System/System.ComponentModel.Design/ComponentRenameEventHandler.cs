//
// System.ComponentModel.Design.ComponentRenameEventHandler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[Serializable]
	[ComVisible(true)]
        public delegate void ComponentRenameEventHandler (object sender,
							  ComponentRenameEventArgs e);
}
