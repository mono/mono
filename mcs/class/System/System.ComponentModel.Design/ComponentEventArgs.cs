//
// System.ComponentModel.Design.ComponentEventArgs
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
        public class ComponentEventArgs : EventArgs
	{
		[MonoTODO]
		public ComponentEventArgs (IComponent component)
		{
		}

		public virtual IComponent Component {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		ComponentEventArgs()
		{
		}
	}
}
