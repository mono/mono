//
// System.Runtime.InteropServices.ErrorWrapper.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	public sealed class ErrorWrapper
	{
		int errorCode;

		public ErrorWrapper (Exception e)
		{
			this.errorCode = Marshal.GetHRForException (e);
		}

		public ErrorWrapper (int errorCode)
		{
			this.errorCode = errorCode;
		}

		public ErrorWrapper (object errorCode)
		{
			if (errorCode.GetType() != typeof(int))
				throw new ArgumentException ("errorCode has to be an int type");
			this.errorCode = (int)errorCode;
		}

		public int ErrorCode {
			get { return errorCode; }
		}
	}
}
