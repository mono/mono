//
// System.ComponentModel.SingleConverter
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class SingleConverter : BaseNumberConverter
	{
		public SingleConverter()
		{
			InnerType = typeof (Single);
		}
	}
}
