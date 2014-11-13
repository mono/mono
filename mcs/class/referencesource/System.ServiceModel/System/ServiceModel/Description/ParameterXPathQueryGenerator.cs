//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Xml.Linq;
    using System.Xml;
    using System.Text;
    using System.Runtime.Serialization;
    using System.ServiceModel.Description;
    using System.Reflection;

    public static class ParameterXPathQueryGenerator
    {
        const string XPathSeparator = "/";
        const string NsSeparator = ":";
        const string ServiceContractPrefix = "xgSc";

        public static string CreateFromDataContractSerializer(XName serviceContractName, string operationName, string parameterName, bool isReply, Type type, MemberInfo[] pathToMember, out XmlNamespaceManager namespaces)
        {
            if (type == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("type"));
            }
            if (pathToMember == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pathToMember"));
            }
            if (operationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("operationName"));
            }
            if (serviceContractName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceContractName"));
            }
            if (isReply)
            {
                operationName += TypeLoader.ResponseSuffix;
            }

            StringBuilder xPathBuilder = new StringBuilder(XPathSeparator + ServiceContractPrefix + NsSeparator + operationName);
            xPathBuilder.Append(XPathSeparator + ServiceContractPrefix + NsSeparator + parameterName);

            string xpath = XPathQueryGenerator.CreateFromDataContractSerializer(type, pathToMember, xPathBuilder, out namespaces);
            string serviceContractNamespace = serviceContractName.NamespaceName;
            // Use default service contract namespace if the provided serviceContractNamespace is null or empty
            if (string.IsNullOrEmpty(serviceContractNamespace))
            {
                serviceContractNamespace = NamingHelper.DefaultNamespace;
            }
            namespaces.AddNamespace(ServiceContractPrefix, serviceContractNamespace);

            return xpath;
        }
    }
}
