//------------------------------------------------------------------------------
// <copyright file="WebServiceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {

    using System;
    using System.Web.Services.Protocols;

    /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceAttribute"]/*' />
    /// <devdoc>
    ///    <para> The WebService attribute may be used to add additional information to a
    ///       Web Service, such as a string describing its functionality. The attribute is not required for a Web Service to
    ///       be published and executed.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class WebServiceAttribute : Attribute {
        string description;
        string ns = DefaultNamespace;
        string name;

        /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceAttribute.WebServiceAttribute"]/*' />
        /// <devdoc>
        ///    Initializes a new instance of the <see cref='System.Web.Services.WebServiceAttribute'/> class.
        /// </devdoc>
        public WebServiceAttribute() {
        }

        /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceAttribute.Description"]/*' />
        /// <devdoc>
        ///    A descriptive message for the Web Service. The message
        ///    is used when generating description documents for the Web Service, such as the
        ///    Sevice Contract and the Service Description page.
        /// </devdoc>
        public string Description {
            get {
                return (description == null) ? string.Empty : description;
            }

            set {
                description = value;
            }
        }

        /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceAttribute.Namespace"]/*' />
        /// <devdoc>
        /// The default XML namespace to use for the web service.
        /// </devdoc>
        public string Namespace {
            get {
                return ns;
            }
            set {
                ns = value;
            }
        }

        /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceAttribute.Name"]/*' />
        /// <devdoc>
        /// The name to use for the web service in the service description.
        /// </devdoc>
        public string Name {
            get {
                return name == null ? string.Empty : name;
            }
            set {
                name = value;
            }
        }

        /// <include file='doc\WebServiceAttribute.uex' path='docs/doc[@for="WebServiceAttribute.DefaultNamespace"]/*' />
        /// <devdoc>
        /// The default value for the XmlNamespace property.
        /// </devdoc>
        public const string DefaultNamespace = "http://tempuri.org/";
    }

    internal class WebServiceReflector {
        private WebServiceReflector() { }
        internal static WebServiceAttribute GetAttribute(Type type) {
            object[] attrs = type.GetCustomAttributes(typeof(WebServiceAttribute), false);
            if (attrs.Length == 0) return new WebServiceAttribute();
            return (WebServiceAttribute)attrs[0];
        }

        internal static WebServiceAttribute GetAttribute(LogicalMethodInfo[] methodInfos) {
            if (methodInfos.Length == 0) return new WebServiceAttribute();
            Type mostDerived = GetMostDerivedType(methodInfos);
            return GetAttribute(mostDerived);
        }

        internal static Type GetMostDerivedType(LogicalMethodInfo[] methodInfos) {
            if (methodInfos.Length == 0) return null;
            Type mostDerived = methodInfos[0].DeclaringType;
            for (int i = 1; i < methodInfos.Length; i++) {
                Type derived = methodInfos[i].DeclaringType;
                if (derived.IsSubclassOf(mostDerived)) {
                    mostDerived = derived;
                }
            }
            return mostDerived;
        }
    }
}
