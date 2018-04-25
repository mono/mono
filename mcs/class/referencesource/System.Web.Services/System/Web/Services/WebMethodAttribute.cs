//------------------------------------------------------------------------------
// <copyright file="WebMethodAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {

    using System;
    using System.Reflection;
    using System.Collections;
    using System.Web.Util;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.EnterpriseServices;
    using System.Text;
    using System.Runtime.InteropServices;

    /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute"]/*' />
    /// <devdoc>
    ///    <para> The WebMethod attribute must be placed on a method in a Web Service class to mark it as available
    ///       to be called via the Web. The method and class must be marked public and must run inside of
    ///       an ASP.NET Web application.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebMethodAttribute : Attribute {
        private int transactionOption; // this is an int to prevent system.enterpriseservices.dll from getting loaded
        private bool enableSession;
        private int cacheDuration;
        private bool bufferResponse;
        private string description;
        private string messageName;
        
        private bool transactionOptionSpecified;
        private bool enableSessionSpecified;
        private bool cacheDurationSpecified;
        private bool bufferResponseSpecified;
        private bool descriptionSpecified;
        private bool messageNameSpecified;

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/> 
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute() {
            enableSession = false;
            transactionOption = 0; // TransactionOption.Disabled
            cacheDuration = 0;
            bufferResponse = true;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute1"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/> 
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession) 
            : this() {
            EnableSession = enableSession;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute2"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/> 
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession, TransactionOption transactionOption) 
            : this() {
            EnableSession = enableSession;
            this.transactionOption = (int)transactionOption;
            transactionOptionSpecified = true;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute3"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/> 
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession, TransactionOption transactionOption, int cacheDuration) {
            EnableSession = enableSession;
            this.transactionOption = (int)transactionOption;
            transactionOptionSpecified = true;
            CacheDuration = cacheDuration;
            BufferResponse = true;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute4"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/> 
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession, TransactionOption transactionOption, int cacheDuration, bool bufferResponse) {
            EnableSession = enableSession;
            this.transactionOption = (int)transactionOption;
            transactionOptionSpecified = true;
            CacheDuration = cacheDuration;
            BufferResponse = bufferResponse;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.Description"]/*' />
        /// <devdoc>
        ///    <para> A message that describes the Web service method. 
        ///       The message is used in description files generated for a Web Service, such as the Service Contract and the Service Description page.</para>
        /// </devdoc>
        public string Description {
            get {
                return (description == null) ? string.Empty : description;
            }

            set {
                description = value;
                descriptionSpecified = true;
            }
        }
        internal bool DescriptionSpecified { get { return descriptionSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.EnableSession"]/*' />
        /// <devdoc>
        ///    <para>Indicates wheter session state is enabled for a Web service Method. The default is false.</para>
        /// </devdoc>
        public bool EnableSession {
            get {
                return enableSession;
            }

            set {
                enableSession = value;
                enableSessionSpecified = true;
            }
        }
        internal bool EnableSessionSpecified { get { return enableSessionSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.CacheDuration"]/*' />
        /// <devdoc>
        ///    <para>Indicates the number of seconds the response should be cached. Defaults to 0 (no caching).
        ///          Should be used with caution when requests are likely to be very large.</para>
        /// </devdoc>
        public int CacheDuration {
            get {
                return cacheDuration;
            }

            set {
                cacheDuration = value;
                cacheDurationSpecified = true;
            }
        }
        internal bool CacheDurationSpecified { get { return cacheDurationSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.BufferResponse"]/*' />
        /// <devdoc>
        ///    <para>Indicates whether the response for this request should be buffered. Defaults to false.</para>
        /// </devdoc>
        public bool BufferResponse {
            get {
                return bufferResponse;
            }

            set {
                bufferResponse = value;
                bufferResponseSpecified = true;
            }
        }
        internal bool BufferResponseSpecified { get { return bufferResponseSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.TransactionOption"]/*' />
        /// <devdoc>
        ///    <para> 
        ///       Indicates the transaction participation mode of a Web Service Method. </para>
        /// </devdoc>
        public TransactionOption TransactionOption {            
            get {
                return (TransactionOption)transactionOption;
            }    
            set {
                transactionOption = (int)value;
                transactionOptionSpecified = true;
            }                                        
        }
        internal bool TransactionOptionSpecified { get { return transactionOptionSpecified; } }

        internal bool TransactionEnabled {
            get {
                return transactionOption != 0;
            }
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.MessageName"]/*' />
        /// <devdoc>
        ///    <para>The name used for the request and response message containing the 
        ///    data passed to and returned from this method.</para>
        /// </devdoc>
        public string MessageName {
            get {
                return messageName == null ? string.Empty : messageName;
            }

            set {
                messageName = value;
                messageNameSpecified = true;
            }
        }
        internal bool MessageNameSpecified { get { return messageNameSpecified; } }
    }

    internal class WebMethodReflector {
        private WebMethodReflector() { }
        /*
        internal static WebMethodAttribute GetAttribute(MethodInfo implementation) {
            return GetAttribute(implementation, null);
        }
        */

        internal static WebMethodAttribute GetAttribute(MethodInfo implementation, MethodInfo declaration) {
            WebMethodAttribute declAttribute = null;
            WebMethodAttribute implAttribute = null;
            object[] attrs;

            if (declaration != null) {
                attrs = declaration.GetCustomAttributes(typeof(WebMethodAttribute), false);
                if (attrs.Length > 0) {
                    declAttribute = (WebMethodAttribute)attrs[0];
                }
            }
            attrs = implementation.GetCustomAttributes(typeof(WebMethodAttribute), false);
            if (attrs.Length > 0) {
                implAttribute = (WebMethodAttribute)attrs[0];
            }
            if (declAttribute == null) {
                return implAttribute;
            }
            if (implAttribute == null) {
                return declAttribute;
            }
            if (implAttribute.MessageNameSpecified) {
                throw new InvalidOperationException(Res.GetString(Res.ContractOverride, implementation.Name, implementation.DeclaringType.FullName, declaration.DeclaringType.FullName, declaration.ToString(), "WebMethod.MessageName"));
            }
            // merge two attributes
            WebMethodAttribute attribute = new WebMethodAttribute(implAttribute.EnableSessionSpecified ? implAttribute.EnableSession : declAttribute.EnableSession);
            attribute.TransactionOption = implAttribute.TransactionOptionSpecified ? implAttribute.TransactionOption : declAttribute.TransactionOption;
            attribute.CacheDuration = implAttribute.CacheDurationSpecified ? implAttribute.CacheDuration : declAttribute.CacheDuration;
            attribute.BufferResponse = implAttribute.BufferResponseSpecified ? implAttribute.BufferResponse : declAttribute.BufferResponse;
            attribute.Description = implAttribute.DescriptionSpecified ? implAttribute.Description : declAttribute.Description;
            return attribute;
        }

        // Find the MethodInfo of the interface method from the implemented method
        internal static MethodInfo FindInterfaceMethodInfo(Type type, string signature)
        {
            Type[] interfaces = type.GetInterfaces();
            // Foreach type get the interface map and then search each TargetMethod
            // till we find the right one. Once found return the corresponding interface method 
            foreach (Type i in interfaces) {
                InterfaceMapping map = type.GetInterfaceMap(i);
                MethodInfo[] targetMethods = map.TargetMethods;
                for (int j = 0; j < targetMethods.Length; j++) {
                    if (targetMethods[j].ToString() == signature) {
                        return map.InterfaceMethods[j];
                    }
                }
            }
            return null;
        }

        internal static LogicalMethodInfo[] GetMethods(Type type) {
            if (type.IsInterface) {
                throw new InvalidOperationException(Res.GetString(Res.NeedConcreteType, type.FullName));
            }
            ArrayList list = new ArrayList();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            Hashtable unique = new Hashtable();
            Hashtable methodInfos = new Hashtable();
            for (int i = 0; i < methods.Length; i++) {
                Type declaringType = methods[i].DeclaringType;
                if (declaringType == typeof(object))
                    continue;
                if (declaringType == typeof(WebService))
                    continue;
                string signature = methods[i].ToString();
                MethodInfo declaration = FindInterfaceMethodInfo(declaringType, signature);
                WebServiceBindingAttribute binding = null;
                
                if (declaration != null) {
                    object[] attrs = declaration.DeclaringType.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
                    if (attrs.Length > 0) {
                        if (attrs.Length > 1)
                            throw new ArgumentException(Res.GetString(Res.OnlyOneWebServiceBindingAttributeMayBeSpecified1, declaration.DeclaringType.FullName), "type");
                        binding = (WebServiceBindingAttribute)attrs[0];
                        if (binding.Name == null || binding.Name.Length == 0) {
                            binding.Name = declaration.DeclaringType.Name;
                        }
                    }
                    else {
                        declaration = null;
                    }
                }
                else if (!methods[i].IsPublic) {
                    continue;
                }
                WebMethodAttribute attribute = WebMethodReflector.GetAttribute(methods[i], declaration);
                if (attribute == null)
                    continue;

                WebMethod webMethod = new WebMethod(declaration, binding, attribute);
                methodInfos.Add(methods[i], webMethod);
                MethodInfo method = (MethodInfo)unique[signature];
                if (method == null) {
                    unique.Add(signature, methods[i]);
                    list.Add(methods[i]);
                }
                else {
                    if (method.DeclaringType.IsAssignableFrom(methods[i].DeclaringType)) {
                        unique[signature] = methods[i];
                        list[list.IndexOf(method)] = methods[i];
                    }
                }
            }
            return LogicalMethodInfo.Create((MethodInfo[])list.ToArray(typeof(MethodInfo)), LogicalMethodTypes.Async | LogicalMethodTypes.Sync, methodInfos);
        }

        internal static void IncludeTypes(LogicalMethodInfo[] methods, XmlReflectionImporter importer) {
            for (int i = 0; i < methods.Length; i++) {
                LogicalMethodInfo method = methods[i];
                IncludeTypes(method, importer);
            }
        }

        internal static void IncludeTypes(LogicalMethodInfo method, XmlReflectionImporter importer) {
            if (method.Declaration != null) {
                importer.IncludeTypes(method.Declaration.DeclaringType);
                importer.IncludeTypes(method.Declaration);
            }
            importer.IncludeTypes(method.DeclaringType);
            importer.IncludeTypes(method.CustomAttributeProvider);
        }
    }

    internal class WebMethod {
        internal MethodInfo declaration;
        internal WebServiceBindingAttribute binding;
        internal WebMethodAttribute attribute;
        internal WebMethod(MethodInfo declaration, WebServiceBindingAttribute binding, WebMethodAttribute attribute) {
            this.declaration = declaration;
            this.binding = binding;
            this.attribute = attribute;
        }
    }
}
