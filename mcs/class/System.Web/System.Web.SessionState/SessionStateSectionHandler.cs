//
// System.Web.SessionState.SessionStateSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Xml;

namespace System.Web.SessionState
{
	class SessionStateSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			//TODO: context?
			SessionConfig config = new SessionConfig (parent);
			if (section.HasChildNodes)
				ThrowException ("No children nodes allowed", section);

			string attvalue = AttValue ("mode", section, false);
			if (!config.SetMode (attvalue))
				ThrowException ("Invalid mode value", section);
			
			if (section.Attributes == null)
				return config;

			attvalue = AttValue ("cookieless", section);
			if (attvalue != null)
				if (!config.SetCookieLess (attvalue))
					ThrowException ("Invalid cookieless value", section);

			attvalue = AttValue ("timeout", section);
			if (attvalue != null)
				if (!config.SetTimeout (attvalue))
					ThrowException ("Invalid timeout value", section);

			attvalue = AttValue ("stateConnectionString", section);
			if (attvalue != null)
				config.SetStateConnectionString (attvalue);

			attvalue = AttValue ("sqlConnectionString", section);
			if (attvalue != null)
				config.SetSqlConnectionString (attvalue);

			if (section.Attributes != null && section.Attributes.Count > 0)
				HandlersUtil.ThrowException ("Unknown attribute.", section);

			return config;
		}

		// A few methods to save some typing
		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
}

