//
// System.ComponentModel.Design.ComponentChangedEventArgs
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
        public sealed class ComponentChangedEventArgs : EventArgs
	{
		[MonoTODO]
		public ComponentChangedEventArgs (object component,
						  MemberDescriptor member,
						  object oldValue,
						  object newValue)
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

		public object NewValue {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public object OldValue {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		~ComponentChangedEventArgs()
		{
		}
	}
}
