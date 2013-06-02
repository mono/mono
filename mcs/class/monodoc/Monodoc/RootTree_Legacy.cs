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

	public partial class RootTree
	{
		static IDocGenerator<string> rawGenerator = new RawGenerator ();
		static HtmlGenerator htmlGenerator = new HtmlGenerator (null);

		[Obsolete ("Use RawGenerator directly")]
		public XmlDocument GetHelpXml (string id)
		{
			var rendered = RenderUrl (id, rawGenerator);
			if (rendered == null)
				return null;
			var doc = new XmlDocument ();
			doc.LoadXml (RenderUrl (id, rawGenerator));
			return doc;
		}

		[Obsolete ("Use the RenderUrl variant accepting a generator")]
		public string RenderUrl (string url, out Node n)
		{
			return RenderUrl (url, htmlGenerator, out n);
		}

		[Obsolete ("Use GenerateIndex")]
		public static void MakeIndex (RootTree root)
		{
			root.GenerateIndex ();
		}

		[Obsolete ("Use GenerateSearchIndex")]
		public static void MakeSearchIndex (RootTree root)
		{
			root.GenerateSearchIndex ();
		}
	}
}

#endif
