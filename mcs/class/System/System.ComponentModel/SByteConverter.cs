//
// System.ComponentModel.SByteConverter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class SByteConverter : BaseNumberConverter
	{
		public SByteConverter()
		{
			InnerType = typeof (SByte);
		}
	}
}
