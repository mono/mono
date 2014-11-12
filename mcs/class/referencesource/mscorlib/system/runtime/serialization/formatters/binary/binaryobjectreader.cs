// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 **
 ** Class: ObjectReader
 **
 **
 ** Purpose: DeSerializes Binary Wire format
 **
 **
 ===========================================================*/

namespace System.Runtime.Serialization.Formatters.Binary {

    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Collections;
    using System.Text;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Security;
    using System.Diagnostics;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.Contracts;
    using StackCrawlMark = System.Threading.StackCrawlMark;

    internal sealed class ObjectReader
    {

        // System.Serializer information
        internal Stream m_stream;
        internal ISurrogateSelector m_surrogates;
        internal StreamingContext m_context;
        internal ObjectManager m_objectManager;
        internal InternalFE formatterEnums;
        internal SerializationBinder m_binder;

        // Top object and headers
        internal long topId;
        internal bool bSimpleAssembly = false;
        internal Object handlerObject;
        internal Object m_topObject;
        internal Header[] headers;
        internal HeaderHandler handler;
        internal SerObjectInfoInit serObjectInfoInit;
        internal IFormatterConverter m_formatterConverter;

        // Stack of Object ParseRecords
        internal SerStack stack;

        // ValueType Fixup Stack
        private SerStack valueFixupStack;

        // Cross AppDomain
        internal Object[] crossAppDomainArray; //Set by the BinaryFormatter

        //MethodCall and MethodReturn are handled special for perf reasons
        private bool bFullDeserialization;
#if FEATURE_REMOTING        
        private bool bMethodCall;
        private bool bMethodReturn;
        private BinaryMethodCall binaryMethodCall;
        private BinaryMethodReturn binaryMethodReturn;
        private bool bIsCrossAppDomain;
#endif        

        private static FileIOPermission sfileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
        
        private SerStack ValueFixupStack
        {
            get {
                if (valueFixupStack == null)
                    valueFixupStack = new SerStack("ValueType Fixup Stack");
                return valueFixupStack;
            }
        }

        internal Object TopObject{
            get {
                return m_topObject;
            }
            set {
                m_topObject = value;
                if (m_objectManager != null)
                    m_objectManager.TopObject = value;
            }
        }
#if FEATURE_REMOTING        
        internal void SetMethodCall(BinaryMethodCall binaryMethodCall)
        {
            bMethodCall = true;
            this.binaryMethodCall = binaryMethodCall;
        }

        internal void SetMethodReturn(BinaryMethodReturn binaryMethodReturn)
        {
            bMethodReturn = true;
            this.binaryMethodReturn = binaryMethodReturn;
        }
#endif

        internal ObjectReader(Stream stream, ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums, SerializationBinder binder)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", Environment.GetResourceString("ArgumentNull_Stream"));
            }
            Contract.EndContractBlock();

            SerTrace.Log(this, "Constructor ISurrogateSelector ", ((selector == null) ? "null selector " : "selector present"));

            m_stream=stream;
            m_surrogates = selector;
            m_context = context;
            m_binder =  binder;

#if !FEATURE_PAL && FEATURE_SERIALIZATION
            // This is a hack to allow us to write a type-limiting deserializer
            // when we know exactly what type to expect at the head of the 
            // object graph.
            if (m_binder != null) {
                ResourceReader.TypeLimitingDeserializationBinder tldBinder = m_binder as ResourceReader.TypeLimitingDeserializationBinder;
                if (tldBinder != null)
                    tldBinder.ObjectReader = this;
            }
#endif // !FEATURE_PAL && FEATURE_SERIALIZATION

            this.formatterEnums = formatterEnums;

            //SerTrace.Log( this, "Constructor formatterEnums.FEtopObject ",formatterEnums.FEtopObject);

        }

#if FEATURE_REMOTING
        [System.Security.SecurityCritical]  // auto-generated
        internal Object Deserialize(HeaderHandler handler, __BinaryParser serParser, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage) {
            if (serParser == null)
                throw new ArgumentNullException("serParser", Environment.GetResourceString("ArgumentNull_WithParamName", serParser));
            Contract.EndContractBlock();

#if _DEBUG
            SerTrace.Log( this, "Deserialize Entry handler", handler);
#endif
            bFullDeserialization = false;
            TopObject = null;
            topId = 0;
#if FEATURE_REMOTING
            bMethodCall = false;
            bMethodReturn = false;
            bIsCrossAppDomain = isCrossAppDomain;
#endif
            bSimpleAssembly =  (formatterEnums.FEassemblyFormat == FormatterAssemblyStyle.Simple);

            if (fCheck)
            {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);
            }

            this.handler = handler;

            Contract.Assert(!bFullDeserialization, "we just set bFullDeserialization to false");

            // Will call back to ParseObject, ParseHeader for each object found
            serParser.Run();

#if _DEBUG
            SerTrace.Log( this, "Deserialize Finished Parsing DoFixups");
#endif

            if (bFullDeserialization)
                m_objectManager.DoFixups();


#if FEATURE_REMOTING
            if (!bMethodCall && !bMethodReturn)
#endif                
            {
                if (TopObject == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_TopObject"));

                //if TopObject has a surrogate then the actual object may be changed during special fixup
                //So refresh it using topID.
                if (HasSurrogate(TopObject.GetType())  && topId != 0)//Not yet resolved
                    TopObject = m_objectManager.GetObject(topId);

                if (TopObject is IObjectReference)
                {
                    TopObject = ((IObjectReference)TopObject).GetRealObject(m_context);
                }
            }

            SerTrace.Log( this, "Deserialize Exit ",TopObject);

            if (bFullDeserialization)
            {
                m_objectManager.RaiseDeserializationEvent(); // This will raise both IDeserialization and [OnDeserialized] events
            }                

            // Return the headers if there is a handler
            if (handler != null)
            {
                handlerObject = handler(headers);
            }
#if FEATURE_REMOTING
            if (bMethodCall)
            {
                Object[] methodCallArray = TopObject as Object[];
                TopObject = binaryMethodCall.ReadArray(methodCallArray, handlerObject);
            }
            else if (bMethodReturn)
            {
                Object[] methodReturnArray = TopObject as Object[];
                TopObject = binaryMethodReturn.ReadArray(methodReturnArray, methodCallMessage, handlerObject);
            }
#endif
            return TopObject;
        }
#endif       

