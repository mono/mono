//
// System.Text.UTF8Encoding.cs
//
// Authors:
//   Sean MacIsaac (macisaac@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Text
{
	[Serializable]
	public class UTF8Encoding : Encoding
	{
		public UTF8Encoding () : base ("UTF-8", false)
		{
			encoding_name = "Unicode (UTF-8)";
			body_name = "utf-8";
			header_name = "utf-8";
			web_name = "utf-8";
			is_browser_display = true;
			is_browser_save = true;
			is_mail_news_display = true;
			is_mail_news_save = true;
		}
		
		public override int GetMaxByteCount (int charCount)
		{
			return charCount*6;
		}

		public override int GetMaxCharCount (int byteCount)
		{
			return byteCount;
		}
	}
}
