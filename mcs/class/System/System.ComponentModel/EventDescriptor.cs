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
		[MonoTODO]
		protected EventDescriptor (MemberDescriptor desc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected EventDescriptor (MemberDescriptor desc, Attribute[] attrs)
		{
			throw new NotImplementedException ();
		}

		protected EventDescriptor(string str, Attribute[] attrs)
		{
		}
	}
}
