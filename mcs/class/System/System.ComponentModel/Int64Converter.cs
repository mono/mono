//
// System.ComponentModel.Int64Converter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class Int64Converter : BaseNumberConverter
	{
		public Int64Converter()
		{
			InnerType = typeof (Int64);
		}
	}
}
