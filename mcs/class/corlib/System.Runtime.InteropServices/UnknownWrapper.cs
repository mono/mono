//
// System.Runtime.InteropServices.UnknownWrapper.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	public sealed class UnknownWrapper
	{
		private object InternalObject;

		public UnknownWrapper (object obj)
		{
			InternalObject = obj;
		}

		public object WrappedObject {
			get { return InternalObject; } 
		}
	}
}
