//
// System.Web.Configuration.PagesConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novel.com)
//

using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class PagesConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
			PagesConfiguration config = new PagesConfiguration (parent);

			if (section.HasChildNodes)
				HandlersUtil.ThrowException ("No child nodes allowed here.", section);

			bool bvalue = false;
			string attvalue = AttValue ("buffer", section);
			if (attvalue != null)
				config.Buffer = GetBool ("buffer", attvalue, section);

			attvalue = AttValue ("enableSessionState", section);
			if (attvalue != null) {
				if (attvalue != "true" && attvalue != "false" && attvalue != "ReadOnly")
					HandlersUtil.ThrowException ("Invalid value for 'enableSessionState'", section);

				config.EnableSessionState = attvalue;
			}

			attvalue = AttValue ("enableViewState", section);
			if (attvalue != null)
				config.EnableViewState = GetBool ("enableViewState", attvalue, section);

			attvalue = AttValue ("enableViewStateMac", section);
			if (attvalue != null)
				config.EnableViewStateMac = GetBool ("enableViewStateMac", attvalue, section);

			attvalue = AttValue ("smartNavigation", section);
			if (attvalue != null)
				config.SmartNavigation = GetBool ("smartNavigation", attvalue, section);

			attvalue = AttValue ("autoEventWireup", section);
			if (attvalue != null)
				config.AutoEventWireup = GetBool ("autoEventWireup", attvalue, section);

			attvalue = AttValue ("validateRequest", section);
			if (attvalue != null)
				config.ValidateRequest = GetBool ("validateRequest", attvalue, section);

			attvalue = AttValue ("pageBaseType", section);
			if (attvalue != null) {
				string v = attvalue.Trim ();
				if (v.Length == 0)
					HandlersUtil.ThrowException ("pageBaseType is empty.", section);

				config.PageBaseType = v;
			}

			attvalue = AttValue ("userControlBaseType", section);
			if (attvalue != null) {
				string v = attvalue.Trim ();
				if (v.Length == 0)
					HandlersUtil.ThrowException ("userControlBaseType is empty.", section);

				config.UserControlBaseType = v;
			}

			if (section.Attributes == null || section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unknown attribute(s).", section);

			return config;
		}

		static bool GetBool (string name, string value, XmlNode section)
		{
			if (value == "true")
				return true;

			if (value != "false")
				HandlersUtil.ThrowException ("Invalid boolean value for '" + name + "'", section);

			return false;
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
		//
	}
}

