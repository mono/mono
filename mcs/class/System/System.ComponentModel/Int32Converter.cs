//
// System.ComponentModel.Int32Converter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class Int32Converter : BaseNumberConverter
	{
		public Int32Converter()
		{
			InnerType = typeof (Int32);
		}
	}
}
