//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;

    [ContentProperty("Namespaces")]
    public class XPathMessageContextMarkupExtension : MarkupExtension
    {
        static List<string> implicitPrefixes;
        Dictionary<string, string> namespaces;

        static XPathMessageContextMarkupExtension()
        {
            implicitPrefixes = new List<string>();

            foreach (string prefix in XPathMessageContext.defaultNamespaces.Keys)
            {
                implicitPrefixes.Add(prefix);
            }

            implicitPrefixes.Add("");
            implicitPrefixes.Add("xml");
            implicitPrefixes.Add("xmlns");
        }

        public XPathMessageContextMarkupExtension()
        {
            this.namespaces = new Dictionary<string, string>();
        }

        public XPathMessageContextMarkupExtension(XPathMessageContext context)
            : this()
        {
            foreach (string prefix in context)
            {
                if (!implicitPrefixes.Contains(prefix))
                {
                    this.namespaces.Add(prefix, context.LookupNamespace(prefix));
                }
            }
        }

        public Dictionary<string, string> Namespaces
        {
            get { return this.namespaces; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            XPathMessageContext context = new XPathMessageContext();

            foreach (KeyValuePair<string, string> ns in this.namespaces)
            {
                context.AddNamespace(ns.Key, ns.Value);
            }

            return context;
        }
    }
}