#if !FEATURE_REMOTING
        internal Object Deserialize(HeaderHandler handler, __BinaryParser serParser, bool fCheck)
        {
            if (serParser == null)
                throw new ArgumentNullException("serParser", Environment.GetResourceString("ArgumentNull_WithParamName", serParser));
            Contract.EndContractBlock();

#if _DEBUG
            SerTrace.Log( this, "Deserialize Entry handler", handler);
#endif
            bFullDeserialization = false;
            TopObject = null;
            topId = 0;
#if FEATURE_REMOTING
            bMethodCall = false;
            bMethodReturn = false;
            bIsCrossAppDomain = isCrossAppDomain;
#endif
            bSimpleAssembly =  (formatterEnums.FEassemblyFormat == FormatterAssemblyStyle.Simple);

            if (fCheck)
            {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);
            }

            this.handler = handler;


            if (bFullDeserialization)
            {
                // Reinitialize
#if FEATURE_REMOTING
                m_objectManager = new ObjectManager(m_surrogates, m_context, false, bIsCrossAppDomain);
#else
                m_objectManager = new ObjectManager(m_surrogates, m_context, false, false);
#endif
                serObjectInfoInit = new SerObjectInfoInit();
            }

            // Will call back to ParseObject, ParseHeader for each object found
            serParser.Run();

#if _DEBUG
            SerTrace.Log( this, "Deserialize Finished Parsing DoFixups");
#endif

            if (bFullDeserialization)
                m_objectManager.DoFixups();


#if FEATURE_REMOTING
            if (!bMethodCall && !bMethodReturn)
#endif                
            {
                if (TopObject == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_TopObject"));

                //if TopObject has a surrogate then the actual object may be changed during special fixup
                //So refresh it using topID.
                if (HasSurrogate(TopObject.GetType())  && topId != 0)//Not yet resolved
                    TopObject = m_objectManager.GetObject(topId);

                if (TopObject is IObjectReference)
                {
                    TopObject = ((IObjectReference)TopObject).GetRealObject(m_context);
                }
            }

            SerTrace.Log( this, "Deserialize Exit ",TopObject);

            if (bFullDeserialization)
            {
                m_objectManager.RaiseDeserializationEvent(); // This will raise both IDeserialization and [OnDeserialized] events
            }                

            // Return the headers if there is a handler
            if (handler != null)
            {
                handlerObject = handler(headers);
            }
#if FEATURE_REMOTING
            if (bMethodCall)
            {
                Object[] methodCallArray = TopObject as Object[];
                TopObject = binaryMethodCall.ReadArray(methodCallArray, handlerObject);
            }
            else if (bMethodReturn)
            {
                Object[] methodReturnArray = TopObject as Object[];
                TopObject = binaryMethodReturn.ReadArray(methodReturnArray, methodCallMessage, handlerObject);
            }
#endif
            return TopObject;
        }
