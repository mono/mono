//------------------------------------------------------------------------------
// <copyright file="ServicesUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Script.Services {
    internal static class ServicesUtilities {
        
        internal static string GetClientTypeName(string name) {
            // e.g. MyNS.MySubNS.MyWebService OR var MyWebService
            return name.Replace('+', '_');
        }

        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "System.Collections.Generic.Dictionary`2<System.Type,System.String>", Justification = "This is used by ASP.Net web services which is a legacy technology.")]
        internal static string GetClientTypeFromServerType(WebServiceData webServiceData, Type type)
        {
            // For intellisense purposes, returns a best estimate of what the appropriate client-side type is for a given server type.
            // Takes generated client proxies and enum proxies into consideration.
            // The rest is a best guess.
            // If all else fails we use "", to indicate "any" client side type. "Object" is not the same as any type on the client since
            // string, for example, is not considered an object. "Object" is equiv to a .net dictionary.



            if (webServiceData.ClientTypeNameDictionary.ContainsKey(type)) {
                // if it exists in the client type dictionary, it will have a proxy generated for it
                //get the client based on type.FullName for ASMX, and schema qualified name and namespace for WCF
                return webServiceData.ClientTypeNameDictionary[type];
            }

            if (type.IsEnum) {
                // there will be a proxy for this enum
                return GetClientTypeName(type.FullName);
            }

            // there is no client proxy for it, so it either maps to a built-in js type or it could be "anything"

            // take care of the most common types
            if (type == typeof(string) || type == typeof(char)) {
                return "String";
            }
            else if (type.IsPrimitive) {
                // The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64 (long), UInt64, IntPtr, Char, Double, and Single (float).
                if (type == typeof(bool)) {
                    // bool is the only primitive we shouldnt treat as a number
                    return "Boolean";
                }
                else {
                    // takes care of all ints, float, double, but not decimal since it isnt a primitive
                    // we also consider byte, sbyte, and intptr to be numbers
                    return "Number";
                }
            }

            if (type.IsValueType) {
                if (type == typeof(DateTime)) {
                    return "Date";
                }
                else if (type == typeof(Guid)) {
                    return "String";
                }
                else if (type == typeof(Decimal)) {
                    return "Number";
                }
            }

            if (typeof(IDictionary).IsAssignableFrom(type)) {
                return "Object";
            }
            // might still be IDictionary<K,T>
            if (type.IsGenericType) {
                Type gtd = type;
                if (!type.IsGenericTypeDefinition) {
                    gtd = type.GetGenericTypeDefinition();
                }
                if (gtd == typeof(IDictionary<,>)) {
                    return "Object";
                }
            }

            if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type)) {
                return "Array";
            }

            // dont know what it is (e.g., TimeSpan), or it is type Object, so allow any client type.
            return "";
        }

        internal static Type UnwrapNullableType(Type type) {
            // check for nullable<t> and pull out <t>
            if (type.IsGenericType && !type.IsGenericTypeDefinition) {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>)) {
                    return type.GetGenericArguments()[0];
                }
            }

            return type;
        }

        
        // Serialize an object to an XML string
        internal static string XmlSerializeObjectToString(object obj) {
            // 
            XmlSerializer xs = new XmlSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8)) {
                xs.Serialize(writer, obj);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
