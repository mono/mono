//
// System.ComponentModel.Design.ComponentEventArgs.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class ComponentEventArgs : EventArgs
	{
		IComponent icomp;

		public ComponentEventArgs (IComponent component)
		{
			icomp = component;
		}

		public virtual IComponent Component {
			get { return icomp; }
		}
	}
}
