// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    Soap.cs
**
** Purpose: Classes used for SOAP configuration.
**
**
===========================================================*/
namespace System.Runtime.Remoting {
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Threading;
    using System.Diagnostics.Contracts;
    
    using CultureInfo = System.Globalization.CultureInfo;


    [System.Security.SecurityCritical]  // auto-generated_required
    [System.Runtime.InteropServices.ComVisible(true)]
    public class SoapServices
    {
        // hide the default constructor
        private SoapServices()
        {
        }
    
        // tables for interop type maps (both map "name namespace" to a Type object)
        private static Hashtable _interopXmlElementToType = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _interopTypeToXmlElement = Hashtable.Synchronized(new Hashtable());

        private static Hashtable _interopXmlTypeToType = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _interopTypeToXmlType = Hashtable.Synchronized(new Hashtable());

        private static Hashtable _xmlToFieldTypeMap = Hashtable.Synchronized(new Hashtable());
        
        private static Hashtable _methodBaseToSoapAction = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _soapActionToMethodBase = Hashtable.Synchronized(new Hashtable());
        

        private static String CreateKey(String elementName, String elementNamespace)
        {
            if (elementNamespace == null)
                return elementName;
            else
                return elementName + " " + elementNamespace;
        }

        // Used for storing interop type mappings
        private class XmlEntry
        {
            public String Name;
            public String Namespace;

            public XmlEntry(String name, String xmlNamespace)
            {
                Name = name;
                Namespace = xmlNamespace;
            }
        } // class XmlEntry


        // contains mappings for xml element and attribute names to actual field names.
        private class XmlToFieldTypeMap
        {
            private class FieldEntry
            {
                public Type Type;
                public String Name;

                public FieldEntry(Type type, String name)
                {
                    Type = type;
                    Name = name;
                }
            }
        
            private Hashtable _attributes = new Hashtable();
            private Hashtable _elements = new Hashtable();

            public XmlToFieldTypeMap(){}

            [System.Security.SecurityCritical]  // auto-generated
            public void AddXmlElement(Type fieldType, String fieldName,
                                      String xmlElement, String xmlNamespace)
            {
                _elements[CreateKey(xmlElement, xmlNamespace)] =
                    new FieldEntry(fieldType, fieldName);                    
            }

            [System.Security.SecurityCritical]  // auto-generated
            public void AddXmlAttribute(Type fieldType, String fieldName,
                                        String xmlAttribute, String xmlNamespace)
            {
                _attributes[CreateKey(xmlAttribute, xmlNamespace)] =
                    new FieldEntry(fieldType, fieldName);
            }

            [System.Security.SecurityCritical]  // auto-generated
            public void GetFieldTypeAndNameFromXmlElement(String xmlElement, String xmlNamespace,
                                                          out Type type, out String name)
            {
                FieldEntry field = (FieldEntry)_elements[CreateKey(xmlElement, xmlNamespace)];
                if (field != null)
                {
                    type = field.Type;
                    name = field.Name;
                }
                else
                {
                    type = null;
                    name = null;
                }
            } // GetTypeFromXmlElement
            
            [System.Security.SecurityCritical]  // auto-generated
            public void GetFieldTypeAndNameFromXmlAttribute(String xmlAttribute, String xmlNamespace,
                                                            out Type type, out String name)
            {
                FieldEntry field = (FieldEntry)_attributes[CreateKey(xmlAttribute, xmlNamespace)];
                if (field != null)
                {
                    type = field.Type;
                    name = field.Name;
                }
                else
                {
                    type = null;
                    name = null;
                }
            } // GetTypeFromXmlAttribute
            
        } // class XmlToFieldTypeMap

    
        [System.Security.SecurityCritical]  // auto-generated
        public static void RegisterInteropXmlElement(String xmlElement, String xmlNamespace,
                                                     Type type)
        {
            _interopXmlElementToType[CreateKey(xmlElement, xmlNamespace)] = type;
            _interopTypeToXmlElement[type] = new XmlEntry(xmlElement, xmlNamespace);
        } // RegisterInteropXmlElement


