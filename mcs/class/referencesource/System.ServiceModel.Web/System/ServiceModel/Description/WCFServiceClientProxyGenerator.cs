//------------------------------------------------------------------------------
// <copyright file="WCFServiceClientProxyGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Collections.Generic;
    using System.Xml;
    using System.Collections;
    using System.Web.Script.Services;

    internal class WCFServiceClientProxyGenerator : ClientProxyGenerator 
    {
        const int MaxIdentifierLength = 511;
        const string DataContractXsdBaseNamespace = @"http://schemas.datacontract.org/2004/07/";
        const string DefaultCallbackParameterName = "callback";
        private string path;
        private ServiceEndpoint serviceEndpoint;

        // Similar to proxy generation code in WCF System.Runtime.Serialization.CodeExporter
        // to generate CLR namespace from DataContract namespace
        private static void AddToNamespace(StringBuilder builder, string fragment) 
        {
            if (fragment == null) 
            {
                return;
            }
            bool isStart = true;

            for (int i = 0; i < fragment.Length && builder.Length < MaxIdentifierLength; i++) 
            {
                char c = fragment[i];

                if (IsValid(c)) 
                {
                    if (isStart && !IsValidStart(c)) 
                    {
                        builder.Append("_");
                    }
                    builder.Append(c);
                    isStart = false;
                }
                else if ((c == '.' || c == '/' || c == ':') && (builder.Length == 1
                    || (builder.Length > 1 && builder[builder.Length - 1] != '.'))) 
                {
                    builder.Append('.');
                    isStart = true;
                }
            }
        }

        protected override string GetProxyPath() 
        {
            return this.path;
        }
        
        protected override string GetJsonpCallbackParameterName() 
        {
            if (this.serviceEndpoint == null) 
            {
                return null;
            }
            WebMessageEncodingBindingElement webEncodingBindingElement = this.serviceEndpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>();
            if (webEncodingBindingElement != null && webEncodingBindingElement.CrossDomainScriptAccessEnabled)
            {
                if (this.serviceEndpoint.Contract.Behaviors.Contains(typeof(JavascriptCallbackBehaviorAttribute)))
                {
                    JavascriptCallbackBehaviorAttribute behavior = (JavascriptCallbackBehaviorAttribute)this.serviceEndpoint.Contract.Behaviors[typeof(JavascriptCallbackBehaviorAttribute)];
                    return behavior.UrlParameterName;
                }
                return DefaultCallbackParameterName;
            }
            return null;
        }

        protected override bool GetSupportsJsonp() 
        {
            return !String.IsNullOrEmpty(GetJsonpCallbackParameterName());
        }

        private static Type ReplaceMessageWithObject(Type t)
        {
            return (typeof(Message).IsAssignableFrom(t)) ? typeof(object) : t;
        }

        static WebServiceData GetWebServiceData(ContractDescription contract) 
        {
            WebServiceData serviceData = new WebServiceData();

            //build method dictionary
            Dictionary<string, WebServiceMethodData> methodDataDictionary = new Dictionary<string, WebServiceMethodData>();
            
            // set service type
            serviceData.Initialize(new WebServiceTypeData(XmlConvert.DecodeName(contract.Name), XmlConvert.DecodeName(contract.Namespace), contract.ContractType),
                methodDataDictionary);

            foreach (OperationDescription operation in contract.Operations) 
            {
                Dictionary<string, WebServiceParameterData> parameterDataDictionary = new Dictionary<string, WebServiceParameterData>();
                bool useHttpGet = operation.Behaviors.Find<WebGetAttribute>() != null;
                WebServiceMethodData methodData = new WebServiceMethodData(serviceData, XmlConvert.DecodeName(operation.Name), parameterDataDictionary, useHttpGet);
                // build parameter dictionary
                MessageDescription requestMessage = operation.Messages[0];
                if (requestMessage != null) 
                {
                    int numMessageParts = requestMessage.Body.Parts.Count;
                    for (int p = 0; p < numMessageParts; p++) 
                    {
                        MessagePartDescription messagePart = requestMessage.Body.Parts[p];
                        // DevDiv 129964:JS proxy generation fails for a WCF service that uses an untyped message
                        // Message or its derived class are special, used for untyped operation contracts. 
                        // As per the WCF team proxy generated for them should treat Message equivalent to Object type.
                        Type paramType = ReplaceMessageWithObject(messagePart.Type);
                        WebServiceParameterData parameterData = new WebServiceParameterData(XmlConvert.DecodeName(messagePart.Name), paramType, p);
                        parameterDataDictionary[parameterData.ParameterName] = parameterData;
                        serviceData.ProcessClientType(paramType, false, true);
                    }
                }
                if (operation.Messages.Count > 1) 
                {
                    // its a two way operation, get type information from return message
                    MessageDescription responseMessage = operation.Messages[1];
                    if (responseMessage != null) 
                    {
                        if (responseMessage.Body.ReturnValue != null && responseMessage.Body.ReturnValue.Type != null) 
                        {
                            // operation has a return type, add type to list of type proxy to generate
                            serviceData.ProcessClientType(ReplaceMessageWithObject(responseMessage.Body.ReturnValue.Type), false, true);
                        }
                    }
                }

                //add known types at operation level
                for (int t = 0; t < operation.KnownTypes.Count; t++) 
                {
                    serviceData.ProcessClientType(operation.KnownTypes[t], false, true);
                }

                methodDataDictionary[methodData.MethodName] = methodData;
            }
            serviceData.ClearProcessedTypes();
            return serviceData;

        }

        internal static string GetClientProxyScript(Type contractType, string path, bool debugMode, ServiceEndpoint serviceEndpoint) 
        {
            ContractDescription contract = ContractDescription.GetContract(contractType);
            WebServiceData webServiceData = GetWebServiceData(contract);
            WCFServiceClientProxyGenerator proxyGenerator = new WCFServiceClientProxyGenerator(path, debugMode, serviceEndpoint);
            return proxyGenerator.GetClientProxyScript(webServiceData);
        }

        // Similar to proxy generation code in WCF System.Runtime.Serialization.CodeExporter
        // to generate CLR namespace from DataContract namespace
        protected override string GetClientTypeNamespace(string ns) 
        {
            if (string.IsNullOrEmpty(ns)) 
            {
                return String.Empty;
            }

            Uri uri = null;
            StringBuilder builder = new StringBuilder();
            if (Uri.TryCreate(ns, UriKind.RelativeOrAbsolute, out uri)) 
            {
                if (!uri.IsAbsoluteUri) 
                {
                    AddToNamespace(builder, uri.OriginalString);
                }
                else 
                {
                    string uriString = uri.AbsoluteUri;
                    if (uriString.StartsWith(DataContractXsdBaseNamespace, StringComparison.Ordinal)) 
                    {
                        AddToNamespace(builder, uriString.Substring(DataContractXsdBaseNamespace.Length));
                    }
                    else 
                    {
                        string host = uri.Host;
                        if (host != null) 
                        {
                            AddToNamespace(builder, host);
                        }
                        string path = uri.PathAndQuery;
                        if (path != null) 
                        {
                            AddToNamespace(builder, path);
                        }
                    }
                }
            }

            if (builder.Length == 0) 
            {
                return String.Empty;
            }

            int length = builder.Length;
            if (builder[builder.Length - 1] == '.') 
            {
                length--;
            }
            length = Math.Min(MaxIdentifierLength, length);

            return builder.ToString(0, length);
        }

        protected override string GetProxyTypeName(WebServiceData data) 
        {
            return GetClientTypeNamespace(data.TypeData.TypeName);
        }

        // Similar to proxy generation code in WCF System.Runtime.Serialization.CodeExporter
        // to generate CLR namespace from DataContract namespace
        private static bool IsValid(char c) 
        {
            UnicodeCategory uc = Char.GetUnicodeCategory(c);

            // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc

            switch (uc) 
            {
                case UnicodeCategory.UppercaseLetter: // Lu
                case UnicodeCategory.LowercaseLetter: // Ll
                case UnicodeCategory.TitlecaseLetter: // Lt
                case UnicodeCategory.ModifierLetter: // Lm
                case UnicodeCategory.OtherLetter: // Lo
                case UnicodeCategory.DecimalDigitNumber: // Nd
                case UnicodeCategory.NonSpacingMark: // Mn
                case UnicodeCategory.SpacingCombiningMark: // Mc
                case UnicodeCategory.ConnectorPunctuation: // Pc
                    return true;
                default:
                    return false;
            }
        }

        // Similar to proxy generation code in WCF System.Runtime.Serialization.CodeExporter
        // to generate CLR namespace from DataContract namespace
        private static bool IsValidStart(char c) 
        {
            return (Char.GetUnicodeCategory(c) != UnicodeCategory.DecimalDigitNumber);
        }

        internal WCFServiceClientProxyGenerator(string path, bool debugMode, ServiceEndpoint serviceEndpoint) 
        {
            this.path = path;
            _debugMode = debugMode;
            this.serviceEndpoint = serviceEndpoint;
        }
    }
}
