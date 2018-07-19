//------------------------------------------------------------------------------
// <copyright file="WebPartsSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.Util;
    using System.Xml;
    using System.Security.Permissions;

    /*  <!-- Configuration for webParts:
                enableExport="[true|false]" - Enable the export of Web Parts (defaults to false)
                <personalization>
                    <providers>            (Can only be defined at the application level)
                        <add               Register a provider
                            name="string"      Identification of the provider
                            type="string"      Class that implements the provider
                            ... />             Additional provider specific configuration attributes
                        <remove            Unregister a provider
                            name="string" />   Identification of the provider
                        <clear />          Unregister all providers
                    </providers>
                    <authorization>
                        [Same content as an <authorization> section]
                    </authorization>
                </personalization>
                <transformers>
                    <add                    Add a transformer
                        name="string"           Name to identify this transformer instance by
                        type="string"           Class that extends Transformer
                        />
                    <remove                 Remove a transformer
                        name="string" />        Name of transformer to remove
                    <clear>                 Remove all transformers
                </transformers>
        -->
        <webParts enableExport="false">
            <personalization defaultProvider="AspNetSqlPersonalizationProvider">
                <providers>
                    <add name="AspNetSqlPersonalizationProvider"
                         type="System.Web.UI.WebControls.WebParts.SqlPersonalizationProvider, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                         connectionStringName="LocalSqlServer"
                         applicationName="/" />
                </providers>
                <authorization>
                    <deny users="*" verbs="enterSharedScope" />
                    <allow users="*" verbs="modifyState" />
                </authorization>
            </personalization>
            <transformers>
                <add name="RowToFieldTransformer"
                     type="System.Web.UI.WebControls.WebParts.RowToFieldTransformer" />
                <add name="RowToParametersTransformer"
                     type="System.Web.UI.WebControls.WebParts.RowToParametersTransformer" />
            </transformers>
        </webParts>

*/
    public sealed class WebPartsSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propEnableExport =
            new ConfigurationProperty("enableExport",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propPersonalization =
            new ConfigurationProperty("personalization",
                                        typeof(WebPartsPersonalization),
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTransformers =
            new ConfigurationProperty("transformers",
                                        typeof(TransformerInfoCollection),
                                        null,
                                        ConfigurationPropertyOptions.IsDefaultCollection);

        static WebPartsSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_propEnableExport);
            _properties.Add(_propPersonalization);
            _properties.Add(_propTransformers);
        }

        public WebPartsSection() {
        }
        /*
                protected override void InitializeDefault()
                {
                    /* Don't Add to Basicmap
                    AuthorizationRule rule0 = new AuthorizationRule(AuthorizationRuleAction.Deny);
                    rule0.Users.Add("*");
                    rule0.Verbs.Add("enterSharedScope");
                    Personalization.Authorization.Rules.Add(rule0);

                    AuthorizationRule rule1 = new AuthorizationRule(AuthorizationRuleAction.Allow);
                    rule1.Users.Add("*");
                    rule1.Verbs.Add("modifyState");
                    Personalization.Authorization.Rules.Add(rule1);
                    */
        /*        }
        */

        [ConfigurationProperty("enableExport", DefaultValue = false)]
        public bool EnableExport {
            get {
                return (bool)base[_propEnableExport];
            }
            set {
                base[_propEnableExport] = value;
            }
        }

        [ConfigurationProperty("personalization")]
        public WebPartsPersonalization Personalization {
            get {
                return (WebPartsPersonalization)base[_propPersonalization];
            }
        }

        /// <internalonly />
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("transformers")]
        public TransformerInfoCollection Transformers {
            get {
                return (TransformerInfoCollection)base[_propTransformers];
            }
        }

        protected override object GetRuntimeObject() {
            Personalization.ValidateAuthorization();
            return base.GetRuntimeObject();
        }
    }
}
