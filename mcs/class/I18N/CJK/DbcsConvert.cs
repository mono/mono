//
// I18N.CJK.DbcsConvert
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//   Atsushi Enomoto  <atsushi@ximian.com>
//

using System;

namespace I18N.CJK
{
	// This class assists other DBCS encoding classes in converting back
	// and forth between JIS character sets and Unicode.  It uses
	// several large tables to do this, some of which are stored in
	// the resource section of the assembly for efficient access.
	internal class DbcsConvert
	{
		// Public access to the conversion tables.
		public byte[] n2u;
		public byte[] u2n;
		
		// Constructor.
		internal DbcsConvert(string fileName) {
			using (CodeTable table = new CodeTable(fileName)) {
				n2u = table.GetSection(1);
				u2n = table.GetSection(2);
			}
		}

		internal static readonly DbcsConvert Gb2312 =
			new DbcsConvert ("gb2312.table");
		internal static readonly DbcsConvert Big5 =
			new DbcsConvert ("big5.table");
		internal static readonly DbcsConvert KS =
			new DbcsConvert ("ks.table");
	}
}
