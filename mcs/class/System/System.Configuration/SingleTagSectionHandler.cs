//
// System.Configuration.SingleTagSectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;
using System.Xml;
using System.Collections;


namespace System.Configuration
{
	/// <summary>
	/// Summary description for SingleTagSectionHandler.
	/// </summary>
	public class SingleTagSectionHandler : IConfigurationSectionHandler
	{

		[MonoTODO]
		public SingleTagSectionHandler()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		///		Returns a collection of configuration section values.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="context"></param>
		/// <param name="section">The name of the configuration section.</param>
		/// <returns></returns>
		[MonoTODO]
		public object Create(object parent, object context, XmlNode section)
		{
			//FIXME: I'm not quite sure how to implement 'parent' or 'context'.
			//TODO: Add in proper Error Handling.

			//Get all of the ChildNodes in the XML section.
			if(section.HasChildNodes)
			{
				throw (new ConfigurationException("Child Nodes not allowed."));
			}
			
			
			//Get the attributes for the childNode
			XmlAttributeCollection xmlAttributes = section.Attributes;

			Hashtable settingsCollection = new Hashtable();
			
			for(int i=0; i < xmlAttributes.Count; i++)
			{
				settingsCollection.Add(xmlAttributes[i].Name, xmlAttributes[i].Value);
			}
			
			return settingsCollection;
		}
	}
}
