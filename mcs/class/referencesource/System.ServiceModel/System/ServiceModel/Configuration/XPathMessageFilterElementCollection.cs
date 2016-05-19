//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Dispatcher;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(XPathMessageFilterElement))]
    public sealed class XPathMessageFilterElementCollection : ServiceModelConfigurationElementCollection<XPathMessageFilterElement>
    {
        public XPathMessageFilterElementCollection()
            : base(ConfigurationElementCollectionType.AddRemoveClearMap, null, new XPathMessageFilterElementComparer())
        { }

        public override bool ContainsKey(object key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            string newKey = string.Empty;
            if (key.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
            {
                newKey = XPathMessageFilterElementComparer.ParseXPathString((XPathMessageFilter)key);
            }
            else if (key.GetType().IsAssignableFrom(typeof(string)))
            {
                newKey = (string)key;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.ConfigInvalidKeyType,
                    "XPathMessageFilterElement",
                    typeof(XPathMessageFilter).AssemblyQualifiedName,
                    key.GetType().AssemblyQualifiedName)));
            }

            return base.ContainsKey(newKey);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            XPathMessageFilterElement configElement = (XPathMessageFilterElement)element;

            if (configElement.Filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("element",
                    SR.GetString(SR.ConfigXPathFilterIsNull));
            }

            return XPathMessageFilterElementComparer.ParseXPathString(configElement.Filter);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return new ConfigurationPropertyCollection(); }
        }

        public override XPathMessageFilterElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }

                if (!key.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.ConfigInvalidKeyType,
                        "XPathMessageFilterElement",
                        typeof(XPathMessageFilter).AssemblyQualifiedName,
                        key.GetType().AssemblyQualifiedName)));
                }

                XPathMessageFilterElement retval = (XPathMessageFilterElement)this.BaseGet(key);
                if (retval == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new System.Collections.Generic.KeyNotFoundException(
                        SR.GetString(SR.ConfigKeyNotFoundInElementCollection,
                        key.ToString())));
                }
                return retval;
            }
            set
            {
                if (this.IsReadOnly())
                {
                    Add(value);
                }

                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }

                if (!key.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.ConfigInvalidKeyType,
                        "XPathMessageFilterElement",
                        typeof(XPathMessageFilter).AssemblyQualifiedName,
                        key.GetType().AssemblyQualifiedName)));
                }

                string oldKey = XPathMessageFilterElementComparer.ParseXPathString((XPathMessageFilter)key);
                string newKey = (string)this.GetElementKey(value);

                if (String.Equals(oldKey, newKey, StringComparison.Ordinal))
                {
                    if (BaseGet(key) != null)
                    {
                        BaseRemove(key);
                    }
                    Add(value);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigKeysDoNotMatch,
                        this.GetElementKey(value).ToString(),
                        key.ToString()));
                }
            }
        }
    }
}


