//
// System.ComponentModel.Design.DesignerEventArgs.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel.Design
{
	public class DesignerEventArgs : EventArgs
	{

		private IDesignerHost host;

		public DesignerEventArgs (IDesignerHost host)
		{
			this.host = host;
		}

		public IDesignerHost Designer {
			get { return host; }
		}
	}
}
