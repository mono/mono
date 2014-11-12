// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 **
 ** Class: BinaryFormatter
 **
 **
 ** Purpose: Soap XML Formatter
 **
 **
 ===========================================================*/

namespace System.Runtime.Serialization.Formatters.Binary {

    using System;
    using System.IO;
    using System.Reflection;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters;
#if FEATURE_REMOTING    
    using System.Runtime.Remoting.Proxies;
#endif
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;

    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Diagnostics.Contracts;
    
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class BinaryFormatter :
#if !FEATURE_REMOTING
        IFormatter
#else
        IRemotingFormatter 
#endif
    {

        internal ISurrogateSelector m_surrogates;
        internal StreamingContext m_context;
        internal SerializationBinder m_binder;
        //internal FormatterTypeStyle m_typeFormat = FormatterTypeStyle.TypesWhenNeeded;
        internal FormatterTypeStyle m_typeFormat = FormatterTypeStyle.TypesAlways; // For version resiliency, always put out types
        internal FormatterAssemblyStyle m_assemblyFormat = FormatterAssemblyStyle.Simple;
        internal TypeFilterLevel m_securityLevel = TypeFilterLevel.Full;
        internal Object[] m_crossAppDomainArray = null;
        private static Dictionary<Type, TypeInformation> typeNameCache = new Dictionary<Type, TypeInformation>();

        // Property which specifies how types are serialized,
        // FormatterTypeStyle Enum specifies options
        public FormatterTypeStyle TypeFormat
        {
            get { return m_typeFormat; }
            set { m_typeFormat = value; }
        }

        // Property which specifies how types are serialized,
        // FormatterAssemblyStyle Enum specifies options
        public FormatterAssemblyStyle AssemblyFormat
        {
            get { return m_assemblyFormat; }
            set { m_assemblyFormat = value; }
        }

        // Property which specifies the security level of formatter
        // TypeFilterLevel Enum specifies options
        public TypeFilterLevel FilterLevel
        {
            get { return m_securityLevel; }
            set { m_securityLevel = value; }
        }

        public ISurrogateSelector SurrogateSelector {
            get { return m_surrogates; }
            set { m_surrogates = value; }
        }

        public SerializationBinder Binder {
            get { return m_binder; }
            set { m_binder = value; }
        }

        public StreamingContext Context {
            get { return m_context; }
            set { m_context = value; }
        }

        // Constructor
        public BinaryFormatter()
        {
            m_surrogates = null;
            m_context = new StreamingContext(StreamingContextStates.All);
        }

        // Constructor
        public BinaryFormatter(ISurrogateSelector selector, StreamingContext context) {
            m_surrogates = selector;
            m_context = context;
        }



        // Deserialize the stream into an object graph.
        public Object Deserialize(Stream serializationStream)
        {
            return Deserialize(serializationStream, null);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal Object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck)
        {
#if FEATURE_REMOTING        
            return Deserialize(serializationStream, handler, fCheck, null);
#else
            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream", Environment.GetResourceString("ArgumentNull_WithParamName", serializationStream));
            }
            Contract.EndContractBlock();

            if (serializationStream.CanSeek && (serializationStream.Length == 0))
                throw new SerializationException(Environment.GetResourceString("Serialization_Stream"));

            SerTrace.Log(this, "Deserialize Entry");
            InternalFE formatterEnums = new InternalFE();
            formatterEnums.FEtypeFormat = m_typeFormat;
            formatterEnums.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
            formatterEnums.FEassemblyFormat = m_assemblyFormat;
            formatterEnums.FEsecurityLevel = m_securityLevel;

            ObjectReader sor = new ObjectReader(serializationStream, m_surrogates, m_context, formatterEnums, m_binder);
            sor.crossAppDomainArray = m_crossAppDomainArray;
            return sor.Deserialize(handler, new __BinaryParser(serializationStream, sor), fCheck);

#endif

        }



        // Deserialize the stream into an object graph.
        [System.Security.SecuritySafeCritical]  // auto-generated
        public Object Deserialize(Stream serializationStream, HeaderHandler handler) {
            return Deserialize(serializationStream, handler, true);
        }

#if FEATURE_REMOTING
        [System.Security.SecuritySafeCritical]  // auto-generated
        public Object DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage) {
            return Deserialize(serializationStream, handler, true, methodCallMessage);
        }
