//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Dispatcher;
    using System.Collections;
    using System.Text;
    using System.Xml;

    public class XPathMessageFilterElementComparer : IComparer
    {
        #region IComparer Members

        int IComparer.Compare(object x, object y)
        {
            string elementKey1 = TranslateObjectToElementKey(x);
            string elementKey2 = TranslateObjectToElementKey(y);

            return String.Compare(elementKey1, elementKey2, StringComparison.Ordinal);
        }

        #endregion

        internal static string ParseXPathString(XPathMessageFilter filter)
        {
            return XPathMessageFilterElementComparer.ParseXPathString(filter, false);
        }
        internal static string ParseXPathString(XPathMessageFilter filter, bool throwOnFailure)
        {
            XPathLexer lexer = new XPathLexer(filter.XPath);
            return XPathMessageFilterElementComparer.ParseXPathString(lexer, filter.Namespaces, throwOnFailure);
        }

        static string ParseXPathString(XPathLexer lexer, XmlNamespaceManager namespaceManager, bool throwOnFailure)
        {
            string retVal = String.Empty;

            int currentTokenStart = lexer.FirstTokenChar;
            if (lexer.MoveNext())
            {
                XPathToken token = lexer.Token;
                StringBuilder xPathString = new StringBuilder(XPathMessageFilterElementComparer.ParseXPathString(lexer, namespaceManager, throwOnFailure));

                if (XPathTokenID.NameTest == token.TokenID)
                {
                    string nsPrefix = token.Prefix;
                    if (!String.IsNullOrEmpty(nsPrefix))
                    {
                        string ns = namespaceManager.LookupNamespace(nsPrefix);
                        if (!String.IsNullOrEmpty(ns))
                        {
                            xPathString = xPathString.Replace(nsPrefix, ns, currentTokenStart, nsPrefix.Length);
                        }
                        else
                        {
                            if (throwOnFailure)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IndexOutOfRangeException(SR.GetString(SR.ConfigXPathNamespacePrefixNotFound, nsPrefix)));
                            }
                        }
                    }
                }

                retVal = xPathString.ToString();
            }
            else
            {
                retVal = lexer.ConsumedSubstring();
            }

            return retVal;
        }

        string TranslateObjectToElementKey(object obj)
        {
            string elementKey = null;

            if (obj.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
            {
                elementKey = XPathMessageFilterElementComparer.ParseXPathString((XPathMessageFilter)obj);
            }
            else if (obj.GetType().IsAssignableFrom(typeof(XPathMessageFilterElement)))
            {
                elementKey = XPathMessageFilterElementComparer.ParseXPathString(((XPathMessageFilterElement)obj).Filter);
            }
            else if (obj.GetType().IsAssignableFrom(typeof(string)))
            {
                elementKey = (string)obj;
            }

            if (String.IsNullOrEmpty(elementKey))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.ConfigCannotParseXPathFilter, obj.GetType().AssemblyQualifiedName)));
            }

            return elementKey;
        }
    }
}
