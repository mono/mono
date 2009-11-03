//
// System.Runtime.InteropServices.DefaultCharSetAttribute
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	[AttributeUsage (AttributeTargets.Module, Inherited = false)]
	[ComVisible (true)]
	public sealed class DefaultCharSetAttribute : Attribute
	{
		CharSet _set;

		public DefaultCharSetAttribute (CharSet charSet)
		{
			_set = charSet;
		}

		public CharSet CharSet { get { return _set; } }
	}
}
