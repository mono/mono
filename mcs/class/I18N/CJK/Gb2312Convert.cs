//
// I18N.CJK.Gb2312Convert
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//   Dick Porter (dick@ximian.com)
//

using System;

namespace I18N.CJK
{
	internal sealed class Gb2312Convert
	{
		public byte[] gb2312_to_unicode;
		public byte[] gb2312_from_unicode1;
		public byte[] gb2312_from_unicode2;
		public byte[] gb2312_from_unicode3;
		public byte[] gb2312_from_unicode4;
		public byte[] gb2312_from_unicode5;
		public byte[] gb2312_from_unicode6;
		public byte[] gb2312_from_unicode7;
		public byte[] gb2312_from_unicode8;
		public byte[] gb2312_from_unicode9;
		
		private Gb2312Convert() 
		{
			using (CodeTable table = new CodeTable ("gb2312.table")) {
				gb2312_to_unicode = table.GetSection (1);

				gb2312_from_unicode1 = table.GetSection (2);
				gb2312_from_unicode2 = table.GetSection (3);
				gb2312_from_unicode3 = table.GetSection (4);
				gb2312_from_unicode4 = table.GetSection (5);
				gb2312_from_unicode5 = table.GetSection (6);
				gb2312_from_unicode6 = table.GetSection (7);
				gb2312_from_unicode7 = table.GetSection (8);
				gb2312_from_unicode8 = table.GetSection (9);
				gb2312_from_unicode9 = table.GetSection (10);
			}
		}
		
		
		// The one and only GB2312 conversion object in the system.
		private static Gb2312Convert convert;
		
		// Get the primary GB2312 conversion object.
		public static Gb2312Convert Convert
		{
			get {
				lock (typeof (Gb2312Convert)) {
					if (convert == null) {
						convert = new Gb2312Convert ();
					}
					return(convert);
				}
			}
		}
	}
}
