//
// System.Text.ASCIIEncoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {
        
	public class ASCIIEncoding : Encoding
	{
		public ASCIIEncoding () : base ("ASCII", false)
		{
			encoding_name = "US-ASCII";
			body_name = "us-ascii";
			header_name = "us-ascii";
			web_name = "us-ascii";
			is_browser_display = false;
			is_browser_save = false;
			is_mail_news_display = true;
			is_mail_news_save = true;
		}

		[MonoTODO]
		public override int GetMaxByteCount (int charCount)
		{
			// FIXME: this is wrong, dont know the right value
			return charCount*6;
		}

		public override int GetMaxCharCount (int byteCount)
		{
			return byteCount;
		}
	}
}

