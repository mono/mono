//
// I18N.CJK.Big5Convert
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

using System;

namespace I18N.CJK
{
	internal sealed class Big5Convert : DbcsConvert
	{
		// Dummy constructor, no one is aupposed to call it
		private Big5Convert() : base("big5.table") {}
		
		// The one and only Big5 conversion object in the system.
		private static DbcsConvert convert;
		
		// Get the primary Big5 conversion object.
		public static DbcsConvert Convert
		{
			get {
				if (convert == null) convert = new DbcsConvert("big5.table");
				return convert;
			}
		}
	}
}
