//
// System.ComponentModel.ByteConverter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class ByteConverter : BaseNumberConverter
	{
		public ByteConverter()
		{
			InnerType = typeof (Byte);
		}
	}
}

