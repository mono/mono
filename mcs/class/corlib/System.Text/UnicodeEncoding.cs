// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Text.UnicodeEncoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

// FIXME: implement byteOrderMark

namespace System.Text {
      
	[MonoTODO]
	public class UnicodeEncoding : Encoding
	{
		private bool byteOrderMark;
		
		private void init (bool byteOrderMark)
		{
			this.byteOrderMark = byteOrderMark;
			encoding_name = "Unicode";
			body_name = "utf-16";
			header_name = "utf-16";
			web_name = "utf-16";
			is_browser_display = false;
			is_browser_save = true;
			is_mail_news_display = false;
			is_mail_news_save = false;
		}
		
		public UnicodeEncoding () : base ("UNICODE", false)
		{
			init (false);
		}
		
                public UnicodeEncoding (bool bigEndian, bool byteOrderMark) : base ("UNICODE", bigEndian)
		{
			init (byteOrderMark);
		}
		
		public override int GetMaxByteCount (int charCount)
		{
			return charCount;
		}

		public override int GetMaxCharCount (int byteCount)
		{
			return byteCount / 2;
		}
	}
}
