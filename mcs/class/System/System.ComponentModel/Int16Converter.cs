//
// System.ComponentModel.Int16Converter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class Int16Converter : BaseNumberConverter
	{
		public Int16Converter ()
		{
			InnerType = typeof (Int16);
		}
	}
}
