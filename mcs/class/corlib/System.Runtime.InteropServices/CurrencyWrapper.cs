//
// System.Runtime.InteropServices.CurrencyWrapper.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	public sealed class CurrencyWrapper
	{
		Decimal currency;

		public CurrencyWrapper (decimal obj)
		{
			currency = obj;
		}

		public CurrencyWrapper (object obj)
		{
			if (obj.GetType() != typeof(Decimal))
				throw new ArgumentException ("obj has to be a Decimal type");
			currency = (Decimal)obj;
		}

		public decimal WrappedObject {
			get { return currency; }
		}
	}
}
