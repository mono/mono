//
// System.Runtime.InteropServices.VariantWrapper.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell, Inc.  http://www.ximian.com
//

#if NET_2_0

namespace System.Runtime.InteropServices
{
	public sealed class VariantWrapper
	{
		private object _wrappedObject;

		public VariantWrapper (object obj)
		{
			_wrappedObject = obj;
		}

		public object WrappedObject
		{
			get
			{
				return _wrappedObject;
			}
		}
	}
}

#endif
