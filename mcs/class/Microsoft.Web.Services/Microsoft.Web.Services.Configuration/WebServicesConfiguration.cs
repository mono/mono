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

	internal class WSEConfig {
		bool diagnosticsTraceEnabled;
		string diagnosticsTraceInputFilename;
		string diagnosticsTraceOutputFilename;

		public WSEConfig (XmlNode section) 
		{
			XmlNode trace = section.SelectSingleNode ("/diagnostics/trace");
			if (trace != null) {
				diagnosticsTraceEnabled = (trace.Attributes ["enabled"].InnerText == "true");
				diagnosticsTraceInputFilename = trace.Attributes ["input"].InnerText;
				diagnosticsTraceOutputFilename = trace.Attributes ["output"].InnerText;
			}
		}

		public bool Trace {
			get { return diagnosticsTraceEnabled; }
		}

		public string TraceInput {
			get { return diagnosticsTraceInputFilename; }
		}

		public string TraceOutput {
			get { return diagnosticsTraceOutputFilename; }
		}
	}

	public sealed class WebServicesConfiguration : ConfigurationBase, IConfigurationSectionHandler {

		static WSEConfig config;

		static WebServicesConfiguration () 
		{
			config = (WSEConfig) ConfigurationSettings.GetConfig ("microsoft.web.services");
		}

		internal WebServicesConfiguration () {}

		internal static WSEConfig Config {
			get { return config; }
		}

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
			return new WSEConfig (section);
		}
	}
}
