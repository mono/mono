// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 **
 ** Class: CommonBinaryClasses
 **
 **
 ** Purpose: utility classes
 **
 **
 ===========================================================*/


namespace System.Runtime.Serialization.Formatters.Binary{

    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.Collections;
    using System.Reflection;
#if FEATURE_REMOTING    
    using System.Runtime.Remoting.Messaging;
#endif
    using System.Diagnostics;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    // Routines to convert between the runtime type and the type as it appears on the wire
    internal static class BinaryConverter
    {

        // From the type create the BinaryTypeEnum and typeInformation which describes the type on the wire

        internal static BinaryTypeEnum GetBinaryTypeInfo(Type type, WriteObjectInfo objectInfo, String typeName, ObjectWriter objectWriter, out Object typeInformation, out int assemId)
        {
            SerTrace.Log("BinaryConverter", "GetBinaryTypeInfo Entry type ",type,", typeName ",typeName," objectInfo "+objectInfo);     
            BinaryTypeEnum binaryTypeEnum;

            assemId = 0;
            typeInformation = null;

            if (Object.ReferenceEquals(type, Converter.typeofString))
                binaryTypeEnum = BinaryTypeEnum.String;
            else if (((objectInfo == null) || ((objectInfo != null) && !objectInfo.isSi))
                     && (Object.ReferenceEquals(type, Converter.typeofObject)))
            {
                // If objectInfo.Si then can be a surrogate which will change the type
                binaryTypeEnum = BinaryTypeEnum.Object;
            }
            else if (Object.ReferenceEquals(type, Converter.typeofStringArray))
                binaryTypeEnum = BinaryTypeEnum.StringArray;
            else if (Object.ReferenceEquals(type, Converter.typeofObjectArray))
                binaryTypeEnum = BinaryTypeEnum.ObjectArray;
            else if (Converter.IsPrimitiveArray(type, out typeInformation))
                binaryTypeEnum = BinaryTypeEnum.PrimitiveArray;
            else
            {
                InternalPrimitiveTypeE primitiveTypeEnum = objectWriter.ToCode(type);
                switch (primitiveTypeEnum)
                {
                    case InternalPrimitiveTypeE.Invalid:
                        String assembly = null;
                        if (objectInfo == null)
                        {
                            assembly = type.Assembly.FullName;
                            typeInformation = type.FullName;
                        }
                        else
                        {
                            assembly = objectInfo.GetAssemblyString();
                            typeInformation = objectInfo.GetTypeFullName();
                        }

                        if (assembly.Equals(Converter.urtAssemblyString))
                        {
                            binaryTypeEnum = BinaryTypeEnum.ObjectUrt;
                            assemId = 0;
                        }
                        else
                        {
                            binaryTypeEnum = BinaryTypeEnum.ObjectUser;
                            Contract.Assert(objectInfo!=null, "[BinaryConverter.GetBinaryTypeInfo]objectInfo null for user object");
                            assemId = (int)objectInfo.assemId;
                            if (assemId == 0)
                                throw new SerializationException(Environment.GetResourceString("Serialization_AssemblyId",typeInformation));
                        }
                        break;
                    default:
                        binaryTypeEnum = BinaryTypeEnum.Primitive;
                        typeInformation = primitiveTypeEnum;
                        break;
                }
            }

            SerTrace.Log( "BinaryConverter", "GetBinaryTypeInfo Exit ",((Enum)binaryTypeEnum).ToString(),", typeInformation ",typeInformation," assemId ",assemId);             
            return binaryTypeEnum;
        }


        // Used for non Si types when Parsing
        internal static BinaryTypeEnum GetParserBinaryTypeInfo(Type type, out Object typeInformation)
        {
            SerTrace.Log("BinaryConverter", "GetParserBinaryTypeInfo Entry type ",type);        
            BinaryTypeEnum binaryTypeEnum;
            typeInformation = null;

            if (Object.ReferenceEquals(type, Converter.typeofString))
                binaryTypeEnum = BinaryTypeEnum.String;
            else if (Object.ReferenceEquals(type, Converter.typeofObject))
                binaryTypeEnum = BinaryTypeEnum.Object;
            else if (Object.ReferenceEquals(type, Converter.typeofObjectArray))
                binaryTypeEnum = BinaryTypeEnum.ObjectArray;
            else if (Object.ReferenceEquals(type, Converter.typeofStringArray))
                binaryTypeEnum = BinaryTypeEnum.StringArray;
            else if (Converter.IsPrimitiveArray(type, out typeInformation))
                binaryTypeEnum = BinaryTypeEnum.PrimitiveArray;
            else
            {
                InternalPrimitiveTypeE primitiveTypeEnum = Converter.ToCode(type);
                switch (primitiveTypeEnum)
                {
                    case InternalPrimitiveTypeE.Invalid:
                        if (Assembly.GetAssembly(type) == Converter.urtAssembly)
                            binaryTypeEnum = BinaryTypeEnum.ObjectUrt;
                        else
                            binaryTypeEnum = BinaryTypeEnum.ObjectUser;

                        typeInformation = type.FullName;
                        break;
                    default:
                        binaryTypeEnum = BinaryTypeEnum.Primitive;
                        typeInformation = primitiveTypeEnum;
                        break;
                }
            }

            SerTrace.Log( "BinaryConverter", "GetParserBinaryTypeInfo Exit ",((Enum)binaryTypeEnum).ToString(),", typeInformation ",typeInformation);               
            return binaryTypeEnum;
        }

        // Writes the type information on the wire
        internal static void WriteTypeInfo(BinaryTypeEnum binaryTypeEnum, Object typeInformation, int assemId, __BinaryWriter sout)
        {
            SerTrace.Log( "BinaryConverter", "WriteTypeInfo Entry  ",((Enum)binaryTypeEnum).ToString()," ",typeInformation," assemId ",assemId);

            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                case BinaryTypeEnum.PrimitiveArray:
                    Contract.Assert(typeInformation!=null, "[BinaryConverter.WriteTypeInfo]typeInformation!=null");
                    sout.WriteByte((Byte)((InternalPrimitiveTypeE)typeInformation));                    
                    break;
                case BinaryTypeEnum.String:
                case BinaryTypeEnum.Object:
                case BinaryTypeEnum.StringArray:
                case BinaryTypeEnum.ObjectArray:
                    break;                    
                case BinaryTypeEnum.ObjectUrt:
                    Contract.Assert(typeInformation!=null, "[BinaryConverter.WriteTypeInfo]typeInformation!=null");
                    sout.WriteString(typeInformation.ToString());
                    break;
                case BinaryTypeEnum.ObjectUser:                             
                    Contract.Assert(typeInformation!=null, "[BinaryConverter.WriteTypeInfo]typeInformation!=null");
                    sout.WriteString(typeInformation.ToString());
                    sout.WriteInt32(assemId);
                    break;                    
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_TypeWrite",((Enum)binaryTypeEnum).ToString()));
            }
            SerTrace.Log( "BinaryConverter", "WriteTypeInfo Exit");
        }

