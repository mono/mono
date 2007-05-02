//
// System.Runtime.InteropServices.BStrWrapper
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;

namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible (true)]
	public sealed class BStrWrapper
	{
		string _value;

		public BStrWrapper (string value)
		{
			_value = value;
		}

		public string WrappedObject { get { return _value; } }
	}
}
#endif
