//
// I18N.CJK.DbcsConvert
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

using System;

namespace I18N.CJK
{
	// This class assists other DBCS encoding classes in converting back
	// and forth between JIS character sets and Unicode.  It uses
	// several large tables to do this, some of which are stored in
	// the resource section of the assembly for efficient access.
	internal sealed class DbcsConvert
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
	}
}
