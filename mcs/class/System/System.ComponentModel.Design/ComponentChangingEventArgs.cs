//
// System.ComponentModel.Design.ComponentChangingEventArgs
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
        public sealed class ComponentChangingEventArgs : EventArgs
	{
		[MonoTODO]
		public ComponentChangingEventArgs (object component,
						   MemberDescriptor member)
		{
		}

		public object Component {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public MemberDescriptor Member {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		~ComponentChangingEventArgs()
		{
		}
	}
}
