using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Monodoc;

namespace Monodoc.Generators
{
	using Html;

	interface IHtmlExporter
	{
		string CssCode { get; }
		string Export (Stream input, Dictionary<string, string> extras);
		string Export (string input, Dictionary<string, string> extras);
	}

	public class HtmlGenerator : IDocGenerator<string>
	{
		const string cachePrefix = "htmlcached#";

		static string css_code;

		IDocCache defaultCache;
		static Dictionary<DocumentType, IHtmlExporter> converters;

		static HtmlGenerator ()
		{
			converters = new Dictionary<DocumentType, IHtmlExporter> {
				{ DocumentType.EcmaXml, new Ecma2Html () },
				{ DocumentType.Man, new Man2Html () },
				{ DocumentType.TocXml, new Toc2Html () },
				{ DocumentType.EcmaSpecXml, new Ecmaspec2Html () },
				{ DocumentType.ErrorXml, new Error2Html () },
				{ DocumentType.Html, new Idem () },
				{ DocumentType.MonoBook, new MonoBook2Html () },
				{ DocumentType.AddinXml, new Addin2Html () },
				{ DocumentType.PlainText, new Idem () },
			};
		}

		public HtmlGenerator (IDocCache defaultCache)
		{
			this.defaultCache = defaultCache;
		}

		public string Generate (HelpSource hs, string id, Dictionary<string, string> context)
		{
			if (hs == null || string.IsNullOrEmpty (id))
				return MakeHtmlError (string.Format ("Your request has found no candidate provider [hs=\"{0}\", id=\"{1}\"]",
				                                     hs == null ? "(null)" : hs.Name, id ?? "(null)"));
			var cache = defaultCache ?? hs.Cache;
			if (cache != null && cache.IsCached (MakeCacheKey (hs, id, null)))
				return cache.GetCachedString (MakeCacheKey (hs, id, null));

			IEnumerable<string> parts;
			if (hs.IsMultiPart (id, out parts))
				return GenerateMultiPart (hs, parts, id, context);

			if (hs.IsRawContent (id))
				return hs.GetText (id) ?? string.Empty;

			DocumentType type = hs.GetDocumentTypeForId (id);
			if (cache != null && context != null && cache.IsCached (MakeCacheKey (hs, id, context)))
				return cache.GetCachedString (MakeCacheKey (hs, id, context));

			IHtmlExporter exporter;
			if (!converters.TryGetValue (type, out exporter))
				return MakeHtmlError (string.Format ("Input type '{0}' not supported",
				                                     type.ToString ()));
			var result = hs.IsGeneratedContent (id) ? 
				exporter.Export (hs.GetCachedText (id), context) :
				exporter.Export (hs.GetCachedHelpStream (id), context);

			if (cache != null)
				cache.CacheText (MakeCacheKey (hs, id, context), result);
			return result;
		}

		string GenerateMultiPart (HelpSource hs, IEnumerable<string> ids, string originalId, Dictionary<string, string> context)
		{
			var sb = new StringBuilder ();
			foreach (var id in ids)
				sb.AppendLine (Generate (hs, id, context));

			var cache = defaultCache ?? hs.Cache;
			if (cache != null)
				cache.CacheText (MakeCacheKey (hs, originalId, null), sb.ToString ());
			return sb.ToString ();
		}

		public static string InlineCss {
			get {
				if (css_code != null)
					return css_code;

				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly (typeof (HtmlGenerator));
				Stream str_css = assembly.GetManifestResourceStream ("base.css");
				StringBuilder sb = new StringBuilder ((new StreamReader (str_css)).ReadToEnd());
				sb.Replace ("@@FONT_FAMILY@@", "Sans Serif");
				sb.Replace ("@@FONT_SIZE@@", "100%");
				css_code = sb.ToString () + converters.Values
					.Select (c => c.CssCode)
					.Where (css => !string.IsNullOrEmpty (css))
					.DefaultIfEmpty (string.Empty)
					.Aggregate (string.Concat);
				return css_code;
			}
			set { 
				css_code = value;
			}
		}

		string MakeHtmlError (string error)
		{
			return string.Format ("<html><head></head><body><p>{0}</p></body></html>", error);
		}

		string MakeCacheKey (HelpSource hs, string page, IDictionary<string,string> extraParams)
		{
			var key = cachePrefix + hs.SourceID + page;
			if (extraParams != null && extraParams.Count > 0) {
				var paramPart = string.Join ("-", extraParams.Select (kvp => kvp.Key + kvp.Value));
				key += '_' + paramPart;
			}
			return key;
		}
	}
}
