//
// System.Configuration.NameValueSectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//
using System;
using System.Xml;
using System.Collections.Specialized;

namespace System.Configuration
{
	/// <summary>
	/// Summary description for NameValueSectionHandler.
	/// </summary>
	public class NameValueSectionHandler : IConfigurationSectionHandler
	{
		private static string keyName;
		private static string valueName;
		private static NameValueCollection settingsCollection;
		

		/// <summary>
		///		NameValueSectionHandler Constructor
		/// </summary>
		public NameValueSectionHandler()
		{
			//Set Default Values.
			keyName = "key";
			valueName = "value";
			
			settingsCollection = new NameValueCollection();
			
		}

		/// <summary>
		///		Creates a new configuration handler and adds the specified configuration object to the collection.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="context">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns></returns>
		[MonoTODO]
		public object Create(object parent, object context, XmlNode section)
		{
			//FIXME: I'm not quite sure how to implement 'parent' or 'context'.
			

			//Get all of the ChildNodes in the XML section.
			XmlNodeList childNodeList = section.ChildNodes;
			
			//loop throught the ChildNodes
			for (int i=0; i < childNodeList.Count; i++)
			{
				XmlNode childNode = childNodeList[i];

				if(childNode.Name == "#text")
				{
					string text = childNode.Value.Trim (' ', '\t', '\n');
					if (text == "")
						continue;
				} else if(childNode.Name == "#whitespace")
					continue;
				
				//if the name of this childNode is not 'add' then throw a ConfigurationException.
				if(childNode.Name != "add")
				{
					throw (new ConfigurationException("Unrecognized element"));
				}

				//Get the attributes for the childNode
				XmlAttributeCollection xmlAttributes = childNode.Attributes;
				
				//Get the key and value Attributes by their Name
				XmlAttribute keyAttribute = xmlAttributes[keyName];
				XmlAttribute valueAttribute = xmlAttributes[valueName];
				
				//Add this Key/Value Pair to the collection
				settingsCollection.Add(keyAttribute.Value, valueAttribute.Value);

			}
			
						
			//FIXME: Something is missing here. MS's version of this method returns a System.Configuration.ReadOnlyNameValueCollection type,
			//this class id not documented ANYWHERE.  This method is curretly returning a NameValueCollection, but it should be ReadOnly.

			return settingsCollection;
		}

		/// <summary>
		///		Gets the name of the key in the key-value pair.
		/// </summary>
		protected virtual string KeyAttributeName
		{
			get
			{
				return keyName;
			}
		}

		/// <summary>
		///		Gets the value for the key in the key-value pair.
		/// </summary>
		protected virtual string ValueAttributeName 
		{
			get
			{
				return valueName;
			}
		}

	}
}
