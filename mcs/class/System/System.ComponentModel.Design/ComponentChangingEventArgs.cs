//
// System.ComponentModel.Design.ComponentChangingEventArgs.cs
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
	public sealed class ComponentChangingEventArgs : EventArgs
	{
		private object component;
		private MemberDescriptor member;

		public ComponentChangingEventArgs (object component,
						   MemberDescriptor member)
		{
			this.component = component;
			this.member = member;
		}

		public object Component 
		{
			get { return component; }
		}

		public MemberDescriptor Member 
		{
			get { return member; }
		}
	}
}
