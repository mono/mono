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


namespace System.Configuration
{
	/// <summary>
	/// Summary description for SingleTagSectionHandler.
	/// </summary>
	public class SingleTagSectionHandler : IConfigurationSectionHandler
	{

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
		public object Create(object parent, object context, XmlNode section)
		{
			//FIXME: Add Implemetation code here.
			return null;
		}
	}
}