        // Reads the type information from the wire
        internal static Object ReadTypeInfo(BinaryTypeEnum binaryTypeEnum, __BinaryParser input, out int assemId)
        {
            SerTrace.Log( "BinaryConverter", "ReadTypeInfo Entry  ",((Enum)binaryTypeEnum).ToString());
            Object var = null;
            int readAssemId = 0;

            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                case BinaryTypeEnum.PrimitiveArray:
                    var = (InternalPrimitiveTypeE)input.ReadByte();
                    break;
                case BinaryTypeEnum.String:
                case BinaryTypeEnum.Object:
                case BinaryTypeEnum.StringArray:
                case BinaryTypeEnum.ObjectArray:
                    break;                    
                case BinaryTypeEnum.ObjectUrt:
                    var = input.ReadString();                   
                    break;
                case BinaryTypeEnum.ObjectUser:
                    var = input.ReadString();
                    readAssemId = input.ReadInt32();
                    break;                    
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_TypeRead",((Enum)binaryTypeEnum).ToString()));                 
            }
            SerTrace.Log( "BinaryConverter", "ReadTypeInfo Exit  ",var," assemId ",readAssemId);
            assemId = readAssemId;
            return var;
        }

        // Given the wire type information, returns the actual type and additional information
        [System.Security.SecurityCritical]  // auto-generated
        internal static void TypeFromInfo(BinaryTypeEnum binaryTypeEnum,
                                          Object typeInformation,
                                          ObjectReader objectReader,
                                          BinaryAssemblyInfo assemblyInfo,
                                          out InternalPrimitiveTypeE primitiveTypeEnum,
                                          out String typeString,
                                          out Type type,
                                          out bool isVariant)
        {
            SerTrace.Log( "BinaryConverter", "TypeFromInfo Entry  ",((Enum)binaryTypeEnum).ToString());

            isVariant = false;
            primitiveTypeEnum = InternalPrimitiveTypeE.Invalid;
            typeString = null;
            type = null;

            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                    primitiveTypeEnum = (InternalPrimitiveTypeE)typeInformation;                    
                    typeString = Converter.ToComType(primitiveTypeEnum);
                    type = Converter.ToType(primitiveTypeEnum);
                    break;
                case BinaryTypeEnum.String:
                    //typeString = "System.String";
                    type = Converter.typeofString;
                    break;
                case BinaryTypeEnum.Object:
                    //typeString = "System.Object";
                    type = Converter.typeofObject;
                    isVariant = true; 
                    break;
                case BinaryTypeEnum.ObjectArray:
                    //typeString = "System.Object[]";
                    type = Converter.typeofObjectArray;
                    break;
                case BinaryTypeEnum.StringArray:
                    //typeString = "System.String[]";
                    type = Converter.typeofStringArray;
                    break;
                case BinaryTypeEnum.PrimitiveArray:
                    primitiveTypeEnum = (InternalPrimitiveTypeE)typeInformation;                    
                    type = Converter.ToArrayType(primitiveTypeEnum);
                    break;
                case BinaryTypeEnum.ObjectUser:
                case BinaryTypeEnum.ObjectUrt:
                    if (typeInformation != null)
                    {
                        typeString = typeInformation.ToString();
                        type = objectReader.GetType(assemblyInfo, typeString);
                        // Temporary for backward compatibility
                        if (Object.ReferenceEquals(type, Converter.typeofObject))
                            isVariant = true;
                    }
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_TypeRead",((Enum)binaryTypeEnum).ToString()));                                     
            }

#if _DEBUG
                SerTrace.Log( "BinaryConverter", "TypeFromInfo Exit  "
                          ,((Enum)primitiveTypeEnum).ToString(),",typeString ",Util.PString(typeString)
                          ,", type ",Util.PString(type),", isVariant ",isVariant);      
#endif

        }

#if _DEBUG                        
         // Used to write type type on the record dump
        internal static String TypeInfoTraceString(Object typeInformation)
        {
            String traceString = null;
            if (typeInformation == null)
                traceString = "(Null)";
            else if (typeInformation is String)
                traceString = "(UTF)";
            else
                traceString = "(Byte)";
            return traceString;
        }
#endif

    }

    internal static class IOUtil
    {
        internal static bool FlagTest(MessageEnum flag, MessageEnum target)
        {
            if ((flag & target) == target)
                return true;
            else
                return false;
        }

        internal static void WriteStringWithCode(String value, __BinaryWriter sout)
        {
            if (value == null)
                sout.WriteByte((Byte)InternalPrimitiveTypeE.Null);
            else
            {
                sout.WriteByte((Byte)InternalPrimitiveTypeE.String);
                sout.WriteString(value);
            }
        }

        internal static void WriteWithCode(Type type, Object value, __BinaryWriter sout)
        {
            if ((object)type == null)
                sout.WriteByte((Byte)InternalPrimitiveTypeE.Null);
            else if (Object.ReferenceEquals(type, Converter.typeofString))
                WriteStringWithCode((String)value, sout);
            else
            {
                InternalPrimitiveTypeE code = Converter.ToCode(type);
                sout.WriteByte((Byte)code);
                sout.WriteValue(code, value);
            }
        }

        internal static Object ReadWithCode(__BinaryParser input)
        {
             InternalPrimitiveTypeE code = (InternalPrimitiveTypeE)input.ReadByte();
             if (code == InternalPrimitiveTypeE.Null)
                 return null;
             else if (code == InternalPrimitiveTypeE.String)
                 return input.ReadString();
             else
                 return input.ReadValue(code);
        }

        internal static Object[] ReadArgs(__BinaryParser input)
        {
            int length = input.ReadInt32();
            Object[] args = new Object[length];
            for (int i=0; i<length; i++)
                args[i] = ReadWithCode(input);
            return args;
        }

    }


    internal static class BinaryUtil
    {
        [Conditional("_LOGGING")]                               
        public static void NVTraceI(String name, String value)
        {
            if (BCLDebug.CheckEnabled("BINARY"))
                BCLDebug.Trace("BINARY", "  ",name, " = ", value);
        }

        // Traces an name value pair
        [Conditional("_LOGGING")]                                       
        public static void NVTraceI(String name, Object value)
        {
            if (BCLDebug.CheckEnabled("BINARY"))
                BCLDebug.Trace("BINARY", "  ",name, " = ", value);
        }

    }


    // Interface for Binary Records.
    internal interface IStreamable
    {
        [System.Security.SecurityCritical]
        void Read(__BinaryParser input);
        void Write(__BinaryWriter sout);
#if _DEBUG        
        void Dump();
#endif
    }

    internal sealed class BinaryAssemblyInfo
    {
        internal String assemblyString;
        private Assembly assembly;


        internal BinaryAssemblyInfo(String assemblyString)
        {
            this.assemblyString = assemblyString;
        }

        internal BinaryAssemblyInfo(String assemblyString, Assembly assembly)
        {
            this.assemblyString = assemblyString;
            this.assembly = assembly;
        }

        internal Assembly GetAssembly()
        {
            if (assembly == null)
            {
                assembly = FormatterServices.LoadAssemblyFromStringNoThrow(assemblyString);
                if (assembly == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_AssemblyNotFound",assemblyString));
            }
            return assembly;
        }
    }

    // The Following classes read and write the binary records
    internal sealed class SerializationHeaderRecord : IStreamable
    {
        internal Int32 binaryFormatterMajorVersion = 1;
        internal Int32 binaryFormatterMinorVersion = 0;
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal Int32 topId;
        internal Int32 headerId;
        internal Int32 majorVersion;
        internal Int32 minorVersion;

        internal SerializationHeaderRecord()
        {
        }

        internal SerializationHeaderRecord(BinaryHeaderEnum binaryHeaderEnum, Int32 topId, Int32 headerId, Int32 majorVersion, Int32 minorVersion)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
            this.topId = topId;
            this.headerId = headerId;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
        }

        public  void Write(__BinaryWriter sout)
        {
            majorVersion = binaryFormatterMajorVersion;
            minorVersion = binaryFormatterMinorVersion;
            sout.WriteByte((Byte)binaryHeaderEnum);
            sout.WriteInt32(topId);
            sout.WriteInt32(headerId);
            sout.WriteInt32(binaryFormatterMajorVersion);
            sout.WriteInt32(binaryFormatterMinorVersion);      
        }

        private static int GetInt32(byte [] buffer, int index)
        {
            return (int)(buffer[index] | buffer[index+1] << 8 | buffer[index+2] << 16 | buffer[index+3] << 24);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public  void Read(__BinaryParser input)
        {
            byte [] headerBytes = input.ReadBytes(17);
            // Throw if we couldnt read header bytes
            if (headerBytes.Length < 17)
                __Error.EndOfFile();
            
            majorVersion = GetInt32(headerBytes, 9);
            if (majorVersion > binaryFormatterMajorVersion)
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFormat", BitConverter.ToString(headerBytes)));
            
            // binaryHeaderEnum has already been read
            binaryHeaderEnum = (BinaryHeaderEnum)headerBytes[0];
            topId = GetInt32(headerBytes, 1);
            headerId = GetInt32(headerBytes, 5);
            minorVersion = GetInt32(headerBytes, 13);
        }

        public  void Dump()
        {
            DumpInternal();
        }


        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY", "*****SerializationHeaderRecord*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                BinaryUtil.NVTraceI("topId (Int32)", topId);
                BinaryUtil.NVTraceI("headerId (Int32)", headerId);
                BinaryUtil.NVTraceI("majorVersion (Int32)", majorVersion);
                BinaryUtil.NVTraceI("minorVersion (Int32)", minorVersion);
                BCLDebug.Trace("BINARY","***********************************");
            }
        }
    }


    internal sealed class BinaryAssembly : IStreamable
    {
        internal Int32 assemId;
        internal String assemblyString;

        internal BinaryAssembly()
        {
        }


        internal void Set(Int32 assemId, String assemblyString)
        {
            SerTrace.Log( this, "BinaryAssembly Set ",assemId," ",assemblyString);      
            this.assemId = assemId;
            this.assemblyString = assemblyString;
        }


        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.Assembly);
            sout.WriteInt32(assemId);
            sout.WriteString(assemblyString);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            assemId = input.ReadInt32();
            assemblyString = input.ReadString();
        }

        public void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryAssembly*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "Assembly");
                BinaryUtil.NVTraceI("assemId (Int32)", assemId);        
                BinaryUtil.NVTraceI("Assembly (UTF)", assemblyString);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }

    internal sealed class BinaryCrossAppDomainAssembly : IStreamable
    {
        internal Int32 assemId;
        internal Int32 assemblyIndex;

        internal BinaryCrossAppDomainAssembly()
        {
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.CrossAppDomainAssembly);
            sout.WriteInt32(assemId);
            sout.WriteInt32(assemblyIndex);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            assemId = input.ReadInt32();
            assemblyIndex = input.ReadInt32();
        }

        public void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryCrossAppDomainAssembly*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "CrossAppDomainAssembly");
                BinaryUtil.NVTraceI("assemId (Int32)", assemId);        
                BinaryUtil.NVTraceI("assemblyIndex (Int32)", assemblyIndex);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }


    internal sealed class BinaryObject : IStreamable
    {
        internal Int32 objectId;
        internal Int32 mapId;

        internal BinaryObject()
        {
        }

        internal  void Set(Int32 objectId, Int32 mapId)
        {
            SerTrace.Log( this, "BinaryObject Set ",objectId," ",mapId);        
            this.objectId = objectId;
            this.mapId = mapId;
        }


        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.Object);
            sout.WriteInt32(objectId);
            sout.WriteInt32(mapId);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            objectId = input.ReadInt32();
            mapId = input.ReadInt32();
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryObject*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "Object");
                BinaryUtil.NVTraceI("objectId (Int32)", objectId);      
                BinaryUtil.NVTraceI("mapId (Int32)", mapId);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }

    internal sealed class BinaryMethodCall
    {
        String uri;
        String methodName;
        String typeName;
        Type[] instArgs;
        Object[] args;
        Object methodSignature;
        Object callContext;
        String scallContext;
        Object properties;
        Type[] argTypes;
        bool bArgsPrimitive = true;
        MessageEnum messageEnum;
        Object[] callA;

        // If the argument list contains only primitive or strings it is written out as part of the header
        // if not the args are written out as a separate array
        internal Object[] WriteArray(String uri, String methodName, String typeName, Type[] instArgs, Object[] args, Object methodSignature, Object callContext, Object[] properties)
        {
            this.uri = uri;
            this.methodName = methodName;
            this.typeName = typeName;
            this.instArgs = instArgs;
            this.args = args;
            this.methodSignature = methodSignature;
            this.callContext = callContext;
            this.properties = properties;

            int arraySize = 0;
            if (args == null || args.Length == 0)
                messageEnum = MessageEnum.NoArgs;
            else
            {
                argTypes = new Type[args.Length];
                // Check if args are all string or primitives
                bArgsPrimitive = true;
                for (int i =0; i<args.Length; i++)
                {
                    if (args[i] != null)
                    {
                        argTypes[i] = args[i].GetType();
                        bool isArgPrimitive = Converter.ToCode(argTypes[i]) != InternalPrimitiveTypeE.Invalid;
                        if (!(isArgPrimitive || Object.ReferenceEquals(argTypes[i], Converter.typeofString)) || args[i] is ISerializable)
                        {
                            bArgsPrimitive = false;
                            break;
                        }
                    }
                }


                if (bArgsPrimitive)
                    messageEnum = MessageEnum.ArgsInline;
                else
                {
                    arraySize++;
                    messageEnum = MessageEnum.ArgsInArray;
                }
            }


            if (instArgs != null)
            {
                arraySize++;
                messageEnum |= MessageEnum.GenericMethod;
            }

            if (methodSignature != null)
            {
                arraySize++;
                messageEnum |= MessageEnum.MethodSignatureInArray;
            }

            if (callContext == null)
                messageEnum |= MessageEnum.NoContext;
            else if (callContext is String)
                messageEnum |= MessageEnum.ContextInline;
            else
            {
                arraySize++;
                messageEnum |= MessageEnum.ContextInArray;
            }

            if (properties != null)
            {
                arraySize++;
                messageEnum |= MessageEnum.PropertyInArray;
            }

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInArray) && arraySize == 1)
            {
                messageEnum ^= MessageEnum.ArgsInArray;
                messageEnum |= MessageEnum.ArgsIsArray;
                return args;
            }


            if (arraySize > 0)
            {
                int arrayPosition = 0;
                callA = new Object[arraySize];
                if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInArray))
                    callA[arrayPosition++] = args;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.GenericMethod))
                    callA[arrayPosition++] = instArgs;
                
                if (IOUtil.FlagTest(messageEnum, MessageEnum.MethodSignatureInArray))
                    callA[arrayPosition++] = methodSignature;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInArray))
                    callA[arrayPosition++] = callContext;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.PropertyInArray))
                    callA[arrayPosition] = properties;

                 return callA;
            }
            else
                return null;
        }

        internal void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.MethodCall);
            sout.WriteInt32((Int32)messageEnum);
            //IOUtil.WriteStringWithCode(uri, sout);
            IOUtil.WriteStringWithCode(methodName, sout);
            IOUtil.WriteStringWithCode(typeName, sout);
            if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInline))
                IOUtil.WriteStringWithCode((String)callContext, sout);

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInline))
            {
                sout.WriteInt32(args.Length);
                for (int i=0; i<args.Length; i++)
                {
                    IOUtil.WriteWithCode(argTypes[i], args[i], sout);
                }

            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void Read(__BinaryParser input)
        {
             messageEnum = (MessageEnum)input.ReadInt32();
             //uri = (String)IOUtil.ReadWithCode(input);
             methodName = (String)IOUtil.ReadWithCode(input);
             typeName = (String)IOUtil.ReadWithCode(input);

#if FEATURE_REMOTING
             if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInline))
             {
                 scallContext = (String)IOUtil.ReadWithCode(input);
                 LogicalCallContext lcallContext = new LogicalCallContext();
                 lcallContext.RemotingData.LogicalCallID = scallContext;
                 callContext = lcallContext;
             }
