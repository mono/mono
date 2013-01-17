using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Utilities;
using Lucene.Net.Index;

#if LEGACY_MODE

namespace Monodoc
{
	using Generators;

	public partial class HelpSource
	{
		static HtmlGenerator htmlGenerator = new HtmlGenerator (null);

		[Obsolete]
		public static bool use_css;
		[Obsolete]
		public static bool FullHtml = true;
		[Obsolete]
		public static bool UseWebdocCache;

		[Obsolete ("Use Monodoc.Providers.HtmlGenerator.InlineCss")]
		public string InlineCss {
			get { return Monodoc.Generators.HtmlGenerator.InlineCss; }
		}

		[Obsolete]
		public string InlineJavaScript {
			get { return null; }
		}

		[Obsolete ("Use RenderUrl")]
		public string GetText (string url, out Node node)
		{
			return rootTree.RenderUrl (url, htmlGenerator, out node, this);
		}

		[Obsolete ("Use RenderUrl")]
		public string RenderNamespaceLookup (string url, out Node node)
		{
			return rootTree.RenderUrl (url, htmlGenerator, out node, this);
		}
	}
}

#endif
