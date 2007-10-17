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
		static ConfigurationProperty diagnosticsProp;
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
			conformanceWarningsProp = new ConfigurationProperty ("conformanceWarnings", typeof (WsiProfilesElementCollection), null,
									     null, null, ConfigurationPropertyOptions.None);
			diagnosticsProp = new ConfigurationProperty ("diagnostics", typeof (DiagnosticsElement), null,
								     null, null, ConfigurationPropertyOptions.None);
                        protocolsProp = new ConfigurationProperty ("protocols", typeof (ProtocolElementCollection), null,
								   null, null, ConfigurationPropertyOptions.None);
			serviceDescriptionFormatExtensionTypesProp = new ConfigurationProperty ("serviceDescriptionFormatExtensionTypes", typeof (TypeElementCollection), null,
												null, null, ConfigurationPropertyOptions.None);
			soapEnvelopeProcessingProp = new ConfigurationProperty ("soapEnvelopeProcessing", typeof (SoapEnvelopeProcessingElement), null,
										null, null, ConfigurationPropertyOptions.None);
			soapExtensionImporterTypesProp = new ConfigurationProperty ("soapExtensionImporterTypes", typeof (TypeElementCollection), null,
										    null, null, ConfigurationPropertyOptions.None);
			soapExtensionReflectorTypesProp = new ConfigurationProperty ("soapExtensionReflectorTypes", typeof (TypeElementCollection), null,
										     null, null, ConfigurationPropertyOptions.None);
			soapExtensionTypesProp = new ConfigurationProperty ("soapExtensionTypes", typeof (SoapExtensionTypeElementCollection), null,
									    null, null, ConfigurationPropertyOptions.None);
			soapServerProtocolFactoryProp = new ConfigurationProperty ("soapServerProtocolFactory", typeof (TypeElement), null,
										   null, null, ConfigurationPropertyOptions.None);
			soapTransportImporterTypesProp = new ConfigurationProperty ("soapTransportImporterTypes", typeof (TypeElementCollection), null,
										    null, null, ConfigurationPropertyOptions.None);
			wsdlHelpGeneratorProp = new ConfigurationProperty ("wsdlHelpGenerator", typeof (WsdlHelpGeneratorElement), null,
									   null, null, ConfigurationPropertyOptions.None);
                        properties = new ConfigurationPropertyCollection ();

			properties.Add (conformanceWarningsProp);
			properties.Add (diagnosticsProp);
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

                public static WebServicesSection GetSection (System.Configuration.Configuration config)
                {
			return (WebServicesSection) config.GetSection ("webServices");
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

                public DiagnosticsElement Diagnostics {
                        get { return (DiagnosticsElement) base [diagnosticsProp]; }
                        set { base[diagnosticsProp] = value; }
                }

		[MonoTODO]
		WebServiceProtocols enabledProtocols = WebServiceProtocols.Unknown;
                public WebServiceProtocols EnabledProtocols {
                        get {
				if (enabledProtocols == WebServiceProtocols.Unknown) {
					foreach (ProtocolElement el in Protocols)
						enabledProtocols |= el.Name;
				}

				return enabledProtocols;
			}
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

		public static WebServicesSection Current {
			get { return (WebServicesSection) ConfigurationManager.GetSection ("system.web/webServices"); }
		}

		internal static bool IsSupported (WebServiceProtocols proto)
		{
			return ((Current.EnabledProtocols & proto) == proto && (proto != WebServiceProtocols.Unknown));
		}
        }

}

#endif
