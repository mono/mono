//
// System.Configuration.IgnoreSectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;
#if (XML_DEP)
using System.Xml;
#endif

namespace System.Configuration
{
	/// <summary>
	/// Summary description for IgnoreSectionHandler.
	/// </summary>
	public class IgnoreSectionHandler : IConfigurationSectionHandler
	{
#if (XML_DEP)
		/// <summary>
		///		Creates a new configuration handler and adds the specified configuration object to the collection.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="configContext">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns></returns>
		public virtual object Create(object parent, object configContext, XmlNode section)
		{
			return null;
		}
#endif
	}
}
