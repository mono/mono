//
// System.ComponentModel.UInt32Converter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class UInt32Converter : BaseNumberConverter
	{
		public UInt32Converter()
		{
			InnerType = typeof (UInt32);
		}
	}
}
