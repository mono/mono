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

namespace System.Configuration
{
	/// <summary>
	/// Summary description for NameValueSectionHandler.
	/// </summary>
	public class NameValueSectionHandler : IConfigurationSectionHandler
	{
		private static string _stringKey;
		private static string _stringValue;

		/// <summary>
		///		NameValueSectionHandler Constructor
		/// </summary>
		public NameValueSectionHandler()
		{
			//Set Default Values.
			_stringKey = "key";
			_stringValue = "value";
		}

		/// <summary>
		///		Creates a new configuration handler and adds the specified configuration object to the collection.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="context">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns></returns>
		public object Create(object parent, object context, XmlNode section)
		{
			//FIXME: Add Implemetation code here.
			return null;
		}

		/// <summary>
		///		Gets the name of the key in the key-value pair.
		/// </summary>
		protected virtual string KeyAttributeName
		{
			get
			{
				return _stringKey;
			}
		}

		/// <summary>
		///		Gets the value for the key in the key-value pair.
		/// </summary>
		protected virtual string ValueAttributeName 
		{
			get
			{
				return _stringValue;
			}
		}

	}
}
