// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
**  File:    RemotingAttributes.cs
** 
**  Purpose: Custom attributes for modifying remoting interactions.
**
**
===========================================================*/

namespace System.Runtime.Remoting.Metadata
{
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Reflection;
    using System.Threading;
    using System.Runtime.Serialization;
    using TypeInfo = System.Runtime.Remoting.TypeInfo;

    // This is the data we store in MemberInfo.CachedData, mainly to cache custom
    //   attributes.
    internal abstract class RemotingCachedData 
    {
        private SoapAttribute _soapAttr = null; // Soap related attributes derive from SoapAttribute

        // Retrieve SOAP attribute info for _mi (or create for caching if not specified)
        internal SoapAttribute GetSoapAttribute()
        {
            if (_soapAttr == null)
            {
                lock (this)
                {
                    if (_soapAttr == null)
                    {
                        _soapAttr = GetSoapAttributeNoLock();
                    } // if (_soapAttr == null)
                } // lock (this)
            }

            return _soapAttr;
        } // GetSoapAttribute

        abstract internal SoapAttribute GetSoapAttributeNoLock();
    } // class RemotingCachedData


    internal class RemotingFieldCachedData : RemotingCachedData
    {
        private FieldInfo RI;  // reflection structure on which this data structure is stored

        internal RemotingFieldCachedData(RuntimeFieldInfo ri)
        {
            RI = ri;
        }
        internal RemotingFieldCachedData(SerializationFieldInfo ri)
        {
            RI = ri;
        }

        // Retrieve SOAP attribute info for _mi (or create for caching if not specified)
        internal override SoapAttribute GetSoapAttributeNoLock()
        {
            SoapAttribute tempSoapAttr = null;

            Object[] attrs = RI.GetCustomAttributes(typeof(SoapFieldAttribute), false);
            if ((attrs != null) && (attrs.Length != 0))
                tempSoapAttr = (SoapAttribute)attrs[0];
            else
                tempSoapAttr = new SoapFieldAttribute();

            // IMPORTANT: This has to be done for certain values to be automatically
            //   generated in the attribute.
            tempSoapAttr.SetReflectInfo(RI);

            return tempSoapAttr;
        }
    }


    internal class RemotingParameterCachedData : RemotingCachedData
    {
        private RuntimeParameterInfo RI;  // reflection structure on which this data structure is stored

        internal RemotingParameterCachedData(RuntimeParameterInfo ri)
        {
            RI = ri;
        }

        internal override SoapAttribute GetSoapAttributeNoLock()
        {
            SoapAttribute tempSoapAttr = null;

            Object[] attrs = RI.GetCustomAttributes(typeof(SoapParameterAttribute), true);
            if ((attrs != null) && (attrs.Length != 0))
                tempSoapAttr = (SoapParameterAttribute)attrs[0];
            else
                tempSoapAttr = new SoapParameterAttribute();

            // IMPORTANT: This has to be done for certain values to be automatically
            //   generated in the attribute.
            tempSoapAttr.SetReflectInfo(RI);

            return tempSoapAttr;
        }
    }


    internal class RemotingTypeCachedData : RemotingCachedData
    {
        private RuntimeType RI;
        private class LastCalledMethodClass
        {
            public String      methodName;
            public MethodBase  MB;
        }

        private LastCalledMethodClass _lastMethodCalled; // cache for last method that was called
        private TypeInfo _typeInfo; // type info to be used for ObjRef's of this type
        private String _qualifiedTypeName;
        private String _assemblyName; 
        private String _simpleAssemblyName; // (no strong name, version, etc.)


        internal RemotingTypeCachedData(RuntimeType ri)
        {
            RI = ri;
        }

        internal override SoapAttribute GetSoapAttributeNoLock()
        {
            SoapAttribute tempSoapAttr = null;

            Object[] attrs = RI.GetCustomAttributes(typeof(SoapTypeAttribute), true);
            if ((attrs != null) && (attrs.Length != 0))
                tempSoapAttr = (SoapAttribute)attrs[0];
            else
                tempSoapAttr = new SoapTypeAttribute();

            // IMPORTANT: This has to be done for certain values to be automatically
            //   generated in the attribute.
            tempSoapAttr.SetReflectInfo(RI);

            return tempSoapAttr;
        }

