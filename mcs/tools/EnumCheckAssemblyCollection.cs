/**
 * Namespace: System.Web
 * Class:     EnumCheckAssembly
 *
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Reflection;

namespace Mono.Enumerations
{
	public class EnumCheckAssemblyCollection: IEnumerable
	{
		public string ConfigFile = "assemblies.xml";
		
		private ArrayList assemblyList = new ArrayList();

		public EnumCheckAssemblyCollection()
		{
		}

		public void Parse()
		{
			Stream      fStream;
			XmlReader   reader;
			XmlDocument document;
			string      url;

			fStream = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			reader = new XmlTextReader(fStream);
			document = new XmlDocument();
			document.Load(reader);
			if(document.DocumentElement != null)
			{
				if(document.DocumentElement.LocalName == "assemblies")
				{
					foreach(XmlNode pathNode in document.DocumentElement)
					{
						if(pathNode.NodeType == XmlNodeType.Element && pathNode.LocalName=="path")
						{
							url = pathNode.Attributes["url"].Value;
							while(url.EndsWith("\\") || url.EndsWith("/"))
							{
								url = url.Substring(0, url.Length - 1);
							}
							if(url == null || url.Length == 0)
							{
								continue;
							}
							foreach(XmlNode assemblyNode in pathNode.ChildNodes)
							{
								if(assemblyNode.LocalName == "assembly")
								{
									assemblyList.Add(url + "\\" + assemblyNode.Attributes["file"].Value);
								}
							}
						}
					}
				}
			}
			fStream.Close();
		}

		public IEnumerator GetEnumerator()
		{
			return assemblyList.GetEnumerator();
		}
	}
}
