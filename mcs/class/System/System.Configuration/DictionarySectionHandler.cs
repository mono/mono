
//
// System.Configuration.DictionarySectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//
using System;
using System.Collections;
using System.Collections.Specialized;
#if (XML_DEP)
using System.Xml;
#endif

namespace System.Configuration
{
	/// <summary>
	/// Summary description for DictionarySectionHandler.
	/// </summary>
	public class DictionarySectionHandler : IConfigurationSectionHandler
	{
#if (XML_DEP)

		/// <summary>
		///		Creates a new DictionarySectionHandler object and adds the object to the collection.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="context">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns></returns>
		public virtual object Create(object parent, object context, XmlNode section)
		{
			return ConfigHelper.GetDictionary (parent as IDictionary, section,
							   KeyAttributeName, ValueAttributeName);
		}
#endif

		/// <summary>
		///		Gets the name of the key attribute tag. This property is overidden by derived classes to change 
		///		the name of the key attribute tag. The default is "key".
		/// </summary>
		protected virtual string KeyAttributeName
		{
			get {
				return "key";
			}
		}

		/// <summary>
		///		Gets the name of the value tag. This property may be overidden by derived classes to change
		///		the name of the value tag. The default is "value".
		/// </summary>
		protected virtual string ValueAttributeName 
		{
			get {
				return "value";
			}
		}
	}
}
