using System;
using System.Xml;
using System.Xml.Xsl;

namespace Transform
{
	public class Transform
	{
		public static void Main (string [] rgstrArgs)
		{
			XmlReader xml = XmlReader.Create (rgstrArgs [0]);

			XslCompiledTransform xsl = new XslCompiledTransform ();
			xsl.Load (rgstrArgs [1]);

			xsl.Transform (xml, null, Console.Out);
		}
	}
}
