//
// System.Runtime.InteropServices.ComEventInterfaceAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Interface)]
	public sealed class ComEventInterfaceAttribute : Attribute
	{
		Type si, ep;
		
		public ComEventInterfaceAttribute (Type SourceInterface,
						   Type EventProvider)
		{
			si = SourceInterface;
			ep = EventProvider;
		}

		public Type EventProvider {
			get { return ep; }
		}

		public Type SourceInterface {
			get { return si; }
		}
	}
}
		
