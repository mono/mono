//
// System.ComponentModel.Design.CheckoutException.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	public class CheckoutException : ExternalException
	{
		public static readonly CheckoutException Canceled = new CheckoutException ();

		public CheckoutException()
			: this (null, 0)
		{
		}

		public CheckoutException (string message)
			: this (message, 0)
		{
		}

		public CheckoutException (string message, int errorCode)
			: base (message, errorCode)
		{
		}
	}
}
