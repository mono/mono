//
// System.ComponentModel.DoubleConverter.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class DoubleConverter : BaseNumberConverter
	{
		public DoubleConverter()
		{
			InnerType = typeof (Double);
		}
	}
}