#endif             

             if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInline))
                 args = IOUtil.ReadArgs(input);
        }
#if FEATURE_REMOTING
        [System.Security.SecurityCritical]  // auto-generated
        internal IMethodCallMessage ReadArray(Object[] callA, Object handlerObject)
        {
            /*
            if (callA.Length != 7)
                throw new SerializationException(String.Format(Environment.GetResourceString("Serialization_Method")));
                */

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsIsArray))
            {
                args = callA;
            }
            else
            {
                int arrayPosition = 0;
                
                if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInArray))
                {
                    if (callA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    args = (Object[])callA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.GenericMethod))
                {
                    if (callA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    instArgs = (Type[])callA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.MethodSignatureInArray))
                {
                    if (callA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    methodSignature = callA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInArray))
                {
                    if (callA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    callContext = callA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.PropertyInArray))
                {
                    if (callA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    properties = callA[arrayPosition++];
                }
            }

            return new MethodCall(handlerObject, new BinaryMethodCallMessage(uri, methodName, typeName, instArgs, args, methodSignature, (LogicalCallContext)callContext, (Object[])properties));
        }
#endif // FEATURE_REMOTING
        internal void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryMethodCall*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "MethodCall");
                BinaryUtil.NVTraceI("messageEnum (Int32)", ((Enum)messageEnum).ToString());
                //BinaryUtil.NVTraceI("uri",uri);
                BinaryUtil.NVTraceI("methodName",methodName);
                BinaryUtil.NVTraceI("typeName",typeName);
                if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInline))
                {
                    if (callContext is String)
                        BinaryUtil.NVTraceI("callContext", (String)callContext);   
                    else
                        BinaryUtil.NVTraceI("callContext", scallContext);   
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInline))
                {
                    BinaryUtil.NVTraceI("args Length", args.Length);
                    for (int i=0; i<args.Length; i++)
                    {
                        BinaryUtil.NVTraceI("arg["+i+"]", args[i]);
                    }
                }

                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }

    internal sealed class BinaryMethodReturn : IStreamable
    {
        Object returnValue;
        Object[] args;
        Exception exception;
        Object callContext;
        String scallContext;
        Object properties;
        Type[] argTypes;
        bool bArgsPrimitive = true;
        MessageEnum messageEnum;
        Object[] callA;
        Type returnType;
        static Object instanceOfVoid = FormatterServices.GetUninitializedObject(Converter.typeofSystemVoid);

        [System.Security.SecuritySafeCritical] // static constructors should be safe to call
        static BinaryMethodReturn()
        {
        }

        internal BinaryMethodReturn()
        {
        }

        // If the argument list contains only primitive or strings it is written out as part of the header
        // if not the args are written out as a separate array
        internal Object[] WriteArray(Object returnValue, Object[] args, Exception exception, Object callContext, Object[] properties)
        {
            SerTrace.Log(this, "WriteArray returnValue ",returnValue, "exception ", exception, " callContext ",callContext," properties ", properties);

            this.returnValue = returnValue;
            this.args = args;
            this.exception = exception;
            this.callContext = callContext;
            this.properties = properties;

            int arraySize = 0;
            if (args == null || args.Length == 0)
                messageEnum = MessageEnum.NoArgs;
            else
            {
                argTypes = new Type[args.Length];

                // Check if args are all string or primitives

                bArgsPrimitive = true;
                for (int i =0; i<args.Length; i++)
                {
                    if (args[i] != null)
                    {
                        argTypes[i] = args[i].GetType();
                        bool isArgPrimitive = Converter.ToCode(argTypes[i]) != InternalPrimitiveTypeE.Invalid;
                        if (!(isArgPrimitive || Object.ReferenceEquals(argTypes[i], Converter.typeofString)))
                        {
                            bArgsPrimitive = false;
                            break;
                        }
                    }
                }

                if (bArgsPrimitive)
                    messageEnum = MessageEnum.ArgsInline;
                else
                {
                    arraySize++;
                    messageEnum = MessageEnum.ArgsInArray;
                }
            }


            if (returnValue == null)
                messageEnum |= MessageEnum.NoReturnValue;
            else if (returnValue.GetType() == typeof(void))
                messageEnum |= MessageEnum.ReturnValueVoid;
            else
            {
                returnType = returnValue.GetType();
                bool isReturnTypePrimitive = Converter.ToCode(returnType) != InternalPrimitiveTypeE.Invalid;
                if (isReturnTypePrimitive || Object.ReferenceEquals(returnType, Converter.typeofString))
                    messageEnum |= MessageEnum.ReturnValueInline;
                else
                {
                    arraySize++;
                    messageEnum |= MessageEnum.ReturnValueInArray;
                }
            }

            if (exception != null)
            {
                arraySize++;
                messageEnum |= MessageEnum.ExceptionInArray;
            }

            if (callContext == null)
                messageEnum |= MessageEnum.NoContext;
            else if (callContext is String)
                messageEnum |= MessageEnum.ContextInline;
            else
            {
                arraySize++;
                messageEnum |= MessageEnum.ContextInArray;
            }

            if (properties != null)
            {
                arraySize++;
                messageEnum |= MessageEnum.PropertyInArray;
            }

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInArray) && (arraySize == 1))
            {
                messageEnum ^= MessageEnum.ArgsInArray;
                messageEnum |= MessageEnum.ArgsIsArray;
                return args;
            }

            if (arraySize > 0)
            {
                int arrayPosition = 0;
                callA = new Object[arraySize];
                if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInArray))
                    callA[arrayPosition++] = args;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ReturnValueInArray))
                    callA[arrayPosition++] = returnValue;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ExceptionInArray))
                    callA[arrayPosition++] = exception;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInArray))
                    callA[arrayPosition++] = callContext;

                if (IOUtil.FlagTest(messageEnum, MessageEnum.PropertyInArray))
                    callA[arrayPosition] = properties;

                 return callA;
            }
            else
                return null;
        }


        public void Write(__BinaryWriter sout) 
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.MethodReturn);
            sout.WriteInt32((Int32)messageEnum);

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ReturnValueInline))
            {
                IOUtil.WriteWithCode(returnType, returnValue, sout);
            }

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInline))
                IOUtil.WriteStringWithCode((String)callContext, sout);

            if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInline))
            {
                sout.WriteInt32(args.Length);
                for (int i=0; i<args.Length; i++)
                {
                    IOUtil.WriteWithCode(argTypes[i], args[i], sout);
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Read(__BinaryParser input)
        {
             messageEnum = (MessageEnum)input.ReadInt32();

             if (IOUtil.FlagTest(messageEnum, MessageEnum.NoReturnValue))
                 returnValue = null;
             else if (IOUtil.FlagTest(messageEnum, MessageEnum.ReturnValueVoid))
             {
                 returnValue = instanceOfVoid;            
             }
             else if (IOUtil.FlagTest(messageEnum, MessageEnum.ReturnValueInline))
                 returnValue = IOUtil.ReadWithCode(input);

#if FEATURE_REMOTING
             if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInline))
             {
                 scallContext = (String)IOUtil.ReadWithCode(input);
                 LogicalCallContext lcallContext = new LogicalCallContext();
                 lcallContext.RemotingData.LogicalCallID = scallContext;
                 callContext = lcallContext;
             }
#endif
             if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInline))
                 args = IOUtil.ReadArgs(input);
        }