        [System.Security.SecurityCritical]  // auto-generated
        public static void RegisterInteropXmlType(String xmlType, String xmlTypeNamespace,
                                                  Type type)
        {
            _interopXmlTypeToType[CreateKey(xmlType, xmlTypeNamespace)] = type;
            _interopTypeToXmlType[type] = new XmlEntry(xmlType, xmlTypeNamespace);
        } // RegisterInteropXmlType


        [System.Security.SecurityCritical]  // auto-generated
        public static void PreLoad(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            Contract.EndContractBlock();

            if (!(type is RuntimeType))
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            // register soap action values
            MethodInfo[] methods = type.GetMethods();
            foreach (MethodInfo mi in methods)
            {
                // This will only add an entry to the table if SoapAction was explicitly set
                //   on the SoapMethodAttribute.
                RegisterSoapActionForMethodBase(mi);
            }

            // register interop xml elements and types if specified
            SoapTypeAttribute attr = (SoapTypeAttribute)
                InternalRemotingServices.GetCachedSoapAttribute(type);

            if (attr.IsInteropXmlElement())
                RegisterInteropXmlElement(attr.XmlElementName, attr.XmlNamespace, type);
            if (attr.IsInteropXmlType())
                RegisterInteropXmlType(attr.XmlTypeName, attr.XmlTypeNamespace, type);

            // construct field maps for mapping xml elements and attributes back to
            //   the correct type            
            int mapCount = 0;
            XmlToFieldTypeMap map = new XmlToFieldTypeMap();

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                SoapFieldAttribute fieldAttr = (SoapFieldAttribute)
                    InternalRemotingServices.GetCachedSoapAttribute(field);

                if (fieldAttr.IsInteropXmlElement())
                {
                    String xmlName = fieldAttr.XmlElementName;
                    String xmlNamespace = fieldAttr.XmlNamespace;
                    if (fieldAttr.UseAttribute)
                        map.AddXmlAttribute(field.FieldType, field.Name, xmlName, xmlNamespace);
                    else
                        map.AddXmlElement(field.FieldType, field.Name, xmlName, xmlNamespace);

                    mapCount++;
                }
            } // foreach field

            // add field map if there is more than one entry
            if (mapCount > 0)
                _xmlToFieldTypeMap[type] = map;

        } // PreLoad


