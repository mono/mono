//
// System.ComponentModel.UInt64Converter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class UInt64Converter : BaseNumberConverter
	{
		public UInt64Converter()
		{
			InnerType = typeof (UInt64);
		}
	}
}
