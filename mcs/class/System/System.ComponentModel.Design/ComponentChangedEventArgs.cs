//
// System.ComponentModel.Design.ComponentChangedEventArgs.cs
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
	public sealed class ComponentChangedEventArgs : EventArgs
	{

		private object component;
		private MemberDescriptor member;
		private object oldValue;
		private object newValue;

		public ComponentChangedEventArgs (object component,
			MemberDescriptor member, object oldValue, object newValue)
		{
			this.component = component;
			this.member = member;
			this.oldValue = oldValue;
			this.newValue = newValue;
		}

		public object Component {
			get { return component; }
		}

		public MemberDescriptor Member {
			get { return member; }
		}

		public object NewValue {
			get { return oldValue; }
		}

		public object OldValue {
			get { return newValue; }
		}
	}
}