#if FEATURE_REMOTING
        [System.Security.SecurityCritical]  // auto-generated
        internal IMethodReturnMessage ReadArray(Object[] returnA, IMethodCallMessage methodCallMessage, Object handlerObject)
        {
            if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsIsArray))
            {
                args = returnA;
            }
            else
            {
                int arrayPosition = 0;
                    
                if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInArray))
                {
                    if (returnA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    args = (Object[])returnA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ReturnValueInArray))
                {
                    if (returnA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    returnValue = returnA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ExceptionInArray))
                {
                    if (returnA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    exception = (Exception)returnA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInArray))
                {
                   if (returnA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    callContext = returnA[arrayPosition++];
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.PropertyInArray))
                {
                    if (returnA.Length < arrayPosition)
                        throw new SerializationException(Environment.GetResourceString("Serialization_Method"));
                    properties = returnA[arrayPosition++];
                }
            }
            return new MethodResponse(methodCallMessage, handlerObject,  new BinaryMethodReturnMessage(returnValue, args, exception, (LogicalCallContext)callContext, (Object[])properties));
        }
#endif // FEATURE_REMOTING
        public void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryMethodReturn*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "MethodReturn");
                BinaryUtil.NVTraceI("messageEnum (Int32)", ((Enum)messageEnum).ToString());

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ReturnValueInline))
                    BinaryUtil.NVTraceI("returnValue", returnValue);

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ContextInline))
                {
                    if (callContext is String)
                        BinaryUtil.NVTraceI("callContext", (String)callContext);   
                    else
                        BinaryUtil.NVTraceI("callContext", scallContext);   
                }

                if (IOUtil.FlagTest(messageEnum, MessageEnum.ArgsInline))
                {
                    BinaryUtil.NVTraceI("args Length", args.Length);
                    for (int i=0; i<args.Length; i++)
                    {
                        BinaryUtil.NVTraceI("arg["+i+"]", args[i]);
                    }
                }

                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }



    internal sealed class BinaryObjectString : IStreamable
    {
        internal Int32 objectId;
        internal String value;

        internal BinaryObjectString()
        {
        }

        internal  void Set(Int32 objectId, String value)
        {
            SerTrace.Log(this, "BinaryObjectString set ",objectId," ",value);
            this.objectId = objectId;
            this.value = value;
        }   


        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.ObjectString);
            sout.WriteInt32(objectId);
            sout.WriteString(value);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            objectId = input.ReadInt32();
            value = input.ReadString();
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryObjectString*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "ObjectString");
                BinaryUtil.NVTraceI("objectId (Int32)", objectId);              
                BinaryUtil.NVTraceI("value (UTF)", value);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }

    internal sealed class BinaryCrossAppDomainString : IStreamable
    {
        internal Int32 objectId;
        internal Int32 value;

        internal BinaryCrossAppDomainString()
        {
        }

        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.CrossAppDomainString);
            sout.WriteInt32(objectId);
            sout.WriteInt32(value);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            objectId = input.ReadInt32();
            value = input.ReadInt32();
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryCrossAppDomainString*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "CrossAppDomainString");
                BinaryUtil.NVTraceI("objectId (Int32)", objectId);              
                BinaryUtil.NVTraceI("value (Int32)", value);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }

    internal sealed class BinaryCrossAppDomainMap : IStreamable
    {
        internal Int32 crossAppDomainArrayIndex;

        internal BinaryCrossAppDomainMap()
        {
        }

        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.CrossAppDomainMap);
            sout.WriteInt32(crossAppDomainArrayIndex);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            crossAppDomainArrayIndex = input.ReadInt32();
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryCrossAppDomainMap*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "CrossAppDomainMap");
                BinaryUtil.NVTraceI("crossAppDomainArrayIndex (Int32)", crossAppDomainArrayIndex);              
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }


    internal sealed class MemberPrimitiveTyped : IStreamable
    {
        internal InternalPrimitiveTypeE primitiveTypeEnum;
        internal Object value;

        internal MemberPrimitiveTyped()
        {
        }

        internal void Set(InternalPrimitiveTypeE primitiveTypeEnum, Object value)
        {
            SerTrace.Log(this, "MemberPrimitiveTyped Set ",((Enum)primitiveTypeEnum).ToString()," ",value);
            this.primitiveTypeEnum = primitiveTypeEnum;
            this.value = value;
        }   


        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.MemberPrimitiveTyped);
            sout.WriteByte((Byte)primitiveTypeEnum); //pdj
            sout.WriteValue(primitiveTypeEnum, value);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            primitiveTypeEnum = (InternalPrimitiveTypeE)input.ReadByte(); //PDJ
            value = input.ReadValue(primitiveTypeEnum);     
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****MemberPrimitiveTyped*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "MemberPrimitiveTyped");
                BinaryUtil.NVTraceI("primitiveTypeEnum (Byte)", ((Enum)primitiveTypeEnum).ToString());
                BinaryUtil.NVTraceI("value ("+ Converter.ToComType(primitiveTypeEnum)+")", value);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }


    internal sealed class BinaryObjectWithMap : IStreamable
    {
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal Int32 objectId;
        internal String name;
        internal Int32 numMembers;
        internal String[] memberNames;
        internal Int32 assemId;   

        internal BinaryObjectWithMap()
        {
        }

        internal BinaryObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        internal  void Set(Int32 objectId, String name, Int32 numMembers, String[] memberNames, Int32 assemId)
        {
#if _DEBUG            
            SerTrace.Log(this, "BinaryObjectWithMap Set ",objectId," assemId ",assemId," ",Util.PString(name)," numMembers ",numMembers);
#endif
            this.objectId = objectId;
            this.name = name;
            this.numMembers = numMembers;
            this.memberNames = memberNames;
            this.assemId = assemId;

            if (assemId > 0)
                binaryHeaderEnum = BinaryHeaderEnum.ObjectWithMapAssemId;
            else
                binaryHeaderEnum = BinaryHeaderEnum.ObjectWithMap;

        }

        public  void Write(__BinaryWriter sout)
        {

            sout.WriteByte((Byte)binaryHeaderEnum);
            sout.WriteInt32(objectId);
            sout.WriteString(name);
            sout.WriteInt32(numMembers);
            for (int i=0; i<numMembers; i++)
                sout.WriteString(memberNames[i]);
            if (assemId > 0)
                sout.WriteInt32(assemId);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            objectId = input.ReadInt32();
            name = input.ReadString();
            numMembers = input.ReadInt32();
            memberNames = new String[numMembers];
            for (int i=0; i<numMembers; i++)
            {
                memberNames[i] = input.ReadString();
                SerTrace.Log(this, "BinaryObjectWithMap Read ",i," ",memberNames[i]);
            }

            if (binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                assemId = input.ReadInt32();
            }
        }


        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryObjectWithMap*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                BinaryUtil.NVTraceI("objectId (Int32)", objectId);
                BinaryUtil.NVTraceI("name (UTF)", name);
                BinaryUtil.NVTraceI("numMembers (Int32)", numMembers);
                for (int i=0; i<numMembers; i++)
                    BinaryUtil.NVTraceI("memberNames (UTF)", memberNames[i]);
                if (binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
                BinaryUtil.NVTraceI("assemId (Int32)", assemId);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }


    internal  sealed class BinaryObjectWithMapTyped : IStreamable
    {
        internal BinaryHeaderEnum binaryHeaderEnum;     
        internal Int32 objectId;
        internal String name;
        internal Int32 numMembers;
        internal String[] memberNames;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal Object[] typeInformationA;
        internal Int32[] memberAssemIds;
        internal Int32 assemId;     

        internal BinaryObjectWithMapTyped()
        {
        }

        internal BinaryObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

#if false
        internal BinaryObjectWithMapTyped Copy()
        {
        BinaryObjectWithMapTyped newBOWM = new BinaryObjectWithMapTyped(binaryHeaderEnum);

        String[] newMemberNames = new String[numMembers];
        Array.Copy(memberNames, newMemberNames, numMembers);
        BinaryTypeEnum[] newBinaryTypeEnumA = new BinaryTypeEnum[binaryTypeEnumA.Length];
        Array.Copy(binaryTypeEnumA, newBinaryTypeEnumA, binaryTypeEnumA.Length);
        Object[] newTypeInformationA = new Object[typeInformationA.Length];
        Array.Copy(typeInformationA, newTypeInformationA, typeInformationA.Length);
        Int32[] newMemberAssemIds = new Int32[memberAssemIds.Length];
        Array.Copy(memberAssemIds, newMemberAssemIds, memberAssemIds.Length);

        newBOWM.Set(objectId, name, numMembers, newMemberNames, newBinaryTypeEnumA, newTypeInformationA, newMemberAssemIds, assemId);
        return newBOWM;
        }
#endif


        internal  void Set(Int32 objectId, String name, Int32 numMembers, String[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, Object[] typeInformationA, Int32[] memberAssemIds, Int32 assemId)
        {
            SerTrace.Log(this, "BinaryObjectWithMapTyped Set ",objectId," assemId ",assemId," ",name," numMembers ",numMembers);
            this.objectId = objectId;
            this.assemId = assemId;         
            this.name = name;
            this.numMembers = numMembers;
            this.memberNames = memberNames;
            this.binaryTypeEnumA = binaryTypeEnumA;
            this.typeInformationA = typeInformationA;
            this.memberAssemIds = memberAssemIds;
            this.assemId = assemId;

            if (assemId > 0)
                binaryHeaderEnum = BinaryHeaderEnum.ObjectWithMapTypedAssemId;
            else
                binaryHeaderEnum = BinaryHeaderEnum.ObjectWithMapTyped;             
        }


        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)binaryHeaderEnum);         
            sout.WriteInt32(objectId);
            sout.WriteString(name);
            sout.WriteInt32(numMembers);
            for (int i=0; i<numMembers; i++)
                sout.WriteString(memberNames[i]);
            for (int i=0; i<numMembers; i++)
                sout.WriteByte((Byte)binaryTypeEnumA[i]);
            for (int i=0; i<numMembers; i++)
                //if (binaryTypeEnumA[i] != BinaryTypeEnum.ObjectUrt && binaryTypeEnumA[i] != BinaryTypeEnum.ObjectUser)
                    BinaryConverter.WriteTypeInfo(binaryTypeEnumA[i], typeInformationA[i], memberAssemIds[i], sout);

            if (assemId > 0)
                sout.WriteInt32(assemId);

        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            // binaryHeaderEnum has already been read
            objectId = input.ReadInt32();
            name = input.ReadString();
            numMembers = input.ReadInt32();
            memberNames = new String[numMembers];
            binaryTypeEnumA = new BinaryTypeEnum[numMembers];
            typeInformationA = new Object[numMembers];
            memberAssemIds = new Int32[numMembers];
            for (int i=0; i<numMembers; i++)
                memberNames[i] = input.ReadString();
            for (int i=0; i<numMembers; i++)
                binaryTypeEnumA[i] = (BinaryTypeEnum)input.ReadByte();
            for (int i=0; i<numMembers; i++)
                if (binaryTypeEnumA[i] != BinaryTypeEnum.ObjectUrt && binaryTypeEnumA[i] != BinaryTypeEnum.ObjectUser)
                    typeInformationA[i] = BinaryConverter.ReadTypeInfo(binaryTypeEnumA[i], input, out memberAssemIds[i]);
                else
                    BinaryConverter.ReadTypeInfo(binaryTypeEnumA[i], input, out memberAssemIds[i]);
            
            if (binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                assemId = input.ReadInt32();                
            }
        }

