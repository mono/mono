//
// System.ComponentModel.EventDescriptor.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc. 2002
//

namespace System.ComponentModel
{
	public abstract class EventDescriptor : MemberDescriptor
	{
		protected EventDescriptor (MemberDescriptor desc) : base (desc)
		{
		}

		protected EventDescriptor (MemberDescriptor desc, Attribute[] attrs) : base (desc, attrs)
		{
		}

		protected EventDescriptor(string str, Attribute[] attrs) : base (str, attrs)
		{
		}
	}
}