#endif

        [System.Security.SecurityCritical]  // auto-generated
        private bool HasSurrogate(Type t){
            if (m_surrogates == null)
                return false;
            ISurrogateSelector notUsed;
            return m_surrogates.GetSurrogate(t, m_context, out notUsed) != null;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void CheckSerializable(Type t)
        {
            if (!t.IsSerializable && !HasSurrogate(t))
                throw new SerializationException(String.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Serialization_NonSerType"), 
                                                                     t.FullName, t.Assembly.FullName));
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void InitFullDeserialization()
        {
            bFullDeserialization = true;
            stack = new SerStack("ObjectReader Object Stack");
#if FEATURE_REMOTING
            m_objectManager = new ObjectManager(m_surrogates, m_context, false, bIsCrossAppDomain);
#else
            m_objectManager = new ObjectManager(m_surrogates, m_context, false, false);
#endif
            if (m_formatterConverter == null)
                m_formatterConverter = new FormatterConverter();
        }


        internal Object CrossAppDomainArray(int index)
        {
            Contract.Assert((index < crossAppDomainArray.Length),
                             "[System.Runtime.Serialization.Formatters.BinaryObjectReader index out of range for CrossAppDomainArray]");
            return crossAppDomainArray[index];
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal ReadObjectInfo CreateReadObjectInfo(Type objectType)
        {
            return ReadObjectInfo.Create(objectType, m_surrogates, m_context, m_objectManager, serObjectInfoInit, m_formatterConverter, bSimpleAssembly);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal ReadObjectInfo CreateReadObjectInfo(Type objectType, String[] memberNames, Type[] memberTypes)
        {
            return ReadObjectInfo.Create(objectType, memberNames, memberTypes, m_surrogates, m_context, m_objectManager, serObjectInfoInit, m_formatterConverter, bSimpleAssembly);
        }


        // Main Parse routine, called by the XML Parse Handlers in XMLParser and also called internally to
        [System.Security.SecurityCritical]  // auto-generated
        internal void Parse(ParseRecord pr)
        {
#if _DEBUG
            SerTrace.Log( this, "Parse");
            stack.Dump();
            pr.Dump();
#endif

            switch (pr.PRparseTypeEnum)
            {
                case InternalParseTypeE.SerializedStreamHeader:
                    ParseSerializedStreamHeader(pr);
                    break;
                case InternalParseTypeE.SerializedStreamHeaderEnd:
                    ParseSerializedStreamHeaderEnd(pr);
                    break;                  
                case InternalParseTypeE.Object:
                    ParseObject(pr);
                    break;
                case InternalParseTypeE.ObjectEnd:
                    ParseObjectEnd(pr);
                    break;
                case InternalParseTypeE.Member:
                    ParseMember(pr);
                    break;
                case InternalParseTypeE.MemberEnd:
                    ParseMemberEnd(pr);
                    break;
                case InternalParseTypeE.Body:
                case InternalParseTypeE.BodyEnd:
                case InternalParseTypeE.Envelope:
                case InternalParseTypeE.EnvelopeEnd:
                    break;
                case InternalParseTypeE.Empty:
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_XMLElement", pr.PRname));                  

            }
        }


        // Styled ParseError output
        private void ParseError(ParseRecord processing, ParseRecord onStack)
        {
#if _DEBUG
            SerTrace.Log( this, " ParseError ",processing," ",onStack);
#endif
            throw new SerializationException(Environment.GetResourceString("Serialization_ParseError",onStack.PRname+" "+((Enum)onStack.PRparseTypeEnum) + " "+processing.PRname+" "+((Enum)processing.PRparseTypeEnum)));                               
        }

        // Parse the SerializedStreamHeader element. This is the first element in the stream if present
        private void ParseSerializedStreamHeader(ParseRecord pr)
        {
#if _DEBUG
            SerTrace.Log( this, "SerializedHeader ",pr);
#endif
            stack.Push(pr);
        }

        // Parse the SerializedStreamHeader end element. This is the last element in the stream if present
        private void ParseSerializedStreamHeaderEnd(ParseRecord pr)
        {
#if _DEBUG
            SerTrace.Log( this, "SerializedHeaderEnd ",pr);
#endif
            stack.Pop();
        }

#if FEATURE_REMOTING
        private bool IsRemoting {
            get {
                //return (m_context.State & (StreamingContextStates.Persistence|StreamingContextStates.File|StreamingContextStates.Clone)) == 0;
                return (bMethodCall || bMethodReturn);
            }
        }

         [System.Security.SecurityCritical]  // auto-generated
         internal void CheckSecurity(ParseRecord pr)
         {
            InternalST.SoapAssert(pr!=null, "[BinaryObjectReader.CheckSecurity]pr!=null");
            Type t = pr.PRdtType;
            if ((object)t != null){
                if( IsRemoting){
                    if (typeof(MarshalByRefObject).IsAssignableFrom(t))
                        throw new ArgumentException(Environment.GetResourceString("Serialization_MBRAsMBV", t.FullName));
                    FormatterServices.CheckTypeSecurity(t, formatterEnums.FEsecurityLevel);
                }
            }
        }
#endif

        // New object encountered in stream
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseObject(ParseRecord pr)
        {
#if _DEBUG
            SerTrace.Log( this, "ParseObject Entry ");
#endif

            if (!bFullDeserialization)
                InitFullDeserialization();

            if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                topId = pr.PRobjectId;

            if (pr.PRparseTypeEnum == InternalParseTypeE.Object)
            {
                    stack.Push(pr); // Nested objects member names are already on stack
            }

            if (pr.PRobjectTypeEnum == InternalObjectTypeE.Array)
            {
                ParseArray(pr);
#if _DEBUG
                SerTrace.Log( this, "ParseObject Exit, ParseArray ");
#endif
                return;
            }

            // If the Type is null, this means we have a typeload issue
            // mark the object with TypeLoadExceptionHolder
            if ((object)pr.PRdtType == null)
            {
                pr.PRnewObj = new TypeLoadExceptionHolder(pr.PRkeyDt);
                return;
            }
            
            if (Object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
            {
                // String as a top level object
                if (pr.PRvalue != null)
                {
                    pr.PRnewObj = pr.PRvalue;
                    if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                    {
#if _DEBUG
                        SerTrace.Log( this, "ParseObject String as top level, Top Object Resolved");
#endif
                        TopObject = pr.PRnewObj;
                        //stack.Pop();
                        return;
                    }
                    else
                    {
#if _DEBUG
                        SerTrace.Log( this, "ParseObject  String as an object");
#endif
                        stack.Pop();                        
                        RegisterObject(pr.PRnewObj, pr, (ParseRecord)stack.Peek());                         
                        return;
                    }
                }
                else
                {
                    // xml Doesn't have the value until later
                    return;
                }
            }
            else {
                    CheckSerializable(pr.PRdtType);
#if FEATURE_REMOTING
                    if (IsRemoting && formatterEnums.FEsecurityLevel != TypeFilterLevel.Full)
                        pr.PRnewObj = FormatterServices.GetSafeUninitializedObject(pr.PRdtType);                                 
                    else
#endif                        
                        pr.PRnewObj = FormatterServices.GetUninitializedObject(pr.PRdtType);            

                    // Run the OnDeserializing methods
                    m_objectManager.RaiseOnDeserializingEvent(pr.PRnewObj);
            }

            if (pr.PRnewObj == null)
                throw new SerializationException(Environment.GetResourceString("Serialization_TopObjectInstantiate",pr.PRdtType));

            if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
            {
#if _DEBUG
                SerTrace.Log( this, "ParseObject  Top Object Resolved ",pr.PRnewObj.GetType());
#endif
                TopObject = pr.PRnewObj;
            }

            if (pr.PRobjectInfo == null)
                pr.PRobjectInfo = ReadObjectInfo.Create(pr.PRdtType, m_surrogates, m_context, m_objectManager, serObjectInfoInit, m_formatterConverter, bSimpleAssembly);

#if FEATURE_REMOTING
            CheckSecurity(pr);
#endif

#if _DEBUG
            SerTrace.Log( this, "ParseObject Exit ");       
#endif
        }

        // End of object encountered in stream
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseObjectEnd(ParseRecord pr)
        {
#if _DEBUG
            SerTrace.Log( this, "ParseObjectEnd Entry ",pr.Trace());
#endif
            ParseRecord objectPr = (ParseRecord)stack.Peek();
            if (objectPr == null)
                objectPr = pr;

            //Contract.Assert(objectPr != null, "[System.Runtime.Serialization.Formatters.ParseObjectEnd]objectPr != null");

#if _DEBUG
            SerTrace.Log( this, "ParseObjectEnd objectPr ",objectPr.Trace());
#endif

            if (objectPr.PRobjectPositionEnum == InternalObjectPositionE.Top)
            {
#if _DEBUG
                SerTrace.Log( this, "ParseObjectEnd Top Object dtType ",objectPr.PRdtType);
#endif
                if (Object.ReferenceEquals(objectPr.PRdtType, Converter.typeofString))
                {
#if _DEBUG
                    SerTrace.Log( this, "ParseObjectEnd Top String");
#endif
                    objectPr.PRnewObj = objectPr.PRvalue;
                    TopObject = objectPr.PRnewObj;
                    return;
                }
            }

            stack.Pop();
            ParseRecord parentPr = (ParseRecord)stack.Peek();

            if (objectPr.PRnewObj == null)
                return;

            if (objectPr.PRobjectTypeEnum == InternalObjectTypeE.Array)
            {
                if (objectPr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
#if _DEBUG
                    SerTrace.Log( this, "ParseObjectEnd  Top Object (Array) Resolved");
#endif
                    TopObject = objectPr.PRnewObj;
                }

#if _DEBUG
                SerTrace.Log( this, "ParseArray  RegisterObject ",objectPr.PRobjectId," ",objectPr.PRnewObj.GetType());
#endif
                RegisterObject(objectPr.PRnewObj, objectPr, parentPr);                  

                return;
            }

            objectPr.PRobjectInfo.PopulateObjectMembers(objectPr.PRnewObj, objectPr.PRmemberData);

            // Registration is after object is populated
            if ((!objectPr.PRisRegistered) && (objectPr.PRobjectId > 0)) 
            {
#if _DEBUG
                SerTrace.Log( this, "ParseObject Register Object ",objectPr.PRobjectId," ",objectPr.PRnewObj.GetType());
#endif
                RegisterObject(objectPr.PRnewObj, objectPr, parentPr);
            }
            
            if (objectPr.PRisValueTypeFixup)
            {
#if _DEBUG
                SerTrace.Log( this, "ParseObjectEnd  ValueTypeFixup ",objectPr.PRnewObj.GetType());
#endif
                ValueFixup fixup = (ValueFixup)ValueFixupStack.Pop(); //Value fixup
                fixup.Fixup(objectPr, parentPr);  // Value fixup

            }

            if (objectPr.PRobjectPositionEnum == InternalObjectPositionE.Top)
            {
#if _DEBUG
                SerTrace.Log( this, "ParseObjectEnd  Top Object Resolved ",objectPr.PRnewObj.GetType());
#endif
                TopObject = objectPr.PRnewObj;
            }

            objectPr.PRobjectInfo.ObjectEnd();

#if _DEBUG
            SerTrace.Log( this, "ParseObjectEnd  Exit ",objectPr.PRnewObj.GetType()," id: ",objectPr.PRobjectId);       
#endif
        }



        // Array object encountered in stream
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseArray(ParseRecord pr)
        {
            SerTrace.Log( this, "ParseArray Entry");

            long genId = pr.PRobjectId;

            if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
            {
                SerTrace.Log( this, "ParseArray bin.base64 ",pr.PRvalue.Length," ",pr.PRvalue);
                // ByteArray
                if (pr.PRvalue.Length > 0)
                    pr.PRnewObj = Convert.FromBase64String(pr.PRvalue);
                else
                    pr.PRnewObj = new Byte[0];

                if (stack.Peek() == pr)
                {
                    SerTrace.Log( this, "ParseArray, bin.base64 has been stacked");
                    stack.Pop();
                }
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
        {  
                    TopObject = pr.PRnewObj;
        }

                ParseRecord parentPr = (ParseRecord)stack.Peek();                                           

                // Base64 can be registered at this point because it is populated
                SerTrace.Log( this, "ParseArray  RegisterObject ",pr.PRobjectId," ",pr.PRnewObj.GetType());
                RegisterObject(pr.PRnewObj, pr, parentPr);

            }
            else if ((pr.PRnewObj != null) && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
            {
                // Primtive typed Array has already been read
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
        {       
                    TopObject = pr.PRnewObj;
        }

                ParseRecord parentPr = (ParseRecord)stack.Peek();                                           

                // Primitive typed array can be registered at this point because it is populated
                SerTrace.Log( this, "ParseArray  RegisterObject ",pr.PRobjectId," ",pr.PRnewObj.GetType());
                RegisterObject(pr.PRnewObj, pr, parentPr);
            }
            else if ((pr.PRarrayTypeEnum == InternalArrayTypeE.Jagged) || (pr.PRarrayTypeEnum == InternalArrayTypeE.Single))
            {
                // Multidimensional jagged array or single array
                SerTrace.Log( this, "ParseArray Before Jagged,Simple create ",pr.PRarrayElementType," ",pr.PRlengthA[0]);
                bool bCouldBeValueType = true;
                if ((pr.PRlowerBoundA == null) || (pr.PRlowerBoundA[0] == 0))
                {
                    if (Object.ReferenceEquals(pr.PRarrayElementType, Converter.typeofString))
                    {
                        pr.PRobjectA = new String[pr.PRlengthA[0]];
                        pr.PRnewObj = pr.PRobjectA;
                        bCouldBeValueType = false;
                    }
                    else if (Object.ReferenceEquals(pr.PRarrayElementType, Converter.typeofObject))
                    {
                        pr.PRobjectA = new Object[pr.PRlengthA[0]];
                        pr.PRnewObj = pr.PRobjectA;
                        bCouldBeValueType = false;
                    }
                    else if ((object)pr.PRarrayElementType != null) {
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA[0]);
                    }
                    pr.PRisLowerBound = false;
                }
                else
                {
                    if ((object)pr.PRarrayElementType != null) {
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
                    }
                    pr.PRisLowerBound = true;
                }

                if (pr.PRarrayTypeEnum == InternalArrayTypeE.Single)
                {
                    if (!pr.PRisLowerBound && (Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode)))
                    {
                        pr.PRprimitiveArray = new PrimitiveArray(pr.PRarrayElementTypeCode, (Array)pr.PRnewObj);
                    }
                    else if (bCouldBeValueType && (object)pr.PRarrayElementType != null)
                    {
                        if (!pr.PRarrayElementType.IsValueType && !pr.PRisLowerBound)
                            pr.PRobjectA = (Object[])pr.PRnewObj;
                    }
                }

                SerTrace.Log( this, "ParseArray Jagged,Simple Array ",pr.PRnewObj.GetType());

                // For binary, headers comes in as an array of header objects
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Headers)
                {
                    SerTrace.Log( this, "ParseArray header array");
                    headers = (Header[])pr.PRnewObj;
                }

                pr.PRindexMap = new int[1];

            }
            else if (pr.PRarrayTypeEnum == InternalArrayTypeE.Rectangular)
            {
                // Rectangle array

                pr.PRisLowerBound = false;
                if (pr.PRlowerBoundA != null)
                {
                    for (int i=0; i<pr.PRrank; i++)
                    {
                        if (pr.PRlowerBoundA[i] != 0)
                            pr.PRisLowerBound = true;
                    }
                }

                if ((object)pr.PRarrayElementType != null){
                    if (!pr.PRisLowerBound)
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA);
                    else
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
                }

                SerTrace.Log( this, "ParseArray Rectangle Array ",pr.PRnewObj.GetType()," lower Bound ",pr.PRisLowerBound);

                // Calculate number of items
                int sum = 1;
                for (int i=0; i<pr.PRrank; i++)
                {
                    sum = sum*pr.PRlengthA[i];
                }
                pr.PRindexMap = new int[pr.PRrank];
                pr.PRrectangularMap = new int[pr.PRrank];
                pr.PRlinearlength = sum;
            }
            else
                throw new SerializationException(Environment.GetResourceString("Serialization_ArrayType",((Enum)pr.PRarrayTypeEnum)));                               

#if FEATURE_REMOTING
            CheckSecurity(pr);
#endif
            SerTrace.Log( this, "ParseArray Exit");     
        }


        // Builds a map for each item in an incoming rectangle array. The map specifies where the item is placed in the output Array Object

        private void NextRectangleMap(ParseRecord pr)
        {
            // For each invocation, calculate the next rectangular array position
            // example
            // indexMap 0 [0,0,0]
            // indexMap 1 [0,0,1]
            // indexMap 2 [0,0,2]
            // indexMap 3 [0,0,3]
            // indexMap 4 [0,1,0]       
            for (int irank = pr.PRrank-1; irank>-1; irank--)
            {
                // Find the current or lower dimension which can be incremented.
                if (pr.PRrectangularMap[irank] < pr.PRlengthA[irank]-1)
                {
                    // The current dimension is at maximum. Increase the next lower dimension by 1
                    pr.PRrectangularMap[irank]++;
                    if (irank < pr.PRrank-1)
                    {
                        // The current dimension and higher dimensions are zeroed.
                        for (int i = irank+1; i<pr.PRrank; i++)
                            pr.PRrectangularMap[i] = 0;
                    }
                    Array.Copy(pr.PRrectangularMap, pr.PRindexMap, pr.PRrank);              
                    break;                  
                }

            }
        }


        // Array object item encountered in stream
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseArrayMember(ParseRecord pr)
        {
            SerTrace.Log( this, "ParseArrayMember Entry");
            ParseRecord objectPr = (ParseRecord)stack.Peek();


            // Set up for inserting value into correct array position
            if (objectPr.PRarrayTypeEnum == InternalArrayTypeE.Rectangular)
            {

                if (objectPr.PRmemberIndex > 0)
                    NextRectangleMap(objectPr); // Rectangle array, calculate position in array
                if (objectPr.PRisLowerBound)
                {
                    for (int i=0; i<objectPr.PRrank; i++)
                    {
            objectPr.PRindexMap[i] = objectPr.PRrectangularMap[i] + objectPr.PRlowerBoundA[i];
                    }
                }
            }
            else
            {
                if (!objectPr.PRisLowerBound)
                {
                        objectPr.PRindexMap[0] = objectPr.PRmemberIndex; // Zero based array
                }
                else
                    objectPr.PRindexMap[0] = objectPr.PRlowerBoundA[0]+objectPr.PRmemberIndex; // Lower Bound based array
            }
            IndexTraceMessage("ParseArrayMember isLowerBound "+objectPr.PRisLowerBound+" indexMap  ", objectPr.PRindexMap);     

            // Set Array element according to type of element

            if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
            {
                // Object Reference

                // See if object has already been instantiated
                Object refObj = m_objectManager.GetObject(pr.PRidRef);
                if (refObj == null)
                {
                    // Object not instantiated
                    // Array fixup manager
            IndexTraceMessage("ParseArrayMember Record Fixup  "+objectPr.PRnewObj.GetType(), objectPr.PRindexMap);
                    int[] fixupIndex = new int[objectPr.PRrank];
                    Array.Copy(objectPr.PRindexMap, 0, fixupIndex, 0, objectPr.PRrank);

                    SerTrace.Log( this, "ParseArrayMember RecordArrayElementFixup objectId ",objectPr.PRobjectId," idRef ",pr.PRidRef);                                                         
                    m_objectManager.RecordArrayElementFixup(objectPr.PRobjectId, fixupIndex, pr.PRidRef);
                }
                else
                {
                    IndexTraceMessage("ParseArrayMember SetValue ObjectReference "+objectPr.PRnewObj.GetType()+" "+refObj, objectPr.PRindexMap);
                    if (objectPr.PRobjectA != null)
                        objectPr.PRobjectA[objectPr.PRindexMap[0]] = refObj;
                    else
                        ((Array)objectPr.PRnewObj).SetValue(refObj, objectPr.PRindexMap); // Object has been instantiated
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                //Set up dtType for ParseObject
                SerTrace.Log( this, "ParseArrayMember Nested ");
                if ((object)pr.PRdtType == null)
                {
                    pr.PRdtType = objectPr.PRarrayElementType;
                }

                ParseObject(pr);
                stack.Push(pr);

                if ((object)objectPr.PRarrayElementType != null) {
                    if ((objectPr.PRarrayElementType.IsValueType) && (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Invalid))
                    {
#if _DEBUG                        
                        SerTrace.Log( "ParseArrayMember ValueType ObjectPr ",objectPr.PRnewObj," index ",objectPr.PRmemberIndex);
#endif
                        pr.PRisValueTypeFixup = true; //Valuefixup
                        ValueFixupStack.Push(new ValueFixup((Array)objectPr.PRnewObj, objectPr.PRindexMap)); //valuefixup
                    }
                    else
                    {
#if _DEBUG                        
                        SerTrace.Log( "ParseArrayMember SetValue Nested, memberIndex ",objectPr.PRmemberIndex);
                        IndexTraceMessage("ParseArrayMember SetValue Nested ContainerObject "+objectPr.PRnewObj.GetType()+" "+objectPr.PRnewObj+" item Object "+pr.PRnewObj+" index ", objectPr.PRindexMap);

                        stack.Dump();               
                        SerTrace.Log( "ParseArrayMember SetValue Nested ContainerObject objectPr ",objectPr.Trace());
                        SerTrace.Log( "ParseArrayMember SetValue Nested ContainerObject pr ",pr.Trace());
#endif
                        if (objectPr.PRobjectA != null)
                            objectPr.PRobjectA[objectPr.PRindexMap[0]] = pr.PRnewObj;
                        else
                            ((Array)objectPr.PRnewObj).SetValue(pr.PRnewObj, objectPr.PRindexMap);
                    }
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
            {
                if ((Object.ReferenceEquals(objectPr.PRarrayElementType, Converter.typeofString)) || (Object.ReferenceEquals(pr.PRdtType, Converter.typeofString)))
                {
                    // String in either a string array, or a string element of an object array
                    ParseString(pr, objectPr);
                    IndexTraceMessage("ParseArrayMember SetValue String "+objectPr.PRnewObj.GetType()+" "+pr.PRvalue, objectPr.PRindexMap);
                    if (objectPr.PRobjectA != null)
                        objectPr.PRobjectA[objectPr.PRindexMap[0]] = (Object)pr.PRvalue;
                    else
                        ((Array)objectPr.PRnewObj).SetValue((Object)pr.PRvalue, objectPr.PRindexMap);
                }
                else if (objectPr.PRisArrayVariant)
                {
                    // Array of type object
                    if (pr.PRkeyDt == null)
                        throw new SerializationException(Environment.GetResourceString("Serialization_ArrayTypeObject"));

                    Object var = null;

                    if (Object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
                    {
                        ParseString(pr, objectPr);
                        var = pr.PRvalue;
                    }
                    else if (Object.ReferenceEquals(pr.PRdtTypeCode, InternalPrimitiveTypeE.Invalid))
                    {
                        CheckSerializable(pr.PRdtType);
                        // Not nested and invalid, so it is an empty object
#if FEATURE_REMOTING                        
                        if (IsRemoting && formatterEnums.FEsecurityLevel != TypeFilterLevel.Full)
                            var = FormatterServices.GetSafeUninitializedObject(pr.PRdtType);                                 
                        else
#endif                            
                            var = FormatterServices.GetUninitializedObject(pr.PRdtType);
                    }
                    else
                    {
                        if (pr.PRvarValue != null)
                            var = pr.PRvarValue;
                        else
                            var = Converter.FromString(pr.PRvalue, pr.PRdtTypeCode);
                    }
                    IndexTraceMessage("ParseArrayMember SetValue variant or Object "+objectPr.PRnewObj.GetType()+" var "+var+" indexMap ", objectPr.PRindexMap);
                    if (objectPr.PRobjectA != null)
                        objectPr.PRobjectA[objectPr.PRindexMap[0]] = var;
                    else
                        ((Array)objectPr.PRnewObj).SetValue(var, objectPr.PRindexMap); // Primitive type
                }
                else
                {
                    // Primitive type
                    if (objectPr.PRprimitiveArray != null)
                    {
                        // Fast path for Soap primitive arrays. Binary was handled in the BinaryParser
                        objectPr.PRprimitiveArray.SetValue(pr.PRvalue, objectPr.PRindexMap[0]);
                    }
                    else
                    {

                        Object var = null;
                        if (pr.PRvarValue != null)
                            var = pr.PRvarValue;
                        else
                            var = Converter.FromString(pr.PRvalue, objectPr.PRarrayElementTypeCode);
                        SerTrace.Log( this, "ParseArrayMember SetValue Primitive pr.PRvalue "+var," elementTypeCode ",((Enum)objectPr.PRdtTypeCode));
                        IndexTraceMessage("ParseArrayMember SetValue Primitive "+objectPr.PRnewObj.GetType()+" var: "+var+" varType "+var.GetType(), objectPr.PRindexMap);
                        if (objectPr.PRobjectA != null)
                        {
                            SerTrace.Log( this, "ParseArrayMember SetValue Primitive predefined array "+objectPr.PRobjectA.GetType());
                            objectPr.PRobjectA[objectPr.PRindexMap[0]] = var;
                        }
                        else
                            ((Array)objectPr.PRnewObj).SetValue(var, objectPr.PRindexMap); // Primitive type   
                        SerTrace.Log( this, "ParseArrayMember SetValue Primitive after");
                    }
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
            {
                SerTrace.Log( "ParseArrayMember Null item ",pr.PRmemberIndex," nullCount ",pr.PRnullCount);
                objectPr.PRmemberIndex += pr.PRnullCount-1; //also incremented again below
            }
            else
                ParseError(pr, objectPr);

#if _DEBUG                        
            SerTrace.Log( "ParseArrayMember increment memberIndex ",objectPr.PRmemberIndex," ",objectPr.Trace());               
#endif
            objectPr.PRmemberIndex++;
            SerTrace.Log( "ParseArrayMember Exit");     
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void ParseArrayMemberEnd(ParseRecord pr)
        {
            SerTrace.Log( this, "ParseArrayMemberEnd");
            // If this is a nested array object, then pop the stack
            if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                ParseObjectEnd(pr);
            }
        }


        // Object member encountered in stream
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseMember(ParseRecord pr)
        {
            SerTrace.Log( this, "ParseMember Entry ");


            ParseRecord objectPr = (ParseRecord)stack.Peek();
            String objName = null;
            if (objectPr != null)
                objName = objectPr.PRname;

#if _DEBUG                        
            SerTrace.Log( this, "ParseMember ",objectPr.PRobjectId," ",pr.PRname);
            SerTrace.Log( this, "ParseMember objectPr ",objectPr.Trace());
            SerTrace.Log( this, "ParseMember pr ",pr.Trace());
#endif
            switch (pr.PRmemberTypeEnum)
            {
                case InternalMemberTypeE.Item:
                    ParseArrayMember(pr);
                    return;
                case InternalMemberTypeE.Field:
                    break;
            }


            //if ((pr.PRdtType == null) && !objectPr.PRobjectInfo.isSi)
            if (((object)pr.PRdtType == null) && objectPr.PRobjectInfo.isTyped)
            {
                SerTrace.Log( this, "ParseMember pr.PRdtType null and not isSi");
                pr.PRdtType = objectPr.PRobjectInfo.GetType(pr.PRname);

                if ((object)pr.PRdtType != null)
                    pr.PRdtTypeCode = Converter.ToCode(pr.PRdtType);
            }

            if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
            {
                // Value is Null
                SerTrace.Log( this, "ParseMember null member: ",pr.PRname);
                SerTrace.Log( this, "AddValue 1");
                objectPr.PRobjectInfo.AddValue(pr.PRname, null, ref objectPr.PRsi, ref objectPr.PRmemberData);
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                SerTrace.Log( this, "ParseMember Nested Type member: ",pr.PRname," objectPr.PRnewObj ",objectPr.PRnewObj);
                ParseObject(pr);
                stack.Push(pr);
                SerTrace.Log( this, "AddValue 2 ",pr.PRnewObj," is value type ",pr.PRnewObj.GetType().IsValueType);

                if ((pr.PRobjectInfo != null) && ((object)pr.PRobjectInfo.objectType != null) && (pr.PRobjectInfo.objectType.IsValueType))
                {
                    SerTrace.Log( "ParseMember ValueType ObjectPr ",objectPr.PRnewObj," memberName  ",pr.PRname," nested object ",pr.PRnewObj);
                    pr.PRisValueTypeFixup = true; //Valuefixup
                    ValueFixupStack.Push(new ValueFixup(objectPr.PRnewObj, pr.PRname, objectPr.PRobjectInfo));//valuefixup
                }
                else
                {
                    SerTrace.Log( this, "AddValue 2A ");
                    objectPr.PRobjectInfo.AddValue(pr.PRname, pr.PRnewObj, ref objectPr.PRsi, ref objectPr.PRmemberData);
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
            {
                SerTrace.Log( this, "ParseMember Reference Type member: ",pr.PRname);           
                // See if object has already been instantiated
                Object refObj = m_objectManager.GetObject(pr.PRidRef);
                if (refObj == null)
                {
                    SerTrace.Log( this, "ParseMember RecordFixup: ",pr.PRname);
                    SerTrace.Log( this, "AddValue 3");                  
                    objectPr.PRobjectInfo.AddValue(pr.PRname, null, ref objectPr.PRsi, ref objectPr.PRmemberData);
                    objectPr.PRobjectInfo.RecordFixup(objectPr.PRobjectId, pr.PRname, pr.PRidRef); // Object not instantiated
                }
                else
                {
                    SerTrace.Log( this, "ParseMember Referenced Object Known ",pr.PRname," ",refObj);
                    SerTrace.Log( this, "AddValue 5");              
                    objectPr.PRobjectInfo.AddValue(pr.PRname, refObj, ref objectPr.PRsi, ref objectPr.PRmemberData);
                }
            }

            else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
            {
                // Primitive type or String
                SerTrace.Log( this, "ParseMember primitive or String member: ",pr.PRname);

                if (Object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
                {
                    ParseString(pr, objectPr);
                    SerTrace.Log( this, "AddValue 6");              
                    objectPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue, ref objectPr.PRsi, ref objectPr.PRmemberData);  
                }
                else if (pr.PRdtTypeCode == InternalPrimitiveTypeE.Invalid)
                {
                    // The member field was an object put the value is Inline either  bin.Base64 or invalid
                    if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
                    {
                        SerTrace.Log( this, "AddValue 7");                  
                        objectPr.PRobjectInfo.AddValue(pr.PRname, Convert.FromBase64String(pr.PRvalue), ref objectPr.PRsi, ref objectPr.PRmemberData);                                    
                    }
                    else if (Object.ReferenceEquals(pr.PRdtType, Converter.typeofObject))
                        throw new SerializationException(Environment.GetResourceString("Serialization_TypeMissing", pr.PRname));
                    else
                    {
                        SerTrace.Log( this, "Object Class with no memberInfo data  Member "+pr.PRname+" type "+pr.PRdtType);

                        ParseString(pr, objectPr); // Register the object if it has an objectId
                        // Object Class with no memberInfo data
                        // only special case where AddValue is needed?
                        if (Object.ReferenceEquals(pr.PRdtType, Converter.typeofSystemVoid))
                        {
                            SerTrace.Log( this, "AddValue 9");
                            objectPr.PRobjectInfo.AddValue(pr.PRname, pr.PRdtType, ref objectPr.PRsi, ref objectPr.PRmemberData);
                        }
                        else if (objectPr.PRobjectInfo.isSi)
                        {
                            // ISerializable are added as strings, the conversion to type is done by the
                            // ISerializable object
                            SerTrace.Log( this, "AddValue 10");
                            objectPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue, ref objectPr.PRsi, ref objectPr.PRmemberData);                          
                        }
                    }
                }
                else
                {
                    Object var = null;
                    if (pr.PRvarValue != null)
                        var = pr.PRvarValue;
                    else
                        var = Converter.FromString(pr.PRvalue, pr.PRdtTypeCode);
#if _DEBUG                        
                    // Not a string, convert the value
                    SerTrace.Log( this, "ParseMember Converting primitive and storing");
                    stack.Dump();
                    SerTrace.Log( this, "ParseMember pr "+pr.Trace());
                    SerTrace.Log( this, "ParseMember objectPr ",objectPr.Trace());

                    SerTrace.Log( this, "AddValue 11");                 
#endif                    
                    objectPr.PRobjectInfo.AddValue(pr.PRname, var, ref objectPr.PRsi, ref objectPr.PRmemberData);             
                }
            }
            else
                ParseError(pr, objectPr);
        }

        // Object member end encountered in stream
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseMemberEnd(ParseRecord pr)
        {
            SerTrace.Log( this, "ParseMemberEnd");
            switch (pr.PRmemberTypeEnum)
            {
                case InternalMemberTypeE.Item:
                    ParseArrayMemberEnd(pr);
                    return;
                case InternalMemberTypeE.Field:
                    if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
                        ParseObjectEnd(pr);
                    break;
                default:
                    ParseError(pr, (ParseRecord)stack.Peek());
                    break;
            }
        }

        // Processes a string object by getting an internal ID for it and registering it with the objectManager
        [System.Security.SecurityCritical]  // auto-generated
        private void ParseString(ParseRecord pr, ParseRecord parentPr)
        {
            SerTrace.Log( this, "ParseString Entry ",pr.PRobjectId," ",pr.PRvalue," ",pr.PRisRegistered);
            // Process String class
            if ((!pr.PRisRegistered) && (pr.PRobjectId > 0))
            {
                SerTrace.Log( this, "ParseString  RegisterObject ",pr.PRvalue," ",pr.PRobjectId);                           
                // String is treated as an object if it has an id
                //m_objectManager.RegisterObject(pr.PRvalue, pr.PRobjectId);
                RegisterObject(pr.PRvalue, pr, parentPr, true);
            }
        }


        [System.Security.SecurityCritical]  // auto-generated
        private void RegisterObject(Object obj, ParseRecord pr, ParseRecord objectPr)
        {
            RegisterObject(obj, pr, objectPr, false);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void RegisterObject(Object obj, ParseRecord pr, ParseRecord objectPr, bool bIsString)
        {
            if (!pr.PRisRegistered)
            {
                pr.PRisRegistered = true;

                SerializationInfo si = null;
                long parentId = 0;
                MemberInfo memberInfo = null;
                int[] indexMap = null;

                if (objectPr != null)
                {
                    indexMap = objectPr.PRindexMap;
                    parentId = objectPr.PRobjectId;                 

                    if (objectPr.PRobjectInfo != null)
                    {
                        if (!objectPr.PRobjectInfo.isSi)
                        {
                            // ParentId is only used if there is a memberInfo

                            memberInfo = objectPr.PRobjectInfo.GetMemberInfo(pr.PRname);
                        }
                    }
                }
                // SerializationInfo is always needed for ISerialization                        
                si = pr.PRsi;

                SerTrace.Log( this, "RegisterObject 0bj ",obj," objectId ",pr.PRobjectId," si ", si," parentId ",parentId," memberInfo ",memberInfo, " indexMap "+indexMap);
                if (bIsString)
                    m_objectManager.RegisterString((String)obj, pr.PRobjectId, si, parentId, memberInfo); 
                else
                    m_objectManager.RegisterObject(obj, pr.PRobjectId, si, parentId, memberInfo, indexMap); 
            }
        }


        // Assigns an internal ID associated with the binary id number

        // Older formatters generate ids for valuetypes using a different counter than ref types. Newer ones use
        // a single counter, only value types have a negative value. Need a way to handle older formats.
        private const int THRESHOLD_FOR_VALUETYPE_IDS = Int32.MaxValue;
        private bool bOldFormatDetected = false;
        private IntSizedArray   valTypeObjectIdTable;

        [System.Security.SecurityCritical]  // auto-generated
        internal long GetId(long objectId)
        {

            if (!bFullDeserialization)
                InitFullDeserialization();


            if (objectId > 0)
                return objectId;
            
            if (bOldFormatDetected || objectId == -1)
            {
                // Alarm bells. This is an old format. Deal with it.
                bOldFormatDetected = true;
                if (valTypeObjectIdTable == null)
                    valTypeObjectIdTable = new IntSizedArray();

                long tempObjId = 0;
                if ((tempObjId = valTypeObjectIdTable[(int)objectId]) == 0)
                {
                    tempObjId = THRESHOLD_FOR_VALUETYPE_IDS + objectId;
                    valTypeObjectIdTable[(int)objectId] = (int)tempObjId;
                }
                return tempObjId;
            }
            return -1 * objectId;
        }


        // Trace which includes a single dimensional int array
        [Conditional("SER_LOGGING")]                        
        private void IndexTraceMessage(String message, int[] index)
        {
            StringBuilder sb = StringBuilderCache.Acquire(10);
            sb.Append("[");     
            for (int i=0; i<index.Length; i++)
            {
                sb.Append(index[i]);
                if (i != index.Length -1)
                    sb.Append(",");
            }
            sb.Append("]");             
            SerTrace.Log( this, message," ", StringBuilderCache.GetStringAndRelease(sb));
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal Type Bind(String assemblyString, String typeString)
        {
            Type type = null;
            if (m_binder != null)
                type = m_binder.BindToType(assemblyString, typeString);
            if ((object)type == null)
                type= FastBindToType(assemblyString, typeString);

            return type;
        }

        internal class TypeNAssembly
        {
            public Type type;
            public String assemblyName;
        }

        NameCache typeCache = new NameCache();
        [System.Security.SecurityCritical]  // auto-generated
        internal Type FastBindToType(String assemblyName, String typeName)
        {
            Type type = null;

            TypeNAssembly entry = (TypeNAssembly)typeCache.GetCachedValue(typeName);

            if (entry == null || entry.assemblyName != assemblyName)
            {
                Assembly assm = null;
                if (bSimpleAssembly)
                {
                    try {
                          sfileIOPermission.Assert();
                          try {
#if FEATURE_FUSION
                              assm = ObjectReader.ResolveSimpleAssemblyName(new AssemblyName(assemblyName));
#else // FEATURE_FUSION
                              Assembly.Load(assemblyName);
#endif // FEATURE_FUSION
                          }
                          finally {
                              CodeAccessPermission.RevertAssert();
                          }
                    }
                    catch(Exception e){
                        SerTrace.Log( this, "FastBindTypeType ",e.ToString());
                    }

                    if (assm == null)
                        return null;

                    ObjectReader.GetSimplyNamedTypeFromAssembly(assm, typeName, ref type);
                }
                else {
                    try
                    {
                          sfileIOPermission.Assert();
                          try {
                              assm = Assembly.Load(assemblyName);
                          }
                          finally {
                              CodeAccessPermission.RevertAssert();
                          }
                    }
                    catch (Exception e)
                    {
                        SerTrace.Log( this, "FastBindTypeType ",e.ToString());
                    }

                    if (assm == null)
                        return null;

                    type = FormatterServices.GetTypeFromAssembly(assm, typeName);
                }

                if ((object)type == null)
                    return null;

                // before adding it to cache, let us do the security check 
                CheckTypeForwardedTo(assm, type.Assembly, type);
               
                entry = new TypeNAssembly();
                entry.type = type;
                entry.assemblyName = assemblyName;
                typeCache.SetCachedValue(entry);
            }
           return entry.type;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        private static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMe;
            Assembly assm = RuntimeAssembly.LoadWithPartialNameInternal(assemblyName, null, ref stackMark);
            if (assm == null && assemblyName != null)
                assm = RuntimeAssembly.LoadWithPartialNameInternal(assemblyName.Name, null, ref stackMark);
            return assm;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static void GetSimplyNamedTypeFromAssembly(Assembly assm, string typeName, ref Type type)
        {
            // Catching any exceptions that could be thrown from a failure on assembly load
            // This is necessary, for example, if there are generic parameters that are qualified with a version of the assembly that predates the one available
            try
            {
                type = FormatterServices.GetTypeFromAssembly(assm, typeName);
            }
            catch (TypeLoadException) { }
            catch (FileNotFoundException) { }
            catch (FileLoadException) { }
            catch (BadImageFormatException) { }
            
            if ((object)type == null)
            {
                type = Type.GetType(typeName, ObjectReader.ResolveSimpleAssemblyName, new TopLevelAssemblyTypeResolver(assm).ResolveType, false /* throwOnError */);
            }
        }


        private String previousAssemblyString;
        private String previousName;
        private Type previousType;
        //private int hit;
        
        [System.Security.SecurityCritical]  // auto-generated
        internal Type GetType(BinaryAssemblyInfo assemblyInfo, String name)
        {
            Type objectType = null;

            if (((previousName != null) && (previousName.Length == name.Length) && (previousName.Equals(name))) &&
                ((previousAssemblyString != null) && (previousAssemblyString.Length == assemblyInfo.assemblyString.Length) &&(previousAssemblyString.Equals(assemblyInfo.assemblyString))))
            {
                objectType = previousType;
                //Console.WriteLine("Hit "+(++hit)+" "+objectType);
            }
            else
            {
                objectType = Bind(assemblyInfo.assemblyString, name);
                if ((object)objectType == null)
                {
                    Assembly sourceAssembly = assemblyInfo.GetAssembly();

                    if (bSimpleAssembly)
                    {
                        ObjectReader.GetSimplyNamedTypeFromAssembly(sourceAssembly, name, ref objectType);
                    }
                    else
                    {
                        objectType = FormatterServices.GetTypeFromAssembly(sourceAssembly, name);
                    }

                    // here let us do the security check 
                    if (objectType != null)
                    {
                        CheckTypeForwardedTo(sourceAssembly, objectType.Assembly, objectType);
                    }
                }

                previousAssemblyString = assemblyInfo.assemblyString;
                previousName = name;
                previousType = objectType;
            }
            //Console.WriteLine("name "+name+" assembly "+assemblyInfo.assemblyString+" objectType "+objectType);
            return objectType;
        }

        [SecuritySafeCritical]
        private static void CheckTypeForwardedTo(Assembly sourceAssembly, Assembly destAssembly, Type resolvedType)
        {
            if ( !FormatterServices.UnsafeTypeForwardersIsEnabled() && sourceAssembly != destAssembly )
            {
                // we have a type forward to attribute !

                // we can try to see if the dest assembly has less permissionSet
                if (!destAssembly.PermissionSet.IsSubsetOf(sourceAssembly.PermissionSet))
                {
                    // let us try to see if typeforwardedfrom is there

                    // let us hit the cache first
                    TypeInformation typeInfo = BinaryFormatter.GetTypeInformation(resolvedType);
                    if (typeInfo.HasTypeForwardedFrom)
                    {
                        Assembly typeFowardedFromAssembly = null;
                        try
                        {
                            // if this Assembly.Load failed, we still want to throw security exception
                            typeFowardedFromAssembly = Assembly.Load(typeInfo.AssemblyString);
                        }
                        catch { }

                        if (typeFowardedFromAssembly != sourceAssembly)
                        {
                            // throw security exception
                            throw new SecurityException() { Demanded = sourceAssembly.PermissionSet };
                        }
                    }
                    else
                    {
                        // throw security exception
                        throw new SecurityException() { Demanded = sourceAssembly.PermissionSet };
                    }
                }
            }         
        }

        internal sealed class TopLevelAssemblyTypeResolver
        {
            private Assembly m_topLevelAssembly;

            public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
            {
                m_topLevelAssembly = topLevelAssembly;
            }

            public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                if (assembly == null)
                    assembly = m_topLevelAssembly;

                return assembly.GetType(simpleTypeName, false, ignoreCase);
            }
        }
    }
}