#if _DEBUG
        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****BinaryObjectWithMapTyped*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                BinaryUtil.NVTraceI("objectId (Int32)", objectId);          
                BinaryUtil.NVTraceI("name (UTF)", name);
                BinaryUtil.NVTraceI("numMembers (Int32)", numMembers);
                for (int i=0; i<numMembers; i++)
                    BinaryUtil.NVTraceI("memberNames (UTF)", memberNames[i]);
                for (int i=0; i<numMembers; i++)
                    BinaryUtil.NVTraceI("binaryTypeEnum("+i+") (Byte)", ((Enum)binaryTypeEnumA[i]).ToString());
                for (int i=0; i<numMembers; i++)
                    if ((binaryTypeEnumA[i] == BinaryTypeEnum.Primitive) || 
                        (binaryTypeEnumA[i] == BinaryTypeEnum.PrimitiveArray) || 
                        (binaryTypeEnumA[i] == BinaryTypeEnum.ObjectUrt) || 
                        (binaryTypeEnumA[i] == BinaryTypeEnum.ObjectUser))
                    {
                        BinaryUtil.NVTraceI("typeInformation("+i+") "+BinaryConverter.TypeInfoTraceString(typeInformationA[i]), typeInformationA[i]);
                        if (binaryTypeEnumA[i] == BinaryTypeEnum.ObjectUser)
                             BinaryUtil.NVTraceI("memberAssemId("+i+") (Int32)", memberAssemIds[i]);
                    }

                    /*
                    for (int i=0; i<numMembers; i++)
                    {
                    if (binaryTypeEnumA[i] == BinaryTypeEnum.ObjectUser)
                    BinaryUtil.NVTraceI("memberAssemId("+i+") (Int32)", memberAssemIds[i]);
                    }
            */
                if (binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
                    BinaryUtil.NVTraceI("assemId (Int32)", assemId);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
#endif
    }


    internal  sealed class BinaryArray : IStreamable
    {
        internal Int32 objectId;
        internal Int32 rank;
        internal Int32[] lengthA;
        internal Int32[] lowerBoundA;
        internal BinaryTypeEnum binaryTypeEnum;
        internal Object typeInformation;
        internal int assemId = 0;

        private BinaryHeaderEnum binaryHeaderEnum;
        internal BinaryArrayTypeEnum binaryArrayTypeEnum;

        internal BinaryArray()
        {
            SerTrace.Log( this, "BinaryArray Constructor 1 ");
        }

        // Read constructor
        internal BinaryArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            SerTrace.Log( this, "BinaryArray Constructor 2 ",   ((Enum)binaryHeaderEnum).ToString());
            this.binaryHeaderEnum = binaryHeaderEnum;
        }


        internal void Set(Int32 objectId, Int32 rank, Int32[] lengthA, Int32[] lowerBoundA, BinaryTypeEnum binaryTypeEnum, Object typeInformation, BinaryArrayTypeEnum binaryArrayTypeEnum, int assemId)
        {
            SerTrace.Log( this, "BinaryArray Set objectId ",objectId," rank ",rank," ",((Enum)binaryTypeEnum).ToString(),", assemId ",assemId);
            this.objectId = objectId;
            this.binaryArrayTypeEnum = binaryArrayTypeEnum;
            this.rank = rank;
            this.lengthA = lengthA;
            this.lowerBoundA = lowerBoundA;
            this.binaryTypeEnum = binaryTypeEnum;
            this.typeInformation = typeInformation;
            this.assemId = assemId;
            binaryHeaderEnum = BinaryHeaderEnum.Array;

            if (binaryArrayTypeEnum == BinaryArrayTypeEnum.Single)
            {
                if (binaryTypeEnum == BinaryTypeEnum.Primitive)
                    binaryHeaderEnum = BinaryHeaderEnum.ArraySinglePrimitive;
                else if (binaryTypeEnum == BinaryTypeEnum.String)
                    binaryHeaderEnum = BinaryHeaderEnum.ArraySingleString;
                else if (binaryTypeEnum == BinaryTypeEnum.Object)
                    binaryHeaderEnum = BinaryHeaderEnum.ArraySingleObject;
            }
            SerTrace.Log( this, "BinaryArray Set Exit ",((Enum)binaryHeaderEnum).ToString());
        }


        public  void Write(__BinaryWriter sout)
        {
            SerTrace.Log( this, "Write");
            switch (binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    sout.WriteByte((Byte)binaryHeaderEnum);
                    sout.WriteInt32(objectId);
                    sout.WriteInt32(lengthA[0]);
                    sout.WriteByte((Byte)((InternalPrimitiveTypeE)typeInformation));
                    break;
                case BinaryHeaderEnum.ArraySingleString:
                    sout.WriteByte((Byte)binaryHeaderEnum);
                    sout.WriteInt32(objectId);
                    sout.WriteInt32(lengthA[0]);
                    break;
                case BinaryHeaderEnum.ArraySingleObject:
                    sout.WriteByte((Byte)binaryHeaderEnum);
                    sout.WriteInt32(objectId);
                    sout.WriteInt32(lengthA[0]);
                    break;
                default:
                    sout.WriteByte((Byte)binaryHeaderEnum);
                    sout.WriteInt32(objectId);
                    sout.WriteByte((Byte)binaryArrayTypeEnum);
                    sout.WriteInt32(rank);
                    for (int i=0; i<rank; i++)
                        sout.WriteInt32(lengthA[i]);
                    if ((binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) ||
                        (binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset) ||
                        (binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset))
                    {
                        for (int i=0; i<rank; i++)
                            sout.WriteInt32(lowerBoundA[i]);
                    }
                    sout.WriteByte((Byte)binaryTypeEnum);
                    BinaryConverter.WriteTypeInfo(binaryTypeEnum, typeInformation, assemId, sout);
                    break;
            }

        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            switch (binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    objectId = input.ReadInt32();
                    lengthA = new int[1];
                    lengthA[0] = input.ReadInt32();
                    binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    rank = 1;
                    lowerBoundA = new Int32[rank];
                    binaryTypeEnum = BinaryTypeEnum.Primitive;
                    typeInformation = (InternalPrimitiveTypeE)input.ReadByte();
                    break;
                case BinaryHeaderEnum.ArraySingleString:
                    objectId = input.ReadInt32();
                    lengthA = new int[1];
                    lengthA[0] = (int)input.ReadInt32();
                    binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    rank = 1;
                    lowerBoundA = new Int32[rank];
                    binaryTypeEnum = BinaryTypeEnum.String;
                    typeInformation = null;
                    break;
                case BinaryHeaderEnum.ArraySingleObject:
                    objectId = input.ReadInt32();
                    lengthA = new int[1];
                    lengthA[0] = (int)input.ReadInt32();
                    binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    rank = 1;
                    lowerBoundA = new Int32[rank];
                    binaryTypeEnum = BinaryTypeEnum.Object;
                    typeInformation = null;
                    break;
        default:
                    objectId = input.ReadInt32();
                    binaryArrayTypeEnum = (BinaryArrayTypeEnum)input.ReadByte();
                    rank = input.ReadInt32();
                    lengthA = new Int32[rank];
                    lowerBoundA = new Int32[rank];
                    for (int i=0; i<rank; i++)
                        lengthA[i] = input.ReadInt32();         
                    if ((binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) ||
                        (binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset) ||
                        (binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset))
                    {
                        for (int i=0; i<rank; i++)
                            lowerBoundA[i] = input.ReadInt32();
                    }
                    binaryTypeEnum = (BinaryTypeEnum)input.ReadByte();
                    typeInformation = BinaryConverter.ReadTypeInfo(binaryTypeEnum, input, out assemId);
                    break;
            }
        }

