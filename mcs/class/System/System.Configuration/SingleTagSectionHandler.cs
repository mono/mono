//
// System.Configuration.SingleTagSectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;
using System.Collections;
#if (XML_DEP)
using System.Xml;
#endif

namespace System.Configuration
{
	/// <summary>
	/// Summary description for SingleTagSectionHandler.
	/// </summary>
	public class SingleTagSectionHandler : IConfigurationSectionHandler
	{
#if (XML_DEP)
		/// <summary>
		///		Returns a collection of configuration section values.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="context"></param>
		/// <param name="section">The name of the configuration section.</param>
		/// <returns></returns>
		public virtual object Create(object parent, object context, XmlNode section)
		{
			Hashtable settingsCollection;
			
			if (parent == null)
				settingsCollection = new Hashtable ();
			else
				settingsCollection = (Hashtable) parent;

			//Get all of the ChildNodes in the XML section.
			if(section.HasChildNodes)
			{
				throw (new ConfigurationException("Child Nodes not allowed."));
			}
			
			
			//Get the attributes for the childNode
			XmlAttributeCollection xmlAttributes = section.Attributes;

			for(int i=0; i < xmlAttributes.Count; i++)
			{
				settingsCollection.Add(xmlAttributes[i].Name, xmlAttributes[i].Value);
			}
			
			return settingsCollection;
		}
#endif
	}
}
