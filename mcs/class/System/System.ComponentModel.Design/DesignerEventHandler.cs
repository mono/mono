//
// System.ComponentModel.Design.DesignerEventHandler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	[Serializable]
        public delegate void DesignerEventHandler (object sender,
						   DesignerEventArgs e);
}
