//
// System.ComponentModel.EventDescriptor.cs
//
// Authors:
//  Rodrigo Moya (rodrigo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc. 2002
// (C) 2003 Andreas Nahr
//

using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public abstract class EventDescriptor : MemberDescriptor
	{

		protected EventDescriptor (MemberDescriptor desc) : base (desc)
		{
		}

		protected EventDescriptor (MemberDescriptor desc, Attribute[] attrs) : base (desc, attrs)
		{
		}

		protected EventDescriptor (string str, Attribute[] attrs) : base (str, attrs)
		{
		}

		public abstract void AddEventHandler (object component, System.Delegate value);

		public abstract void RemoveEventHandler(object component, System.Delegate value);

		public abstract System.Type ComponentType { get; }

		public abstract System.Type EventType { get; }

		public abstract bool IsMulticast { get; }
	}
}