#endif
        [System.Security.SecurityCritical]  // auto-generated_required
        [System.Runtime.InteropServices.ComVisible(false)]
        public Object UnsafeDeserialize(Stream serializationStream, HeaderHandler handler) {
            return Deserialize(serializationStream, handler, false);
        }

#if FEATURE_REMOTING        
        [System.Security.SecurityCritical]  // auto-generated_required
        [System.Runtime.InteropServices.ComVisible(false)]
        public Object UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage) {
            return Deserialize(serializationStream, handler, false, methodCallMessage);
        }         

        [System.Security.SecurityCritical]  // auto-generated
        internal Object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, IMethodCallMessage methodCallMessage) {
            return Deserialize(serializationStream, handler, fCheck, false/*isCrossAppDomain*/, methodCallMessage);
        }

        // Deserialize the stream into an object graph.
        [System.Security.SecurityCritical]  // auto-generated
        internal Object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage) {
            if (serializationStream==null)
            {
                throw new ArgumentNullException("serializationStream", Environment.GetResourceString("ArgumentNull_WithParamName",serializationStream));
            }
            Contract.EndContractBlock();

            if (serializationStream.CanSeek && (serializationStream.Length == 0))
                throw new SerializationException(Environment.GetResourceString("Serialization_Stream"));

            SerTrace.Log(this, "Deserialize Entry");
            InternalFE formatterEnums = new InternalFE();
            formatterEnums.FEtypeFormat = m_typeFormat;
            formatterEnums.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
            formatterEnums.FEassemblyFormat = m_assemblyFormat;         
            formatterEnums.FEsecurityLevel = m_securityLevel;            

               ObjectReader sor = new ObjectReader(serializationStream, m_surrogates, m_context, formatterEnums, m_binder);
            sor.crossAppDomainArray = m_crossAppDomainArray;
            return sor.Deserialize(handler, new __BinaryParser(serializationStream, sor), fCheck, isCrossAppDomain, methodCallMessage);
    }
#endif // FEATURE_REMOTING

        public void Serialize(Stream serializationStream, Object graph)
        {
            Serialize(serializationStream, graph, null);
        }

        // Commences the process of serializing the entire graph.  All of the data (in the appropriate format
        // is emitted onto the stream).
        [System.Security.SecuritySafeCritical]  // auto-generated
        public void Serialize(Stream serializationStream, Object graph, Header[] headers)
        {
            Serialize(serializationStream, graph, headers, true);
        }

        // Commences the process of serializing the entire graph.  All of the data (in the appropriate format
        // is emitted onto the stream).
        [System.Security.SecurityCritical]  // auto-generated
        internal void Serialize(Stream serializationStream, Object graph, Header[] headers, bool fCheck)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream", Environment.GetResourceString("ArgumentNull_WithParamName", serializationStream));
            }
            Contract.EndContractBlock();
            SerTrace.Log(this, "Serialize Entry");

            InternalFE formatterEnums = new InternalFE();
            formatterEnums.FEtypeFormat = m_typeFormat;
            formatterEnums.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
            formatterEnums.FEassemblyFormat = m_assemblyFormat;

            ObjectWriter sow = new ObjectWriter(m_surrogates, m_context, formatterEnums, m_binder);
            __BinaryWriter binaryWriter = new __BinaryWriter(serializationStream, sow, m_typeFormat);
            sow.Serialize(graph, headers, binaryWriter, fCheck);
            m_crossAppDomainArray = sow.crossAppDomainArray;
        }

        internal static TypeInformation GetTypeInformation(Type type)
        {
            lock (typeNameCache)
            {
                TypeInformation typeInformation = null;
                if (!typeNameCache.TryGetValue(type, out typeInformation))
                {
                    bool hasTypeForwardedFrom;
                    string assemblyName = FormatterServices.GetClrAssemblyName(type, out hasTypeForwardedFrom);
                    typeInformation = new TypeInformation(FormatterServices.GetClrTypeFullName(type), assemblyName, hasTypeForwardedFrom);
                    typeNameCache.Add(type, typeInformation);
                }
                return typeInformation;
            }
        }
    }
}
