//
// System.ComponentModel.Design.ComponentRenameEventArgs.cs
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
	public class ComponentRenameEventArgs : EventArgs
	{
		private object component;
		private string oldName;
		private string newName;

		public ComponentRenameEventArgs (object component,
			string oldName, string newName)
		{
			this.component = component;
			this.oldName = oldName;
			this.newName = newName;
		}

		public object Component {
			get { return component; }
		}

		public virtual string NewName {
			get { return newName; }
		}

		public virtual string OldName {
			get { return oldName; }
		}
	}
}
