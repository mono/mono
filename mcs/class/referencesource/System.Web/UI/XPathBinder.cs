//------------------------------------------------------------------------------
// <copyright file="XPathBinder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Globalization;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Xml;
    using System.Xml.XPath;


    /// <devdoc>
    /// </devdoc>
    public sealed class XPathBinder {

        /// <internalonly/>
        private XPathBinder() {
        }


        /// <devdoc>
        /// </devdoc>
        public static object Eval(object container, string xPath) {
            IXmlNamespaceResolver resolver = null;
            return Eval(container, xPath, resolver);
        }

        public static object Eval(object container, string xPath, IXmlNamespaceResolver resolver) {
            if (container == null) {
                throw new ArgumentNullException("container");
            }
            if (String.IsNullOrEmpty(xPath)) {
                throw new ArgumentNullException("xPath");
            }

            IXPathNavigable node = container as IXPathNavigable;
            if (node == null) {
                throw new ArgumentException(SR.GetString(SR.XPathBinder_MustBeIXPathNavigable, container.GetType().FullName));
            }
            XPathNavigator navigator = node.CreateNavigator();

            object retValue = navigator.Evaluate(xPath, resolver);

            // If we get back an XPathNodeIterator instead of a simple object, advance
            // the iterator to the first node and return the value.
            XPathNodeIterator iterator = retValue as XPathNodeIterator;
            if (iterator != null) {
                if (iterator.MoveNext()) {
                    retValue = iterator.Current.Value;
                }
                else {
                    retValue = null;
                }
            }

            return retValue;
        }


        /// <devdoc>
        /// </devdoc>
        public static string Eval(object container, string xPath, string format) {
            return Eval(container, xPath, format, null);
        }

        public static string Eval(object container, string xPath, string format, IXmlNamespaceResolver resolver) {
            object value = XPathBinder.Eval(container, xPath, resolver);

            if (value == null) {
                return String.Empty;
            }
            else {
                if (String.IsNullOrEmpty(format)) {
                    return value.ToString();
                }
                else {
                    return String.Format(format, value);
                }
            }
        }


        /// <devdoc>
        /// Evaluates an XPath query with respect to a context IXPathNavigable object that returns a NodeSet.
        /// </devdoc>
        public static IEnumerable Select(object container, string xPath) {
            return Select(container, xPath, null);
        }

        public static IEnumerable Select(object container, string xPath, IXmlNamespaceResolver resolver) {
            if (container == null) {
                throw new ArgumentNullException("container");
            }
            if (String.IsNullOrEmpty(xPath)) {
                throw new ArgumentNullException("xPath");
            }

            ArrayList results = new ArrayList();

            IXPathNavigable node = container as IXPathNavigable;
            if (node == null) {
                throw new ArgumentException(SR.GetString(SR.XPathBinder_MustBeIXPathNavigable, container.GetType().FullName));
            }
            XPathNavigator navigator = node.CreateNavigator();
            XPathNodeIterator iterator = navigator.Select(xPath, resolver);

            while (iterator.MoveNext()) {
                IHasXmlNode hasXmlNode = iterator.Current as IHasXmlNode;
                if (hasXmlNode == null) {
                    throw new InvalidOperationException(SR.GetString(SR.XPathBinder_MustHaveXmlNodes));
                }
                results.Add(hasXmlNode.GetNode());
            }

            return results;
        }
    }
}

