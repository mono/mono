//------------------------------------------------------------------------------
// <copyright file="XmlReturnReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Web.Services;
    using System.Net;
    using System.Text;
    using System.Security.Policy;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Services.Diagnostics;

    internal class XmlReturn {
        private XmlReturn() { }
        internal static object[] GetInitializers(LogicalMethodInfo[] methodInfos) {
            if (methodInfos.Length == 0) return new object[0];
            WebServiceAttribute serviceAttribute = WebServiceReflector.GetAttribute(methodInfos);
            bool serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(WebServiceReflector.GetMostDerivedType(methodInfos));
            XmlReflectionImporter importer = SoapReflector.CreateXmlImporter(serviceAttribute.Namespace, serviceDefaultIsEncoded);
            WebMethodReflector.IncludeTypes(methodInfos, importer);
            ArrayList mappings = new ArrayList();
            bool[] supported = new bool[methodInfos.Length];
            for (int i = 0; i < methodInfos.Length; i++) {
                LogicalMethodInfo methodInfo = methodInfos[i];
                Type type = methodInfo.ReturnType;
                if (IsSupported(type) && HttpServerProtocol.AreUrlParametersSupported(methodInfo)) {
                    XmlAttributes a = new XmlAttributes(methodInfo.ReturnTypeCustomAttributeProvider);
                    XmlTypeMapping mapping = importer.ImportTypeMapping(type, a.XmlRoot);
                    mapping.SetKey(methodInfo.GetKey() + ":Return");
                    mappings.Add(mapping);
                    supported[i] = true;
                }
            }
            if (mappings.Count == 0)
                return new object[0];

            XmlMapping[] xmlMappings = (XmlMapping[])mappings.ToArray(typeof(XmlMapping));
            Evidence evidence = GetEvidenceForType(methodInfos[0].DeclaringType);

            TraceMethod caller = Tracing.On ? new TraceMethod(typeof(XmlReturn), "GetInitializers", methodInfos) : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceCreateSerializer), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", xmlMappings, evidence));
            XmlSerializer[] serializers = null;
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                serializers = XmlSerializer.FromMappings(xmlMappings);
            }
            else
            {
#pragma warning disable 618 // If we're in a non-homogenous domain, legacy CAS mode is enabled, so passing through evidence will not fail
                serializers = XmlSerializer.FromMappings(xmlMappings, evidence);
#pragma warning restore 618
            }

            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceCreateSerializer), caller);

            object[] initializers = new object[methodInfos.Length];
            int count = 0;
            for (int i = 0; i < initializers.Length; i++) {
                if (supported[i]) {
                    initializers[i] = serializers[count++];
                }
            }
            return initializers;
        }

        static bool IsSupported(Type returnType) {
            return returnType != typeof(void);
        }

        internal static object GetInitializer(LogicalMethodInfo methodInfo) {
            return GetInitializers(new LogicalMethodInfo[] { methodInfo });
        }

        // Asserts full-trust permission-set.
        // Reason: Assembly.Evidence demands SecurityPermission and/or other permissions.
        // Justification: The type returned is only used to get the GetInitializers method.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static Evidence GetEvidenceForType(Type type)
        {
            return type.Assembly.Evidence;
        }
    }

    /// <include file='doc\XmlReturnReader.uex' path='docs/doc[@for="XmlReturnReader"]/*' />
    public class XmlReturnReader : MimeReturnReader {
        XmlSerializer xmlSerializer;

        /// <include file='doc\XmlReturnReader.uex' path='docs/doc[@for="XmlReturnReader.Initialize"]/*' />
        public override void Initialize(object o) {
            xmlSerializer = (XmlSerializer)o;
        }

        /// <include file='doc\XmlReturnReader.uex' path='docs/doc[@for="XmlReturnReader.GetInitializers"]/*' />
        public override object[] GetInitializers(LogicalMethodInfo[] methodInfos) {
            return XmlReturn.GetInitializers(methodInfos);
        }

        /// <include file='doc\XmlReturnReader.uex' path='docs/doc[@for="XmlReturnReader.GetInitializer"]/*' />
        public override object GetInitializer(LogicalMethodInfo methodInfo) {
            return XmlReturn.GetInitializer(methodInfo);
        }

        /// <include file='doc\XmlReturnReader.uex' path='docs/doc[@for="XmlReturnReader.Read"]/*' />
        public override object Read(WebResponse response, Stream responseStream) {
            try {
                if (response == null) throw new ArgumentNullException("response");
                if (!ContentType.MatchesBase(response.ContentType, ContentType.TextXml)) {
                    throw new InvalidOperationException(Res.GetString(Res.WebResultNotXml));
                }
                Encoding e = RequestResponseUtils.GetEncoding(response.ContentType);
                StreamReader reader = new StreamReader(responseStream, e, true);
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "Read") : null;
                if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceReadResponse), caller, new TraceMethod(xmlSerializer, "Deserialize", reader));
                object returnValue = xmlSerializer.Deserialize(reader);
                if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceReadResponse), caller);
                return returnValue;
            }
            finally {
                response.Close();
            }
        }
    }
}