        [System.Security.SecurityCritical]  // auto-generated
        public static void PreLoad(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();

            if (!(assembly is RuntimeAssembly))
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "assembly");

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                PreLoad(type);
            }
        } // PreLoad       
                                                             
        [System.Security.SecurityCritical]  // auto-generated
        public static Type GetInteropTypeFromXmlElement(String xmlElement, String xmlNamespace)
        {
            return (Type)_interopXmlElementToType[CreateKey(xmlElement, xmlNamespace)];
        } // GetInteropTypeFromXmlElement


        [System.Security.SecurityCritical]  // auto-generated
        public static Type GetInteropTypeFromXmlType(String xmlType, String xmlTypeNamespace)
        {
            return (Type)_interopXmlTypeToType[CreateKey(xmlType, xmlTypeNamespace)];
        } // GetInteropTypeFromXmlElement     


        public static void GetInteropFieldTypeAndNameFromXmlElement(Type containingType,
            String xmlElement, String xmlNamespace,
            out Type type, out String name)
        {        
            if (containingType == null)
            {
                type = null;
                name = null;
                return;
            }
        
            XmlToFieldTypeMap map = (XmlToFieldTypeMap)_xmlToFieldTypeMap[containingType];
            if (map != null)            
            {
                map.GetFieldTypeAndNameFromXmlElement(xmlElement, xmlNamespace,
                                                      out type, out name);
            }
            else
            {
                type = null;
                name = null;
            }
        } // GetInteropFieldTypeFromXmlElement


        public static void GetInteropFieldTypeAndNameFromXmlAttribute(Type containingType,
            String xmlAttribute, String xmlNamespace,
            out Type type, out String name)
        {
            if (containingType == null)
            {
                type = null;
                name = null;
                return;
            }
        
            XmlToFieldTypeMap map = (XmlToFieldTypeMap)_xmlToFieldTypeMap[containingType];
            if (map != null)
            {
                map.GetFieldTypeAndNameFromXmlAttribute(xmlAttribute, xmlNamespace,
                                                        out type, out name);
            }
            else
            {
                type = null;
                name = null;
            }
        } // GetInteropFieldTypeFromXmlAttribute


        [System.Security.SecurityCritical]  // auto-generated
        public static bool GetXmlElementForInteropType(Type type, 
                                                       out String xmlElement, out String xmlNamespace)
        {
            // check table first
            XmlEntry entry = (XmlEntry)_interopTypeToXmlElement[type];
            if (entry != null)
            {
                xmlElement = entry.Name;
                xmlNamespace = entry.Namespace;
                return true;
            }

            // check soap attribute
            SoapTypeAttribute attr = (SoapTypeAttribute)
                InternalRemotingServices.GetCachedSoapAttribute(type);

            if (attr.IsInteropXmlElement())
            {
                xmlElement = attr.XmlElementName;
                xmlNamespace = attr.XmlNamespace;
                return true;
            }
            else
            {
                xmlElement = null;
                xmlNamespace = null;
                return false;
            }
        } // GetXmlElementForInteropType


        [System.Security.SecurityCritical]  // auto-generated
        public static bool GetXmlTypeForInteropType(Type type, 
                                                    out String xmlType, out String xmlTypeNamespace)
        {
            // check table first
            XmlEntry entry = (XmlEntry)_interopTypeToXmlType[type];
            if (entry != null)
            {
                xmlType = entry.Name;
                xmlTypeNamespace = entry.Namespace;
                return true;
            }

            // check soap attribute
            SoapTypeAttribute attr = (SoapTypeAttribute)
                InternalRemotingServices.GetCachedSoapAttribute(type);

            if (attr.IsInteropXmlType())
            {
                xmlType = attr.XmlTypeName;
                xmlTypeNamespace = attr.XmlTypeNamespace;
                return true;
            }
            else
            {
                xmlType = null;
                xmlTypeNamespace = null;
                return false;
            }           
        } // GetXmlTypeForInteropType


        [System.Security.SecurityCritical]  // auto-generated
        public static String GetXmlNamespaceForMethodCall(MethodBase mb)
        {
            SoapMethodAttribute attr = (SoapMethodAttribute)
                InternalRemotingServices.GetCachedSoapAttribute(mb);
            return attr.XmlNamespace;
        } // GetXmlNamespaceForMethodCall


        [System.Security.SecurityCritical]  // auto-generated
        public static String GetXmlNamespaceForMethodResponse(MethodBase mb)
        {
            SoapMethodAttribute attr = (SoapMethodAttribute)
                InternalRemotingServices.GetCachedSoapAttribute(mb);
            return attr.ResponseXmlNamespace;
        } // GetXmlNamespaceForMethodResponse


        [System.Security.SecurityCritical]  // auto-generated
        public static void RegisterSoapActionForMethodBase(MethodBase mb)
        {
            SoapMethodAttribute attr = 
                     (SoapMethodAttribute)InternalRemotingServices.GetCachedSoapAttribute(mb);
            if (attr.SoapActionExplicitySet)
                RegisterSoapActionForMethodBase(mb, attr.SoapAction);
        } // RegisterSoapActionForMethodBase
        

        public static void RegisterSoapActionForMethodBase(MethodBase mb, String soapAction)
        {
            if (soapAction != null)
            {            
                _methodBaseToSoapAction[mb] = soapAction;

                // get table of method bases
                ArrayList mbTable = (ArrayList)_soapActionToMethodBase[soapAction];
                if (mbTable == null)
                {
                    lock (_soapActionToMethodBase)
                    {
                        mbTable = ArrayList.Synchronized(new ArrayList());
                        _soapActionToMethodBase[soapAction] = mbTable;
                    }
                }
                mbTable.Add(mb);
            }
        } // RegisterSoapActionForMethodBase



        [System.Security.SecurityCritical]  // auto-generated
        public static String GetSoapActionFromMethodBase(MethodBase mb)
        {
            String soapAction = (String)_methodBaseToSoapAction[mb];

            if (soapAction == null)
            {                      
                SoapMethodAttribute attr = 
                     (SoapMethodAttribute)InternalRemotingServices.GetCachedSoapAttribute(mb);
                soapAction = attr.SoapAction;
            }
            
            return soapAction;
        } // GetSoapActionFromMethodBase


        [System.Security.SecurityCritical]  // auto-generated
        public static bool IsSoapActionValidForMethodBase(String soapAction, MethodBase mb)
        {
            if (mb == null)
                throw new ArgumentNullException("mb");

            // remove quotation marks if present
            if ((soapAction[0] == '"') && (soapAction[soapAction.Length - 1] == '"'))
                soapAction = soapAction.Substring(1, soapAction.Length - 2);

            // compare this to the soapAction on the method base
            SoapMethodAttribute attr = (SoapMethodAttribute)
                InternalRemotingServices.GetCachedSoapAttribute(mb);
            if (String.CompareOrdinal(attr.SoapAction, soapAction) == 0)
                return true;

            // check to see if a soap action value is registered for this
            String registeredSoapAction = (String)_methodBaseToSoapAction[mb];
            if (registeredSoapAction != null)
            {
                if (String.CompareOrdinal(registeredSoapAction, soapAction) == 0)
                    return true;
            }

            // otherwise, parse SOAPAction and verify
            String typeName, methodName;
            
            String[] parts = soapAction.Split(new char[1]{'#'});
            if (parts.Length == 2)
            {
                bool assemblyIncluded;
                typeName = XmlNamespaceEncoder.GetTypeNameForSoapActionNamespace(
                    parts[0], out assemblyIncluded);
                if (typeName == null)
                    return false;

                methodName = parts[1];
                
                // compare to values of method base (
                RuntimeMethodInfo rmi = mb as RuntimeMethodInfo;
                RuntimeConstructorInfo rci = mb as RuntimeConstructorInfo;

                RuntimeModule rtModule;
                if (rmi != null)
                    rtModule = rmi.GetRuntimeModule();
                else if (rci != null)
                    rtModule = rci.GetRuntimeModule();
                else
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));

                String actualTypeName = mb.DeclaringType.FullName;
                if (assemblyIncluded)
                    actualTypeName += ", " + rtModule.GetRuntimeAssembly().GetSimpleName();

                // return true if type and method name are the same
                return actualTypeName.Equals(typeName) && mb.Name.Equals(methodName);
            }
            else
                return false;            
        } // IsSoapActionValidForMethodBase


        public static bool GetTypeAndMethodNameFromSoapAction(String soapAction,
                                                              out String typeName,
                                                              out String methodName)
        {             
            // remove quotation marks if present
            if ((soapAction[0] == '"') && (soapAction[soapAction.Length - 1] == '"'))
                soapAction = soapAction.Substring(1, soapAction.Length - 2);

            ArrayList mbTable = (ArrayList)_soapActionToMethodBase[soapAction];
            if (mbTable != null)
            {
                // indicate that we can't resolve soap action to type and method name
                if (mbTable.Count > 1)
                {
                    typeName = null;
                    methodName = null;
                    return false;
                }
        
                MethodBase mb = (MethodBase)mbTable[0];
                if (mb != null)
                {
                    // compare to values of method base (
                    RuntimeMethodInfo rmi = mb as RuntimeMethodInfo;
                    RuntimeConstructorInfo rci = mb as RuntimeConstructorInfo;

                    RuntimeModule rtModule;
                    if (rmi != null)
                        rtModule = rmi.GetRuntimeModule();
                    else if (rci != null)
                        rtModule = rci.GetRuntimeModule();
                    else
                        throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));

                    typeName = mb.DeclaringType.FullName + ", " + rtModule.GetRuntimeAssembly().GetSimpleName();
                    methodName = mb.Name;
                    return true;
                }                
            }

    
            String[] parts = soapAction.Split(new char[1]{'#'});
            if (parts.Length == 2)
            {
                bool assemblyIncluded;
                typeName = XmlNamespaceEncoder.GetTypeNameForSoapActionNamespace(
                    parts[0], out assemblyIncluded);
                if (typeName == null)
                {
                    methodName = null;
                    return false;
                }

                methodName = parts[1];
                return true;
            }
            else
            {
                typeName = null;
                methodName = null;
                return false;
            }

        } // GetTypeAndMethodNameFromSoapAction


        //Future namespaces might be
        // urn:a.clr.ms.com/assembly
        // urn:n.clr.ms.com/typeNamespace
        // urn:f.clr.ms.com/typeNamespace/assembly

        //namespaces are 
        // http://schemas.microsoft.com/clr/assem/assembly
        // http://schemas.microsoft.com/clr/ns/typeNamespace
        // http://schemas.microsoft.com/clr/nsassem/typeNamespace/assembly

        internal static String startNS = "http://schemas.microsoft.com/clr/";
        internal static String assemblyNS = "http://schemas.microsoft.com/clr/assem/";
        internal static String namespaceNS = "http://schemas.microsoft.com/clr/ns/";
        internal static String fullNS = "http://schemas.microsoft.com/clr/nsassem/";

        public static String XmlNsForClrType
        {
            get {return startNS;}
        }

        public static String XmlNsForClrTypeWithAssembly
        {
            get {return assemblyNS;}
        }

        public static String XmlNsForClrTypeWithNs
        {
            get {return namespaceNS;}
        }

        public static String XmlNsForClrTypeWithNsAndAssembly
        {
            get {return fullNS;}
        }

        public static bool IsClrTypeNamespace(String namespaceString)
        {
            if (namespaceString.StartsWith(startNS, StringComparison.Ordinal))
                return true;
            else
                return false;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public static String CodeXmlNamespaceForClrTypeNamespace(String typeNamespace, String assemblyName)
        {
            StringBuilder sb = new StringBuilder(256);
            if (IsNameNull(typeNamespace))
            {
                if (IsNameNull(assemblyName))
                    throw new ArgumentNullException("typeNamespace"+",assemblyName");
                else
                {
                    sb.Append(assemblyNS);
                    UriEncode(assemblyName, sb);
                }
            }
            else if (IsNameNull(assemblyName))
            {
                sb.Append(namespaceNS);
                sb.Append(typeNamespace);
            }
            else
            {
                sb.Append(fullNS);
                if (typeNamespace[0] == '.')
                    sb.Append(typeNamespace.Substring(1));
                else
                    sb.Append(typeNamespace);
                sb.Append('/');
                UriEncode(assemblyName, sb);
            }
            return sb.ToString();
        }

        [System.Security.SecurityCritical]  // auto-generated
        public static bool DecodeXmlNamespaceForClrTypeNamespace(String inNamespace, out String typeNamespace, out String assemblyName)
        {
            if (IsNameNull(inNamespace))
                throw new ArgumentNullException("inNamespace");

            assemblyName = null;
            typeNamespace = "";

            if (inNamespace.StartsWith(assemblyNS, StringComparison.Ordinal))
                assemblyName = UriDecode(inNamespace.Substring(assemblyNS.Length));
            else if (inNamespace.StartsWith(namespaceNS, StringComparison.Ordinal))
                typeNamespace = inNamespace.Substring(namespaceNS.Length);
            else if (inNamespace.StartsWith(fullNS, StringComparison.Ordinal))
            {
                int index = inNamespace.IndexOf("/", fullNS.Length);
                typeNamespace = inNamespace.Substring(fullNS.Length,index-fullNS.Length);
                assemblyName = UriDecode(inNamespace.Substring(index+1));
            }
            else
                return false;

            return true;
        }

        internal static void UriEncode(String value, StringBuilder sb)
        {
            if (value == null || value.Length == 0)
                return;

            for (int i=0; i<value.Length; i++)
            {
                if (value[i] == ' ')
                    sb.Append("%20");
                else if (value[i] == '=')
                    sb.Append("%3D");
                else if (value[i] == ',')
                    sb.Append("%2C");

                else
                    sb.Append(value[i]);
            }
        }

        internal static String UriDecode(String value)
        {
            if (value == null || value.Length == 0)
                return value;

            StringBuilder sb = new StringBuilder();

            for (int i=0; i<value.Length; i++)
            {
                if (value[i] == '%' && (value.Length-i >= 3))
                {
                    if (value[i+1] == '2' && value[i+2] == '0')
                    {
                        sb.Append(' ');
                        i += 2;
                    }
                    else if (value[i+1] == '3' && value[i+2] == 'D')
                    {
                        sb.Append('=');
                        i += 2;
                    }
                    else if (value[i+1] == '2' && value[i+2] == 'C')
                    {
                        sb.Append(',');
                        i += 2;
                    }

                    else
                        sb.Append(value[i]);
                }
                else
                    sb.Append(value[i]);
            }
            return sb.ToString();
        }



        private static bool IsNameNull(String name)
        {
            if (name == null || name.Length == 0)
                return true;
            else
                return false;
        }
        
    } // class SoapServices

    internal static class XmlNamespaceEncoder
    {   
        // <


        [System.Security.SecurityCritical]  // auto-generated
        internal static String GetXmlNamespaceForType(RuntimeType type, String dynamicUrl)
        {        
            String typeName = type.FullName;
            RuntimeAssembly assem = type.GetRuntimeAssembly();
            StringBuilder sb = new StringBuilder(256);
            Assembly systemAssembly = typeof(String).Module.Assembly;

            if(assem == systemAssembly)
            {
                sb.Append(SoapServices.namespaceNS);
                sb.Append(typeName);
            }
            else
            {
                sb.Append(SoapServices.fullNS);
                sb.Append(typeName);
                sb.Append('/');
                sb.Append(assem.GetSimpleName());
            }

            return sb.ToString();
        } // GetXmlNamespaceForType


        // encode a type namespace as an xml namespace (dynamic url is for types which have
        //   dynamically changing uri's, such as .SOAP files)
        [System.Security.SecurityCritical]  // auto-generated
        internal static String GetXmlNamespaceForTypeNamespace(RuntimeType type, String dynamicUrl)
        {        
            String typeNamespace = type.Namespace;
            RuntimeAssembly assem = type.GetRuntimeAssembly();
            StringBuilder sb = StringBuilderCache.Acquire(256);
            Assembly systemAssembly = typeof(String).Module.Assembly;

            if(assem == systemAssembly)
            {
                sb.Append(SoapServices.namespaceNS);
                sb.Append(typeNamespace);
            }
            else
            {
                sb.Append(SoapServices.fullNS);
                sb.Append(typeNamespace);
                sb.Append('/');
                sb.Append(assem.GetSimpleName());
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        } // GetXmlNamespaceForTypeNamespace

        // retrieve xml namespace that matches this type
        [System.Security.SecurityCritical]  // auto-generated
        internal static String GetTypeNameForSoapActionNamespace(String uri, out bool assemblyIncluded)
        {
            assemblyIncluded = false;
            String urtNSprefix = SoapServices.fullNS;
            String systemNSprefix = SoapServices.namespaceNS;
            
            if (uri.StartsWith(urtNSprefix, StringComparison.Ordinal))
            {
                uri = uri.Substring(urtNSprefix.Length); // now contains type/assembly
                char[] sep = new char[]{'/'};
                String[] parts = uri.Split(sep); 
                if (parts.Length != 2)
                    return null;
                else
                {
                    assemblyIncluded = true;
                    return parts[0] + ", " + parts[1];
                }
            }
            if (uri.StartsWith(systemNSprefix, StringComparison.Ordinal))
            {
                String assemName = ((RuntimeAssembly)typeof(String).Module.Assembly).GetSimpleName();
                assemblyIncluded = true;
                return uri.Substring(systemNSprefix.Length) + ", " + assemName; // now contains type
            }

            return null;
        } // GetTypeForXmlNamespace
    } // XmlNamespaceEncoder
    
} // namespace
