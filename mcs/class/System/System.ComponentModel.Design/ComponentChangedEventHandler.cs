//
// System.ComponentModel.Design.ComponentChangedEventHandler
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
        public delegate void ComponentChangedEventHandler (object sender,
							   ComponentChangedEventArgs e);
}
