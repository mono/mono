//
// System.ComponentModel.UInt16Converter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class UInt16Converter : BaseNumberConverter
	{
		public UInt16Converter()
		{
			InnerType = typeof (UInt16);
		}
	}
}
