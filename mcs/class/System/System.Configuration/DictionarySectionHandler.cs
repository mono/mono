
//
// System.Configuration.DictionarySectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//
using System;
using System.Collections.Specialized;
using System.Xml;

namespace System.Configuration
{
	/// <summary>
	/// Summary description for DictionarySectionHandler.
	/// </summary>
	public class DictionarySectionHandler : IConfigurationSectionHandler
	{
		private static string _stringKeyName;
		private static string _stringValueName;

		/// <summary>
		///		DictionarySectionHandler Constructor
		/// </summary>
		public DictionarySectionHandler()
		{
			//Set Default Values.
			_stringKeyName = "key";
			_stringValueName = "value";
		}

		/// <summary>
		///		Creates a new DictionarySectionHandler object and adds the object to the collection.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="context">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns></returns>
		[MonoTODO]
		public virtual object Create(object parent, object context, XmlNode section)
		{
			//FIXME: Enter a meaningful error message
			if(section == null)
			{ throw new ConfigurationException("XML Node can not be null."); }
			
			//FIXME: Enter a meaningful error message
			if(parent == null)
			{ throw new ConfigurationException("", section); }

			
			DictionarySectionHandler objHandler = new DictionarySectionHandler();
			NameValueCollection objCollection;
			
			//Unbox parent as a NameValueCollection type.
			objCollection=(NameValueCollection)parent;

			objCollection.Add(section.Attributes[_stringKeyName].Value, section.Attributes[_stringValueName].Value);
			 
			return null;

			//FIXME: this code is far form complete, probably not even correct.
			
		}

		/// <summary>
		///		Gets the name of the key attribute tag. This property is overidden by derived classes to change 
		///		the name of the key attribute tag. The default is "key".
		/// </summary>
		protected virtual string KeyAttributeName
		{
			get
			{
				return _stringKeyName;
			}
		}

		/// <summary>
		///		Gets the name of the value tag. This property may be overidden by derived classes to change
		///		the name of the value tag. The default is "value".
		/// </summary>
		protected virtual string ValueAttributeName 
		{
			get
			{
				return _stringValueName;
			}
		}
	}
}
