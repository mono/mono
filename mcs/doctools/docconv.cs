// docconv.cs
//
// Adam Treat (manyoso@yahoo.com)
//
// (C) 2002 Adam Treat
//
// Licensed under the terms of the GNU GPL

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Util
{
	public class DocConv
	{
		public static void Main(string[] args)
		{
			string xmlfile = null;
            string xslfile = null;
			string htmlfile = null;

            if(args.Length < 2)
			{
				Console.Write("Usage: docconv <xmlfile> <xslfile> <htmlout>\n");
				return;
            }

            if(!File.Exists(args[0]))
			{
                Console.WriteLine("\n   No Xml file at {0}\n", args[0]);
                return;
            }

            if(!File.Exists(args[1]))
			{
                Console.WriteLine("\n   No Xsl file at {0}\n", args[1]);
                return;
            }

			if(args.Length > 2)
			{
				htmlfile = args[2];
			}

			xmlfile = args[0];
			xslfile = args[1];
			htmlfile = args[2];

			try
			{
				// All this does is apply a transform to an xml file using an xsl stylesheet
				// Not to complicated.  The real magic is in the xsl stylesheet.
				XslTransform xslt = new XslTransform();
				xslt.Load(xslfile);
				XmlDocument xml = new XmlDocument();
				xml.PreserveWhitespace = true;
				xml.Load(xmlfile);
				XmlTextWriter writer = new XmlTextWriter(htmlfile, null);
				writer.Formatting = Formatting.Indented;
				xslt.Transform(((IXPathNavigable)xml).CreateNavigator(),null,writer);
				writer.Close();
			}
			catch(XmlException e)
			{
				throw(e);
			}
			catch(Exception e)
			{
				throw(e);
			}
		}
	}
}
