//
// System.ComponentModel.Design.CheckoutException
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	public class CheckoutException : ExternalException
	{
		public static readonly CheckoutException Canceled;

		[MonoTODO]
		public CheckoutException()
		{
		}

		[MonoTODO]
		public CheckoutException (string message)
		{
		}

		[MonoTODO]
		public CheckoutException (string message, int errorCode)
		{
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~CheckoutException()
		{
		}
	}
}
