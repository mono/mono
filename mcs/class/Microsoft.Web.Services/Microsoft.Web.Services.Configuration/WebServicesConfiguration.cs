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

		[MonoTODO("read from app.config")]
		public static FilterConfiguration FilterConfiguration { 
			get { return null; }
		}

		// from IConfigurationSectionHandler
		[MonoTODO()]
		object IConfigurationSectionHandler.Create (object parent, object configContext, XmlNode section) 
		{
			return null;
		}
	}
}