#if _DEBUG                        
        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                switch (binaryHeaderEnum)
                {
                    case BinaryHeaderEnum.ArraySinglePrimitive:
                        BCLDebug.Trace("BINARY","*****ArraySinglePrimitive*****");
                        BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                        BinaryUtil.NVTraceI("objectId (Int32)", objectId);                              
                        BinaryUtil.NVTraceI("length (Int32)", lengthA[0]);
                        BinaryUtil.NVTraceI("InternalPrimitiveTypeE (Byte)", ((Enum)typeInformation).ToString());
                        BCLDebug.Trace("BINARY","****************************");
                        break;
                    case BinaryHeaderEnum.ArraySingleString:
                        BCLDebug.Trace("BINARY","*****ArraySingleString*****");
                        BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                        BinaryUtil.NVTraceI("objectId (Int32)", objectId);
                        BinaryUtil.NVTraceI("length (Int32)", lengthA[0]);
                        BCLDebug.Trace("BINARY","****************************");
                        break;
                    case BinaryHeaderEnum.ArraySingleObject:
                        BCLDebug.Trace("BINARY","*****ArraySingleObject*****");
                        BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                        BinaryUtil.NVTraceI("objectId (Int32)", objectId);
                        BinaryUtil.NVTraceI("length (Int32)", lengthA[0]);
                        BCLDebug.Trace("BINARY","****************************");
                        break;
                    default:
                        BCLDebug.Trace("BINARY","*****BinaryArray*****");
                        BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)binaryHeaderEnum).ToString());
                        BinaryUtil.NVTraceI("objectId (Int32)", objectId);                              
                        BinaryUtil.NVTraceI("binaryArrayTypeEnum (Byte)", ((Enum)binaryArrayTypeEnum).ToString());              
                        BinaryUtil.NVTraceI("rank (Int32)", rank);
                        for (int i=0; i<rank; i++)
                            BinaryUtil.NVTraceI("length (Int32)", lengthA[i]);
                        if ((binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) ||
                            (binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset) ||
                            (binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset))
                        {
                            for (int i=0; i<rank; i++)
                                BinaryUtil.NVTraceI("lowerBound (Int32)", lowerBoundA[i]);
                        }
                        BinaryUtil.NVTraceI("binaryTypeEnum (Byte)", ((Enum)binaryTypeEnum).ToString());
                        if ((binaryTypeEnum == BinaryTypeEnum.Primitive) || 
                            (binaryTypeEnum == BinaryTypeEnum.PrimitiveArray) || 
                            (binaryTypeEnum == BinaryTypeEnum.ObjectUrt) || 
                            (binaryTypeEnum == BinaryTypeEnum.ObjectUser))
                            BinaryUtil.NVTraceI("typeInformation "+BinaryConverter.TypeInfoTraceString(typeInformation), typeInformation);
                        if (binaryTypeEnum == BinaryTypeEnum.ObjectUser)
                            BinaryUtil.NVTraceI("assemId (Int32)", assemId);
                        BCLDebug.Trace("BINARY","****************************");
                        break;
                }
            }
        }
