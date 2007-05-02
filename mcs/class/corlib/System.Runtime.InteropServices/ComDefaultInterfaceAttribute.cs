//
// System.Runtime.InteropServices.ComDefaultInterfaceAttribute
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;

namespace System.Runtime.InteropServices
{
	[AttributeUsage (AttributeTargets.Class, Inherited = false)]
	[ComVisible (true)]
	public sealed class ComDefaultInterfaceAttribute : Attribute
	{
		Type _type;

		public ComDefaultInterfaceAttribute (Type defaultInterface)
		{
			_type = defaultInterface;
		}

		public Type Value { get { return _type; }}
	}
}
#endif
