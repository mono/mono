//
// System.Text.UTF7Encoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {

	public class UTF7Encoding : Encoding
	{
		public UTF7Encoding () : base ("UTF-7", false)
		{
			encoding_name = "Unicode (UTF-7)";
			body_name = "utf-7";
			header_name = "utf-7";
			web_name = "utf-7";
			is_browser_display = false;
			is_browser_save = false;
			is_mail_news_display = true;
			is_mail_news_save = true;

		}

		[MonoTODO]
		public override int GetMaxByteCount (int charCount)
		{
			// FIXME: dont know if this is right
			return charCount*6;
		}

		public override int GetMaxCharCount (int byteCount)
		{
			return byteCount;
		}
	}
}

