//
// System.Runtime.InteropServices.DispatchWrapper.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	public sealed class DispatchWrapper
	{
		object wrappedObject;

		public DispatchWrapper (object obj)
		{
			Marshal.GetIDispatchForObject (obj);
			wrappedObject = obj;
		}

		public object WrappedObject {
			get { return wrappedObject; }
		}
	}
}
