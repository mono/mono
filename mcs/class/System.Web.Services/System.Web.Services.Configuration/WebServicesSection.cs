//
// System.Web.Services.Configuration.WebServicesSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;
using System.Web.Configuration;

namespace System.Web.Services.Configuration
{
        public sealed class WebServicesSection : ConfigurationSection
        {
                static ConfigurationProperty conformanceWarningsProp;
                static ConfigurationProperty protocolsProp;
                static ConfigurationProperty serviceDescriptionFormatExtensionTypesProp;
                static ConfigurationProperty soapEnvelopeProcessingProp;
                static ConfigurationProperty soapExtensionImporterTypesProp;
                static ConfigurationProperty soapExtensionReflectorTypesProp;
                static ConfigurationProperty soapExtensionTypesProp;
                static ConfigurationProperty soapServerProtocolFactoryProp;
                static ConfigurationProperty soapTransportImporterTypesProp;
                static ConfigurationProperty wsdlHelpGeneratorProp;
                static ConfigurationPropertyCollection properties;

                static WebServicesSection ()
                {
			conformanceWarningsProp = new ConfigurationProperty ("conformanceWarnings", typeof (WsiProfilesElementCollection));
                        protocolsProp = new ConfigurationProperty ("protocols", typeof (ProtocolElementCollection));
			serviceDescriptionFormatExtensionTypesProp = new ConfigurationProperty ("serviceDescriptionFormatExtensionTypes", typeof (TypeElementCollection));
			soapEnvelopeProcessingProp = new ConfigurationProperty ("soapEnvelopeProcessing", typeof (SoapEnvelopeProcessingElement));
			soapExtensionImporterTypesProp = new ConfigurationProperty ("soapExtensionImporterTypes", typeof (TypeElementCollection));
			soapExtensionReflectorTypesProp = new ConfigurationProperty ("soapExtensionReflectorTypes", typeof (TypeElementCollection));
			soapExtensionTypesProp = new ConfigurationProperty ("soapExtensionTypes", typeof (SoapExtensionTypeElementCollection));
			soapServerProtocolFactoryProp = new ConfigurationProperty ("soapServerProtocolFactory", typeof (TypeElement));
			soapTransportImporterTypesProp = new ConfigurationProperty ("soapTransportImporterTypes", typeof (TypeElementCollection));
			wsdlHelpGeneratorProp = new ConfigurationProperty ("wsdlHelpGenerator", typeof (WsdlHelpGeneratorElement));
                        properties = new ConfigurationPropertyCollection ();

			properties.Add (conformanceWarningsProp);
                        properties.Add (protocolsProp);
			properties.Add (serviceDescriptionFormatExtensionTypesProp);
			properties.Add (soapEnvelopeProcessingProp);
			properties.Add (soapExtensionImporterTypesProp);
			properties.Add (soapExtensionReflectorTypesProp);
			properties.Add (soapExtensionTypesProp);
			properties.Add (soapServerProtocolFactoryProp);
			properties.Add (soapTransportImporterTypesProp);
			properties.Add (wsdlHelpGeneratorProp);

                }

                public WebServicesSection GetSection (System.Configuration.Configuration config)
                {
			return (WebServicesSection)config.GetSection ("webServices");
                }

		protected override void InitializeDefault ()
		{
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}

                [ConfigurationProperty ("conformanceWarnings")]
                public WsiProfilesElementCollection ConformanceWarnings {
                        get { return (WsiProfilesElementCollection) base [conformanceWarningsProp];}
                }

		[MonoTODO]
                public WebServicesSection Current {
                        get { throw new NotImplementedException (); }
                }

		[MonoTODO]
                public DiagnosticsElement Diagnostics {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

		[MonoTODO]
                public WebServiceProtocols EnabledProtocols {
                        get { throw new NotImplementedException (); }
                }

                [ConfigurationProperty ("protocols")]
                public ProtocolElementCollection Protocols {
                        get { return (ProtocolElementCollection) base [protocolsProp];}
                }

                [ConfigurationProperty ("serviceDescriptionFormatExtensionTypes")]
                public TypeElementCollection ServiceDescriptionFormatExtensionTypes {
                        get { return (TypeElementCollection) base [serviceDescriptionFormatExtensionTypesProp];}
                }

                [ConfigurationProperty ("soapEnvelopeProcessing")]
                public SoapEnvelopeProcessingElement SoapEnvelopeProcessing {
                        get { return (SoapEnvelopeProcessingElement) base [soapEnvelopeProcessingProp];}
                        set { base[soapEnvelopeProcessingProp] = value; }
                }

                [ConfigurationProperty ("soapExtensionImporterTypes")]
                public TypeElementCollection SoapExtensionImporterTypes {
                        get { return (TypeElementCollection) base [soapExtensionImporterTypesProp];}
                }

                [ConfigurationProperty ("soapExtensionReflectorTypes")]
                public TypeElementCollection SoapExtensionReflectorTypes {
                        get { return (TypeElementCollection) base [soapExtensionReflectorTypesProp];}
                }

                [ConfigurationProperty ("soapExtensionTypes")]
                public SoapExtensionTypeElementCollection SoapExtensionTypes {
                        get { return (SoapExtensionTypeElementCollection) base [soapExtensionTypesProp];}
                }

                [ConfigurationProperty ("soapServerProtocolFactory")]
                public TypeElement SoapServerProtocolFactoryType {
                        get { return (TypeElement) base [soapServerProtocolFactoryProp];}
                }

                [ConfigurationProperty("soapTransportImporterTypes")]
                public TypeElementCollection SoapTransportImporterTypes {
                        get { return (TypeElementCollection) base [soapTransportImporterTypesProp];}
                }

                [ConfigurationProperty ("wsdlHelpGenerator")]
                public WsdlHelpGeneratorElement WsdlHelpGenerator {
                        get { return (WsdlHelpGeneratorElement) base [wsdlHelpGeneratorProp];}
                }

                protected override ConfigurationPropertyCollection Properties {
                        get { return properties; }
                }

		internal static WebServicesSection Instance {
			get { return (WebServicesSection)WebConfigurationManager.GetWebApplicationSection ("system.web/webServices"); }
		}

		internal static bool IsSupported (WebServiceProtocols proto)
		{
			return ((Instance.EnabledProtocols & proto) == proto && (proto != 0));
		}
        }

}

#endif
