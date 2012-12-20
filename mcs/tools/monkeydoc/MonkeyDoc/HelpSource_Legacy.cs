using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Utilities;
using Lucene.Net.Index;

#if LEGACY_MODE

namespace MonkeyDoc
{
	public partial class HelpSource
	{
		[Obsolete]
		public static bool use_css;
		[Obsolete]
		public static bool FullHtml = true;
		[Obsolete]
		public static bool UseWebdocCache;

		[Obsolete ("Use MonkeyDoc.Providers.HtmlGenerator.InlineCss")]
		public string InlineCss {
			get { return MonkeyDoc.Generators.HtmlGenerator.InlineCss; }
		}

		[Obsolete]
		public string InlineJavaScript {
			get { return null; }
		}
	}
}

#endif