        internal MethodBase GetLastCalledMethod(String newMeth)
        {
            LastCalledMethodClass lastMeth = _lastMethodCalled;                        
            if (lastMeth == null)
                return null;

            String methodName = lastMeth.methodName;
            MethodBase mbToReturn = lastMeth.MB;

            if (mbToReturn==null || methodName==null)
                return null;

            if (methodName.Equals(newMeth))
                return mbToReturn;

            return null;
        } // GetLastCalledMethod

        internal void SetLastCalledMethod(String newMethName, MethodBase newMB)
        {
            LastCalledMethodClass lastMeth = new LastCalledMethodClass();           
            lastMeth.methodName = newMethName;
            lastMeth.MB = newMB;

            _lastMethodCalled = lastMeth;
        } // SetLastCalledMethod


        // Retrieve TypeInfo object to be used in ObjRef.
        internal TypeInfo TypeInfo
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {   
                if (_typeInfo == null)
                    _typeInfo = new TypeInfo(RI);
                    
                return _typeInfo;
            }
        } // TypeInfo


        internal String QualifiedTypeName
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_qualifiedTypeName == null)
                    _qualifiedTypeName = RemotingServices.DetermineDefaultQualifiedTypeName(RI);

                return _qualifiedTypeName;
            }
        } // QualifiedTypeName


        internal String AssemblyName
        {
            get
            {
                if (_assemblyName == null)
                    _assemblyName = RI.Module.Assembly.FullName;

                return _assemblyName;
            }
        } // AssemblyName
            

        internal String SimpleAssemblyName
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_simpleAssemblyName == null)
                    _simpleAssemblyName = RI.GetRuntimeAssembly().GetSimpleName();

                return _simpleAssemblyName;
            }
        } // SimpleAssemblyName

    } // class RemotingTypeCachedData


    internal class RemotingMethodCachedData : RemotingCachedData
    {
        private MethodBase RI;
        private ParameterInfo[] _parameters = null; // list of parameters (cached because reflection always
                                                    //   generates a new copy of this array)

        [Serializable]
        [Flags]
        private enum MethodCacheFlags
        {
            None                 = 0x00,
            CheckedOneWay        = 0x01, // Have we checked for OneWay attribute?
            IsOneWay             = 0x02, // Is the OneWay attribute present?
            CheckedOverloaded    = 0x04, // Have we checked to see if this method is overloaded
            IsOverloaded         = 0x08, // Is the method overloaded?
            CheckedForAsync      = 0x10, // Have we looked for async versions of this method?
            CheckedForReturnType = 0x20, // Have we looked for the return type?
        }

        private MethodCacheFlags flags;

        // Names
        private String _typeAndAssemblyName = null;
        private String _methodName = null;
        private Type _returnType = null; // null if return type is void or .ctor

        // parameter maps
        // NOTE: these fields are all initialized at the same time however access to 
        // the internal property of each field is locked only on that specific field 
        // having been initialized. - [....]
        private int[] _inRefArgMap = null;     // parameter map of input and ref parameters
        private int[] _outRefArgMap = null;     // parameter map of out and ref parameters (exactly all byref parameters)
        private int[] _outOnlyArgMap = null;   // parameter map of only output parameters
        private int[] _nonRefOutArgMap = null; // parameter map of non byref parameters marked with [In, Out] (or [Out])

        private int[] _marshalRequestMap = null;  // map of parameters that should be marshaled in
        private int[] _marshalResponseMap = null; // map of parameters that should be marshaled out

        internal RemotingMethodCachedData(RuntimeMethodInfo ri)
        {
            RI = ri;
        }

        internal RemotingMethodCachedData(RuntimeConstructorInfo ri)
        {
            RI = ri;
        }

        internal override SoapAttribute GetSoapAttributeNoLock()
        {
            SoapAttribute tempSoapAttr = null;

            Object[] attrs = RI.GetCustomAttributes(typeof(SoapMethodAttribute), true);
            if ((attrs != null) && (attrs.Length != 0))
                tempSoapAttr = (SoapAttribute)attrs[0];
            else
                tempSoapAttr = new SoapMethodAttribute();

            // IMPORTANT: This has to be done for certain values to be automatically
            //   generated in the attribute.
            tempSoapAttr.SetReflectInfo(RI);

            return tempSoapAttr;
        }

        internal String TypeAndAssemblyName
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_typeAndAssemblyName == null)
                    UpdateNames();
                return _typeAndAssemblyName;
            }
        } // TypeAndAssemblyName

        internal String MethodName
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_methodName == null)
                    UpdateNames();
                return _methodName;
            }
        } // MethodName

        [System.Security.SecurityCritical]  // auto-generated
        private void UpdateNames()
        {
            MethodBase mb = RI;
            _methodName = mb.Name;
            if (mb.DeclaringType != null) {
                _typeAndAssemblyName = RemotingServices.GetDefaultQualifiedTypeName((RuntimeType)mb.DeclaringType);       
            }
        } // UpdateNames
        
        internal ParameterInfo[] Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = RI.GetParameters();
                return _parameters;
            }
        } // Parameters


        // contains index of all byref parameters (marked as "out" or "ref")
        internal int[] OutRefArgMap
        {
            get 
            {
                if (_outRefArgMap == null)
                    GetArgMaps();
                return _outRefArgMap;
            }
        } // OutRefArgMap

        // contains index of parameters marked as out
        internal int[] OutOnlyArgMap
        {
            get
            {
                if (_outOnlyArgMap == null)
                    GetArgMaps();
                return _outOnlyArgMap;
            }
        } // OutOnlyArgMap

        // contains index of non byref parameters marked with [In, Out]
        internal int[] NonRefOutArgMap
        {
            get
            {
                if (_nonRefOutArgMap == null)
                    GetArgMaps();
                return _nonRefOutArgMap;
            }
        } // NonRefOutArgMap

        // contains index of parameters that should be marshalled for a request        
        internal int[] MarshalRequestArgMap
        {
            get
            {
                if (_marshalRequestMap == null)
                    GetArgMaps();
                return _marshalRequestMap;
            }
        } // MarshalRequestMap

        // contains index of parameters that should be marshalled for a response
        internal int[] MarshalResponseArgMap
        {
            get
            {
                if (_marshalResponseMap == null)
                    GetArgMaps();
                return _marshalResponseMap;
            }
        } // MarshalResponseArgMap


        private void GetArgMaps()
        {
            lock (this)
            {
                if (_inRefArgMap == null)
                {
                    int[] inRefArgMap = null;
                    int[] outRefArgMap = null;
                    int[] outOnlyArgMap = null;
                    int[] nonRefOutArgMap = null;
                    int[] marshalRequestMap = null;
                    int[] marshalResponseMap = null;
                
                    ArgMapper.GetParameterMaps(Parameters,
                        out inRefArgMap, out outRefArgMap, out outOnlyArgMap, 
                        out nonRefOutArgMap,
                        out marshalRequestMap, out marshalResponseMap);

                    _inRefArgMap = inRefArgMap;
                    _outRefArgMap = outRefArgMap;
                    _outOnlyArgMap = outOnlyArgMap;
                    _nonRefOutArgMap = nonRefOutArgMap;
                    _marshalRequestMap = marshalRequestMap;
                    _marshalResponseMap = marshalResponseMap;
                    
                }
            }
        } // GetArgMaps
                                
        internal bool IsOneWayMethod()
        {
            // We are not protecting against a ----
            // If there is a ---- while setting flags
            // we will have to compute the result again, 
            // but we will always return the correct result
            // 
            if ((flags & MethodCacheFlags.CheckedOneWay) == 0)
            {
                MethodCacheFlags isOneWay = MethodCacheFlags.CheckedOneWay;
                Object[] attrs = RI.GetCustomAttributes(typeof(OneWayAttribute), true);
                    
                if ((attrs != null) && (attrs.Length > 0))
                    isOneWay |= MethodCacheFlags.IsOneWay;
                
                flags |= isOneWay;
                return (isOneWay & MethodCacheFlags.IsOneWay) != 0;
            }
            return (flags & MethodCacheFlags.IsOneWay) != 0;
        } // IsOneWayMethod

        internal bool IsOverloaded()
        {
            // We are not protecting against a ----
            // If there is a ---- while setting flags
            // we will have to compute the result again, 
            // but we will always return the correct result
            // 
            if ((flags & MethodCacheFlags.CheckedOverloaded) == 0)
            {
                MethodCacheFlags isOverloaded = MethodCacheFlags.CheckedOverloaded;
                MethodBase mb = RI;

                RuntimeMethodInfo rmi = null;
                RuntimeConstructorInfo rci = null;

                if ((rmi = mb as RuntimeMethodInfo) != null)
                {
                    if (rmi.IsOverloaded)
                        isOverloaded |= MethodCacheFlags.IsOverloaded;
                }
                else if ((rci = mb as RuntimeConstructorInfo) != null)
                {
                    if (rci.IsOverloaded)
                        isOverloaded |= MethodCacheFlags.IsOverloaded;
                }
                else
                {
                    throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_Method"));
                }

                flags |= isOverloaded;
            }

            return (flags & MethodCacheFlags.IsOverloaded) != 0;
        } // IsOverloaded


        // This will return the return type of the method, or null
        // if the return type is void or this is a .ctor.
        internal Type ReturnType
        {
            get
            {
                if ((flags & MethodCacheFlags.CheckedForReturnType) == 0)
                {
                    MethodInfo mi = RI as MethodInfo;
                    if (mi != null)
                    {
                        Type returnType = mi.ReturnType;
                        if (returnType != typeof(void))
                            _returnType = returnType;
                    }
                
                    flags |= MethodCacheFlags.CheckedForReturnType;
                }
               
                return _returnType;
            } // get
        } // ReturnType

    } // class RemotingMethodCachedData


    //
    // SOAP ATTRIBUTES
    // 

    // Options for use with SoapOptionAttribute (combine with OR to get any combination)
    [Serializable]
    [Flags]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum SoapOption
    {
        None = 0x0,
        AlwaysIncludeTypes = 0x1, // xsi:type always included on SOAP elements
        XsdString = 0x2, // xsi:type always included on SOAP elements    
        EmbedAll = 0x4, // Soap will be generated without references
        /// <internalonly/>
        Option1 = 0x8, // Option for temporary interop conditions, the use will change over time
        /// <internalonly/>
        Option2 = 0x10, // Option for temporary interop conditions, the use will change over time
    } // SoapOption

    /// <internalonly/>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum XmlFieldOrderOption
    {
        All,
        Sequence,
        Choice
    } // XmlFieldOrderOption 

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapTypeAttribute : SoapAttribute
    {    
        // Used to track which values have been explicitly set. Information needed
        // by SoapServices.
        [Serializable]
        [Flags]
        private enum ExplicitlySet
        {
            None = 0x0,
            XmlElementName = 0x1,
            XmlNamespace = 0x2,
            XmlTypeName = 0x4,
            XmlTypeNamespace = 0x8
        }

        private ExplicitlySet _explicitlySet = ExplicitlySet.None;        
    
        private SoapOption           _SoapOptions = SoapOption.None;
        private String               _XmlElementName = null;
        private String               _XmlTypeName = null;
        private String               _XmlTypeNamespace = null;
        private XmlFieldOrderOption  _XmlFieldOrder = XmlFieldOrderOption.All;


        // Returns true if this attribute specifies interop xml element values.
        internal bool IsInteropXmlElement()
        {
            return (_explicitlySet & (ExplicitlySet.XmlElementName | ExplicitlySet.XmlNamespace)) != 0;
        } // IsInteropXmlElement       

        internal bool IsInteropXmlType()
        {
            return (_explicitlySet & (ExplicitlySet.XmlTypeName | ExplicitlySet.XmlTypeNamespace)) != 0;
        } // IsInteropXmlType
        

        
        public SoapOption SoapOptions
        {
            get { return _SoapOptions; }
            set { _SoapOptions = value; }
        } // SoapOptions

        public String XmlElementName
        {
            get 
            {
                // generate this if it hasn't been set yet
                if ((_XmlElementName == null) && (ReflectInfo != null))
                    _XmlElementName = GetTypeName((Type)ReflectInfo);
                return _XmlElementName; 
            }
            
            set 
            {
                _XmlElementName = value; 
                _explicitlySet |= ExplicitlySet.XmlElementName;
            }
        } // XmlElementName

        public override String XmlNamespace
        {
            get 
            {
                // generate this if it hasn't been set
                if ((ProtXmlNamespace == null) && (ReflectInfo != null))
                {
                    ProtXmlNamespace = XmlTypeNamespace;
                }
                return ProtXmlNamespace;
            }
            
            set 
            { 
                ProtXmlNamespace = value; 
                _explicitlySet |= ExplicitlySet.XmlNamespace;
            }
        } // XmlNamespace

        public String XmlTypeName // value for xml type name (this should always be valid)
        {
            get 
            { 
                // generate this if it hasn't been set yet
                if ((_XmlTypeName == null) && (ReflectInfo != null))
                    _XmlTypeName = GetTypeName((Type)ReflectInfo);
                return _XmlTypeName; 
            }
            
            set 
            {
                _XmlTypeName = value; 
                _explicitlySet |= ExplicitlySet.XmlTypeName;
            }
        } // XmlTypeName

        public String XmlTypeNamespace // value for xml type namespace (this should always be valid)
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get 
            {
                // generate this if it hasn't been set yet
                if ((_XmlTypeNamespace == null) && (ReflectInfo != null))
                {
                    _XmlTypeNamespace = 
                        XmlNamespaceEncoder.GetXmlNamespaceForTypeNamespace((RuntimeType)ReflectInfo, null);
                }
                return _XmlTypeNamespace; 
            }
            
            set 
            { 
                _XmlTypeNamespace = value; 
                _explicitlySet |= ExplicitlySet.XmlTypeNamespace;
            }
        } // XmlTypeNamespace

        /// <internalonly/>
        public XmlFieldOrderOption XmlFieldOrder
        {
            get { return _XmlFieldOrder; }
            set { _XmlFieldOrder = value; }
        } // XmlFieldOrder

        public override bool UseAttribute
        {
            get { return false; }
            set { throw new RemotingException(
                                Environment.GetResourceString("Remoting_Attribute_UseAttributeNotsettable")); }
        } // UseAttribute
        
        private static string GetTypeName(Type t) {
            if (t.IsNested) {
                string name = t.FullName;
                string ns = t.Namespace;
                if (ns == null || ns.Length == 0) {
                    return name;
                }
                else {
                    name = name.Substring(ns.Length + 1);
                    return name;
                }
            }
            return t.Name;
        }

    } // class SoapTypeAttribute


    [AttributeUsage(AttributeTargets.Method)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapMethodAttribute : SoapAttribute
    {       
        private String _SoapAction = null;

        private String _responseXmlElementName = null;
        private String _responseXmlNamespace = null;
        private String _returnXmlElementName = null;

        private bool _bSoapActionExplicitySet = false; // Needed by SoapServices to determine if
                                                       // SoapAction was actually set (otherwise,
                                                       // accessing it will return a generated
                                                       // value)    

        internal bool SoapActionExplicitySet { get { return _bSoapActionExplicitySet; } }

        public String SoapAction // SoapAction value to place in protocol headers.
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get 
            {
                // generate this if it hasn't been set
                if (_SoapAction == null)
                {
                    _SoapAction = XmlTypeNamespaceOfDeclaringType + "#" +
                        ((MemberInfo)ReflectInfo).Name; // This will be the method name.
                }
                return _SoapAction; 
            }
                
            set 
            {
                _SoapAction = value; 
                _bSoapActionExplicitySet = true;
            }
        }

        public override bool UseAttribute
        {
            get { return false; }
            set { throw new RemotingException(
                                Environment.GetResourceString("Remoting_Attribute_UseAttributeNotsettable")); }
        }

        public override String XmlNamespace
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get 
            {
                // generate this if it hasn't been set
                if (ProtXmlNamespace == null)
                {
                    ProtXmlNamespace = XmlTypeNamespaceOfDeclaringType;
                }
                return ProtXmlNamespace;
            }
            
            set { ProtXmlNamespace = value; }
        } // XmlNamespace


        public String ResponseXmlElementName
        {
            get 
            {
                // generate this if it hasn't been set yet
                if ((_responseXmlElementName == null) && (ReflectInfo != null))                
                    _responseXmlElementName = ((MemberInfo)ReflectInfo).Name + "Response";
                return _responseXmlElementName; 
            }
            
            set { _responseXmlElementName = value; }
        } // ResponseXmlElementName
        

        public String ResponseXmlNamespace
        {
            get 
            {
                // generate this if it hasn't been set
                if (_responseXmlNamespace == null)
                    _responseXmlNamespace = XmlNamespace;
                return _responseXmlNamespace;
            }
            
            set { _responseXmlNamespace = value; }
        } // ResponseXmlNamespace


        public String ReturnXmlElementName
        {
            get 
            {
                // generate this if it hasn't been set yet
                if (_returnXmlElementName == null)                
                    _returnXmlElementName = "return";
                return _returnXmlElementName; 
            }
            
            set { _returnXmlElementName = value; }
        } // ReturnXmlElementName


        private String XmlTypeNamespaceOfDeclaringType 
        { 
            [System.Security.SecurityCritical]  // auto-generated
            get 
            { 
                if (ReflectInfo != null)
                {
                    Type declaringType = ((MemberInfo)ReflectInfo).DeclaringType;
                    return XmlNamespaceEncoder.GetXmlNamespaceForType((RuntimeType)declaringType, null);
                }
                else
                    return null;
            }
        } // XmlTypeNamespaceOfDeclaringType

    } // class SoapMethodAttribute


    [AttributeUsage(AttributeTargets.Field)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapFieldAttribute : SoapAttribute
    {
        // Used to track which values have been explicitly set. Information needed
        // by SoapServices.
        [Serializable]
        [Flags]
        private enum ExplicitlySet
        {
            None = 0x0,
            XmlElementName = 0x1
        }

        private ExplicitlySet _explicitlySet = ExplicitlySet.None;
    

        private String _xmlElementName = null;  
        private int _order; // order in which fields should be serialized 
                            //  (if Sequence is specified on containing type's SoapTypeAttribute


        // Returns true if this attribute specifies interop xml element values.
        public bool IsInteropXmlElement()
        {
            return (_explicitlySet & ExplicitlySet.XmlElementName) != 0;
        } // GetInteropXmlElement        


        public String XmlElementName
        {
            get 
            {
                // generate this if it hasn't been set yet
                if ((_xmlElementName == null) && (ReflectInfo != null))                
                    _xmlElementName = ((FieldInfo)ReflectInfo).Name;
                return _xmlElementName; 
            }
            
            set 
            {
                _xmlElementName = value; 
                _explicitlySet |= ExplicitlySet.XmlElementName;
            }
        } // XmlElementName


        /// <internalonly/>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }
        
    } // class SoapFieldAttribute


    [AttributeUsage(AttributeTargets.Parameter)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapParameterAttribute : SoapAttribute
    {
    } // SoapParameterAttribute



    // Not actually used as an attribute (just the base for the rest of them)
    [System.Runtime.InteropServices.ComVisible(true)]
    public class SoapAttribute : Attribute
    {
        /// <internalonly/>
        protected String  ProtXmlNamespace = null;
        private   bool    _bUseAttribute = false;
        private   bool    _bEmbedded = false;
        
        /// <internalonly/>
        protected Object  ReflectInfo = null; // Reflection structure on which this attribute was defined

        // IMPORTANT: The caching mechanism is required to set this value before
        //   handing back a SoapAttribute, so that certain values can be automatically
        //   generated.
        internal void SetReflectInfo(Object info)
        {
            ReflectInfo = info;
        }

        public virtual String XmlNamespace // If this returns null, then this shouldn't be namespace qualified.
        {
            get { return ProtXmlNamespace; }
            set { ProtXmlNamespace = value; }
        }
            
        public virtual bool UseAttribute 
        {
            get { return _bUseAttribute; }
            set { _bUseAttribute = value; }
        }

        public virtual bool Embedded // Determines if type should be nested when serializing for SOAP.
        {
            get { return _bEmbedded; }
            set { _bEmbedded = value; }
        }
        
    } // class SoapAttribute


    //
    // END OF SOAP ATTRIBUTES
    //
    

} // namespace System.Runtime.Remoting

