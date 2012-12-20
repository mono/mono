using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;

using Monodoc;
using Monodoc.Generators;

namespace Monodoc.Generators.Html
{
	// Input is expected to be already HTML so just return it
	public class MonoBook2Html : IHtmlExporter
	{
		public string CssCode {
			get {
				return @"   h3 { 
       font-size: 18px;
       padding-bottom: 4pt;
       border-bottom: 2px solid #dddddd;
   }
       
   .api {
     border: 1px solid;
     padding: 10pt;
     margin: 10pt;
   } 

   .api-entry { 
       border-bottom: none;
       font-size: 18px;
   }

   .prototype {
     border: 1px solid;
     background-color: #f2f2f2;
     padding: 5pt;
     margin-top: 5pt;
     margin-bottom: 5pt;  
   } 

   .header {
     border: 1px solid !important;
     padding: 0 0 5pt 5pt !important;
     margin: 10pt !important;
     white-space: pre !important;
       font-family: monospace !important;
     font-weight: normal !important;
     font-size: 1em !important;
   }
    
   .code {
     border: 1px solid;
     padding: 0 0 5pt 5pt;
     margin: 10pt;
     white-space: pre;
       font-family: monospace;
   }
";
			}
		}

		public string Export (Stream input, Dictionary<string, string> extraArgs)
		{
			if (input == null)
				return null;
			return FromXmlReader (XmlReader.Create (input));
		}

		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			if (string.IsNullOrEmpty (input))
				return null;
			return FromXmlReader (XmlReader.Create (new StringReader (input)));
		}

		public string FromXmlReader (XmlReader reader)
		{
			if (!reader.ReadToDescendant ("head"))
				return null;
			if (!reader.ReadToNextSibling ("body"))
				return null;

			return reader.ReadInnerXml ();
		}
	}
}
