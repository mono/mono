//
// WebServicesConfiguration.cs: Web Services Configuration
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Configuration;
using System.Xml;

namespace Microsoft.Web.Services.Configuration {

	public sealed class WebServicesConfiguration : ConfigurationBase, IConfigurationSectionHandler {

		public static FilterConfiguration FilterConfiguration { 
			get { return new FilterConfiguration (); }
		}
/* FIXME: Classes are not stubbed yet, breaks a WSE2 build
#if WSE2
		[MonoTODO()]
		public static MessagingConfiguration MessagingConfiguration {
			get { return null; }
		}

		[MonoTODO()]
		public static TokenIssuerConfiguration TokenIssuerConfiguration {
			get { return null; }
		}
#endif */

		// from IConfigurationSectionHandler
		[MonoTODO()]
		object IConfigurationSectionHandler.Create (object parent, object configContext, XmlNode section) 
		{
			return null;
		}
	}
}