#endif        
    }

    internal sealed class MemberPrimitiveUnTyped : IStreamable
    {
        // Used for members with primitive values and types are needed

        internal InternalPrimitiveTypeE typeInformation;
        internal Object value;

        internal MemberPrimitiveUnTyped()
        {
        }

        internal  void Set(InternalPrimitiveTypeE typeInformation, Object value)
        {
            SerTrace.Log( this, "MemberPrimitiveUnTyped Set typeInformation ",typeInformation," value ",value);
            this.typeInformation = typeInformation;
            this.value = value;
        }

        internal  void Set(InternalPrimitiveTypeE typeInformation)
        {
            SerTrace.Log(this, "MemberPrimitiveUnTyped  Set ",typeInformation);
            this.typeInformation = typeInformation;
        }



        public  void Write(__BinaryWriter sout)
        {
            sout.WriteValue(typeInformation, value);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
            value = input.ReadValue(typeInformation);
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                String typeString = Converter.ToComType(typeInformation);
                BCLDebug.Trace("BINARY","*****MemberPrimitiveUnTyped*****");
                BinaryUtil.NVTraceI("value ("+typeString+")", value);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }


    internal  sealed class MemberReference : IStreamable
    {
        internal Int32 idRef;

        internal MemberReference()
        {
        }

        internal  void Set(Int32 idRef)
        {
            SerTrace.Log( this, "MemberReference Set ",idRef);
            this.idRef = idRef;
        }

        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.MemberReference);
            sout.WriteInt32(idRef);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
            idRef = input.ReadInt32();
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****MemberReference*****");       
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", ((Enum)BinaryHeaderEnum.MemberReference).ToString());        
                BinaryUtil.NVTraceI("idRef (Int32)", idRef);
                BCLDebug.Trace("BINARY","****************************");
            }
        }
    }

    internal  sealed class ObjectNull : IStreamable
    {
        internal int nullCount;

        internal ObjectNull()
        {
        }

        internal void SetNullCount(int nullCount)
        {
            this.nullCount = nullCount;
        }

        public  void Write(__BinaryWriter sout)
        {
            if (nullCount == 1)
            {
                sout.WriteByte((Byte)BinaryHeaderEnum.ObjectNull);
            }
            else if (nullCount < 256)
            {
                sout.WriteByte((Byte)BinaryHeaderEnum.ObjectNullMultiple256);
                sout.WriteByte((Byte)nullCount);
                //Console.WriteLine("Write nullCount "+nullCount);
            }
            else
            {
                sout.WriteByte((Byte)BinaryHeaderEnum.ObjectNullMultiple);
                sout.WriteInt32(nullCount);                
                //Console.WriteLine("Write nullCount "+nullCount);
            }
        }


        [System.Security.SecurityCritical] // implements Critical method
        public  void Read(__BinaryParser input)
        {
            Read(input, BinaryHeaderEnum.ObjectNull);
        }

        public  void Read(__BinaryParser input, BinaryHeaderEnum binaryHeaderEnum)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
            switch (binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ObjectNull:
                    nullCount = 1;
                    break;
                case BinaryHeaderEnum.ObjectNullMultiple256:
                    nullCount = input.ReadByte();
                    //Console.WriteLine("Read nullCount "+nullCount);
                    break;
                case BinaryHeaderEnum.ObjectNullMultiple:
                    nullCount = input.ReadInt32();
                    //Console.WriteLine("Read nullCount "+nullCount);
                    break;
            }
        }

        public  void Dump()
        {
            DumpInternal();
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****ObjectNull*****");
                if (nullCount == 1)
                {
                    BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "ObjectNull");
                }
                else if (nullCount < 256)
                {
                    BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "ObjectNullMultiple256");
                    BinaryUtil.NVTraceI("nullCount (Byte)", nullCount);
                }
                else
                {
                    BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "ObjectNullMultiple");
                    BinaryUtil.NVTraceI("nullCount (Int32)", nullCount);
                }

                BCLDebug.Trace("BINARY","********************");
            }
        }
    }

    internal sealed class MessageEnd : IStreamable
    {

        internal MessageEnd()
        {
        }

        public  void Write(__BinaryWriter sout)
        {
            sout.WriteByte((Byte)BinaryHeaderEnum.MessageEnd);
        }

        [System.Security.SecurityCritical] // implements Critical method
        public void Read(__BinaryParser input)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
        }

        public  void Dump()
        {
            DumpInternal(null);
        }

        public  void Dump(Stream sout)
        {
            DumpInternal(sout);
        }

        [Conditional("_LOGGING")]
        private void DumpInternal(Stream sout)
        {
            if (BCLDebug.CheckEnabled("BINARY"))
            {
                BCLDebug.Trace("BINARY","*****MessageEnd*****");
                BinaryUtil.NVTraceI("binaryHeaderEnum (Byte)", "MessageEnd");
                long length = -1;
                if (sout != null && sout.CanSeek)
                {
                    length = sout.Length;
                    BinaryUtil.NVTraceI("Total Message Length in Bytes ", length);
                }
                BCLDebug.Trace("BINARY","********************");
            }
        }
    }


    // When an ObjectWithMap or an ObjectWithMapTyped is read off the stream, an ObjectMap class is created
    // to remember the type information. 
    internal sealed class ObjectMap
    {
        internal String objectName;
        internal Type objectType;

        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal Object[] typeInformationA;
        internal Type[] memberTypes;
        internal String[] memberNames;
        internal ReadObjectInfo objectInfo;
        internal bool isInitObjectInfo = true;
        internal ObjectReader objectReader = null;
        internal Int32 objectId;
        internal BinaryAssemblyInfo assemblyInfo;

        [System.Security.SecurityCritical]  // auto-generated
        internal ObjectMap(String objectName, Type objectType, String[] memberNames, ObjectReader objectReader, Int32 objectId, BinaryAssemblyInfo assemblyInfo)
        {
            SerTrace.Log( this, "Constructor 1 objectName ",objectName, ", objectType ",objectType);                            
            this.objectName = objectName;
            this.objectType = objectType;
            this.memberNames = memberNames;
            this.objectReader = objectReader;
            this.objectId = objectId;
            this.assemblyInfo = assemblyInfo;

            objectInfo = objectReader.CreateReadObjectInfo(objectType);
            memberTypes = objectInfo.GetMemberTypes(memberNames, objectType); 

            binaryTypeEnumA = new BinaryTypeEnum[memberTypes.Length];
            typeInformationA = new Object[memberTypes.Length];

            for (int i=0; i<memberTypes.Length; i++)
            {
                Object typeInformation = null;
                BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetParserBinaryTypeInfo(memberTypes[i], out typeInformation);
                binaryTypeEnumA[i] = binaryTypeEnum;
                typeInformationA[i] = typeInformation;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal ObjectMap(String objectName, String[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, Object[] typeInformationA, int[] memberAssemIds, ObjectReader objectReader, Int32 objectId, BinaryAssemblyInfo assemblyInfo, SizedArray assemIdToAssemblyTable)
        {
            SerTrace.Log( this, "Constructor 2 objectName ",objectName);
            this.objectName = objectName;
            this.memberNames = memberNames;
            this.binaryTypeEnumA = binaryTypeEnumA;
            this.typeInformationA = typeInformationA;
            this.objectReader = objectReader;
            this.objectId = objectId;
            this.assemblyInfo = assemblyInfo;

            if (assemblyInfo == null)
                throw new SerializationException(Environment.GetResourceString("Serialization_Assembly",objectName));

            objectType = objectReader.GetType(assemblyInfo, objectName);

            memberTypes = new Type[memberNames.Length];

            for (int i=0; i<memberNames.Length; i++)
            {
                InternalPrimitiveTypeE primitiveTypeEnum;
                String typeString;
                Type type;
                bool isVariant;

                BinaryConverter.TypeFromInfo(binaryTypeEnumA[i], typeInformationA[i], objectReader, (BinaryAssemblyInfo)assemIdToAssemblyTable[memberAssemIds[i]],
                                             out primitiveTypeEnum, out typeString, out type, out isVariant);
                //if ((object)type == null)
                //    throw new SerializationException(String.Format(Environment.GetResourceString("Serialization_TypeResolved"),objectName+" "+memberNames[i]+" "+typeInformationA[i]));
                memberTypes[i] = type;
            }

            objectInfo = objectReader.CreateReadObjectInfo(objectType, memberNames, null);
            if (!objectInfo.isSi)
                objectInfo.GetMemberTypes(memberNames, objectInfo.objectType);  // Check version match
        }

        internal ReadObjectInfo CreateObjectInfo(ref SerializationInfo si, ref Object[] memberData)
        {
            if (isInitObjectInfo)
            {
                isInitObjectInfo = false;
                objectInfo.InitDataStore(ref si, ref memberData);
                return objectInfo;
            }
            else
            {
                objectInfo.PrepareForReuse();
                objectInfo.InitDataStore(ref si, ref memberData);
                return objectInfo;
            }
        }


        // No member type information
        [System.Security.SecurityCritical]  // auto-generated
        internal static ObjectMap Create(String name, Type objectType, String[] memberNames, ObjectReader objectReader, Int32 objectId, BinaryAssemblyInfo assemblyInfo)
        {
            return new ObjectMap(name, objectType, memberNames, objectReader, objectId, assemblyInfo);
        }

        // Member type information 
        [System.Security.SecurityCritical]  // auto-generated
        internal static ObjectMap Create(String name, String[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, Object[] typeInformationA, int[] memberAssemIds, ObjectReader objectReader, Int32 objectId, BinaryAssemblyInfo assemblyInfo, SizedArray assemIdToAssemblyTable)
        {
            return new ObjectMap(name, memberNames, binaryTypeEnumA, typeInformationA, memberAssemIds, objectReader, objectId, assemblyInfo, assemIdToAssemblyTable);           
        }
    }

    // For each object or array being read off the stream, an ObjectProgress object is created. This object
    // keeps track of the progress of the parsing. When an object is being parsed, it keeps track of
    // the object member being parsed. When an array is being parsed it keeps track of the position within the
    // array.
    internal sealed class ObjectProgress
    {
        internal static int opRecordIdCount = 1;
        internal int opRecordId;


        // Control
        internal bool isInitial;
        internal int count; //Progress count
        internal BinaryTypeEnum expectedType = BinaryTypeEnum.ObjectUrt;
        internal Object expectedTypeInformation = null;

        internal String name;
        internal InternalObjectTypeE objectTypeEnum = InternalObjectTypeE.Empty;
        internal InternalMemberTypeE memberTypeEnum;
        internal InternalMemberValueE memberValueEnum;
        internal Type dtType;   

        // Array Information
        internal int numItems;
        internal BinaryTypeEnum binaryTypeEnum;
        internal Object typeInformation;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal int nullCount;
#pragma warning restore 0414

        // Member Information
        internal int memberLength;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal Object[] typeInformationA;
        internal String[] memberNames;
        internal Type[] memberTypes;

        // ParseRecord
        internal ParseRecord pr = new ParseRecord();


        internal ObjectProgress()
        {
            Counter();
        }

        [Conditional("SER_LOGGING")]                                    
        private void Counter()
        {
            lock(this) {
                opRecordId = opRecordIdCount++;
                if (opRecordIdCount > 1000)
                    opRecordIdCount = 1;
            }
        }

        internal void Init()
        {
            isInitial = false;
            count = 0;
            expectedType = BinaryTypeEnum.ObjectUrt;
            expectedTypeInformation = null;

            name = null;
            objectTypeEnum = InternalObjectTypeE.Empty;
            memberTypeEnum = InternalMemberTypeE.Empty;
            memberValueEnum = InternalMemberValueE.Empty;
            dtType = null;  

            // Array Information
            numItems = 0;
            nullCount = 0;
            //binaryTypeEnum
            typeInformation = null;

            // Member Information
            memberLength = 0;
            binaryTypeEnumA = null;
            typeInformationA = null;
            memberNames = null;
            memberTypes = null;

            pr.Init();
        }

        //Array item entry of nulls has a count of nulls represented by that item. The first null has been 
        // incremented by GetNext, the rest of the null counts are incremented here
        internal void ArrayCountIncrement(int value)
        {
            count += value;
        }

        // Specifies what is to parsed next from the wire.
        internal bool GetNext(out BinaryTypeEnum outBinaryTypeEnum, out Object outTypeInformation)  
        {
            //Initialize the out params up here.
            //<
            outBinaryTypeEnum = BinaryTypeEnum.Primitive;
            outTypeInformation = null;

#if _DEBUG
            SerTrace.Log( this, "GetNext Entry");
            Dump();
#endif

            if (objectTypeEnum == InternalObjectTypeE.Array)
            {
                SerTrace.Log( this, "GetNext Array");                   
                // Array
                if (count == numItems)
                    return false;
                else
                {
                    outBinaryTypeEnum =  binaryTypeEnum;
                    outTypeInformation = typeInformation;
                    if (count == 0)
                        isInitial = false;
                    count++;
                    SerTrace.Log( this, "GetNext Array Exit ",((Enum)outBinaryTypeEnum).ToString()," ",outTypeInformation);                                 
                    return true;
                }
            }
            else
            {
                // Member
                SerTrace.Log( this, "GetNext Member");                              
                if ((count == memberLength) && (!isInitial))
                    return false;
                else
                {
                    outBinaryTypeEnum = binaryTypeEnumA[count];
                    outTypeInformation = typeInformationA[count];
                    if (count == 0)
                        isInitial = false;
                    name = memberNames[count];
                    if (memberTypes == null)
                    {
                        SerTrace.Log( this, "GetNext memberTypes = null");
                    }
                    dtType = memberTypes[count];
                    count++;
                    SerTrace.Log( this, "GetNext Member Exit ",((Enum)outBinaryTypeEnum).ToString()," ",outTypeInformation," memberName ",name);                    
                    return true;
                }
            }
        }

#if _DEBUG
// Get a String describing the ObjectProgress Record
        public  String Trace()
        {
            return "ObjectProgress "+opRecordId+" name "+Util.PString(name)+" expectedType "+((Enum)expectedType).ToString();
        }

        // Dump contents of record

        [Conditional("SER_LOGGING")]                            
        internal  void Dump()
        {
            try
            {
                SerTrace.Log("ObjectProgress Dump ");
                Util.NVTrace("opRecordId", opRecordId);
                Util.NVTrace("isInitial", isInitial);
                Util.NVTrace("count", count);
                Util.NVTrace("expectedType", ((Enum)expectedType).ToString());
                Util.NVTrace("expectedTypeInformation", expectedTypeInformation);
                SerTrace.Log("ParseRecord Information");
                Util.NVTrace("name", name);
                Util.NVTrace("objectTypeEnum",((Enum)objectTypeEnum).ToString());
                Util.NVTrace("memberTypeEnum",((Enum)memberTypeEnum).ToString());
                Util.NVTrace("memberValueEnum",((Enum)memberValueEnum).ToString());
                if (dtType != null)
                    Util.NVTrace("dtType", dtType.ToString());
                SerTrace.Log("Array Information");
                Util.NVTrace("numItems", numItems);
                Util.NVTrace("binaryTypeEnum",((Enum)binaryTypeEnum).ToString());
                Util.NVTrace("typeInformation", typeInformation);
                SerTrace.Log("Member Information");
                Util.NVTrace("memberLength", memberLength);
                if (binaryTypeEnumA != null)
                {
                    for (int i=0; i<memberLength; i++)
                        Util.NVTrace("binaryTypeEnumA",((Enum)binaryTypeEnumA[i]).ToString());
                }
                if (typeInformationA != null)
                {
                    for (int i=0; i<memberLength; i++)
                        Util.NVTrace("typeInformationA", typeInformationA[i]);
                }
                if (memberNames != null)
                {
                    for (int i=0; i<memberLength; i++)
                        Util.NVTrace("memberNames", memberNames[i]);
                }
                if (memberTypes != null)
                {
                    for (int i=0; i<memberLength; i++)
                        Util.NVTrace("memberTypes", memberTypes[i].ToString());
                }
            }
            catch (Exception e)
            {
                BCLDebug.Log("[ObjectProgress.Dump]Unable to Dump Object Progress.");
                BCLDebug.Log("[ObjectProgress.Dump]Error: "+e);
            }
        }
#endif 
    }

        }




    
