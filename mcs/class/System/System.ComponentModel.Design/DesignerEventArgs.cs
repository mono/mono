//
// System.ComponentModel.Design.DesignerEventArgs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public class DesignerEventArgs : EventArgs
	{
		[MonoTODO]
		public DesignerEventArgs (IDesignerHost host)
		{
		}

		public IDesignerHost Designer {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		~DesignerEventArgs()
		{
		}

	}
}
