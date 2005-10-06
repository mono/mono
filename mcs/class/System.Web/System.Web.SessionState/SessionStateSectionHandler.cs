//
// System.Web.SessionState.SessionStateSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

                        attvalue = AttValue ("stateNetworkTimeout", section);
                        if (attvalue != null)
                                config.SetStateNetworkTimeout (attvalue);

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

