//
// System.Runtime.InteropServices.BStrWrapper
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

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
		
#if NET_4_0
		public BStrWrapper (object value)
		{
			_value = (string)value;
		}
#endif

		public string WrappedObject { get { return _value; } }
	}
}
