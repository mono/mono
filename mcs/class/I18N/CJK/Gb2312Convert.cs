//
// I18N.CJK.Gb2312Convert
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

using System;

namespace I18N.CJK
{
	internal sealed class Gb2312Convert : DbcsConvert
	{
		// Dummy constructor, no one is aupposed to call it
		private Gb2312Convert() : base("gb2312.table") {}
		
		// The one and only GB2312 conversion object in the system.
		private static DbcsConvert convert;
		
		// Get the primary GB2312 conversion object.
		public static DbcsConvert Convert
		{
			get {
				if (convert == null) convert = new DbcsConvert("gb2312.table");
				return convert;
			}
		}
	}
}
