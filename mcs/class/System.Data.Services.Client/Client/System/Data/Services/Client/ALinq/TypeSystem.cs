//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.

namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    internal static class TypeSystem
    {
        private static readonly Dictionary<MethodInfo, string> expressionMethodMap;

        private static readonly Dictionary<string, string> expressionVBMethodMap;

        private static readonly Dictionary<PropertyInfo, MethodInfo> propertiesAsMethodsMap;

#if !ASTORIA_LIGHT
        private const string VisualBasicAssemblyFullName = "Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=" + AssemblyRef.MicrosoftPublicKeyToken;
#else
        private const string VisualBasicAssemblyFullName = "Microsoft.VisualBasic, Version=2.0.5.0, Culture=neutral, PublicKeyToken=" + AssemblyRef.MicrosoftSilverlightPublicKeyToken;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Cleaner code")]
        static TypeSystem()
        {
#if !ASTORIA_LIGHT
            const int ExpectedCount = 24;
#else
            const int ExpectedCount = 22;
#endif
            expressionMethodMap = new Dictionary<MethodInfo, string>(ExpectedCount, EqualityComparer<MethodInfo>.Default);
            expressionMethodMap.Add(typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), @"substringof");
            expressionMethodMap.Add(typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), @"endswith");
            expressionMethodMap.Add(typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), @"startswith");
            expressionMethodMap.Add(typeof(string).GetMethod("IndexOf", new Type[] { typeof(string) }), @"indexof");
            expressionMethodMap.Add(typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) }), @"replace");
            expressionMethodMap.Add(typeof(string).GetMethod("Substring", new Type[] { typeof(int) }), @"substring");
            expressionMethodMap.Add(typeof(string).GetMethod("Substring", new Type[] { typeof(int), typeof(int) }), @"substring");
            expressionMethodMap.Add(typeof(string).GetMethod("ToLower", Type.EmptyTypes), @"tolower");
            expressionMethodMap.Add(typeof(string).GetMethod("ToUpper", Type.EmptyTypes), @"toupper");
            expressionMethodMap.Add(typeof(string).GetMethod("Trim", Type.EmptyTypes), @"trim");
            expressionMethodMap.Add(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }, null), @"concat");   
            expressionMethodMap.Add(typeof(string).GetProperty("Length", typeof(int)).GetGetMethod(), @"length");

            expressionMethodMap.Add(typeof(DateTime).GetProperty("Day", typeof(int)).GetGetMethod(), @"day");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Hour", typeof(int)).GetGetMethod(), @"hour");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Month", typeof(int)).GetGetMethod(), @"month");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Minute", typeof(int)).GetGetMethod(), @"minute");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Second", typeof(int)).GetGetMethod(), @"second");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Year", typeof(int)).GetGetMethod(), @"year");

            expressionMethodMap.Add(typeof(Math).GetMethod("Round", new Type[] { typeof(double) }), @"round");
            expressionMethodMap.Add(typeof(Math).GetMethod("Round", new Type[] { typeof(decimal) }), @"round");
            expressionMethodMap.Add(typeof(Math).GetMethod("Floor", new Type[] { typeof(double) }), @"floor");
#if !ASTORIA_LIGHT            
            expressionMethodMap.Add(typeof(Math).GetMethod("Floor", new Type[] { typeof(decimal) }), @"floor");
#endif
            expressionMethodMap.Add(typeof(Math).GetMethod("Ceiling", new Type[] { typeof(double) }), @"ceiling");
#if !ASTORIA_LIGHT            
            expressionMethodMap.Add(typeof(Math).GetMethod("Ceiling", new Type[] { typeof(decimal) }), @"ceiling");
#endif

            Debug.Assert(expressionMethodMap.Count == ExpectedCount, "expressionMethodMap.Count == ExpectedCount");

            expressionVBMethodMap = new Dictionary<string, string>(EqualityComparer<string>.Default);

            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.Trim", @"trim");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.Len", @"length");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.Mid", @"substring");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.UCase", @"toupper");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.LCase", @"tolower");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Year", @"year");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Month", @"month");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Day", @"day");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Hour", @"hour");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Minute", @"minute");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Second", @"second");

            Debug.Assert(expressionVBMethodMap.Count == 11, "expressionVBMethodMap.Count == 11");

            propertiesAsMethodsMap = new Dictionary<PropertyInfo, MethodInfo>(EqualityComparer<PropertyInfo>.Default);
            propertiesAsMethodsMap.Add(
                typeof(string).GetProperty("Length", typeof(int)), 
                typeof(string).GetProperty("Length", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(
                typeof(DateTime).GetProperty("Day", typeof(int)), 
                typeof(DateTime).GetProperty("Day", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(
                typeof(DateTime).GetProperty("Hour", typeof(int)), 
                typeof(DateTime).GetProperty("Hour", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(
                typeof(DateTime).GetProperty("Minute", typeof(int)), 
                typeof(DateTime).GetProperty("Minute", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(
                typeof(DateTime).GetProperty("Second", typeof(int)), 
                typeof(DateTime).GetProperty("Second", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(
                typeof(DateTime).GetProperty("Month", typeof(int)),
                typeof(DateTime).GetProperty("Month", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(
                typeof(DateTime).GetProperty("Year", typeof(int)), 
                typeof(DateTime).GetProperty("Year", typeof(int)).GetGetMethod());

            Debug.Assert(propertiesAsMethodsMap.Count == 7, "propertiesAsMethodsMap.Count == 7");
        }

        internal static bool TryGetQueryOptionMethod(MethodInfo mi, out string methodName)
        {
            return (expressionMethodMap.TryGetValue(mi, out methodName) ||
                (mi.DeclaringType.Assembly.FullName == VisualBasicAssemblyFullName &&
                 expressionVBMethodMap.TryGetValue(mi.DeclaringType.FullName + "." + mi.Name, out methodName)));
        }

        internal static bool TryGetPropertyAsMethod(PropertyInfo pi, out MethodInfo mi)
        {
            return propertiesAsMethodsMap.TryGetValue(pi, out mi);
        }

        internal static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) 
            {
                return seqType;
            }

            return ienum.GetGenericArguments()[0];
        }

        internal static bool IsPrivate(PropertyInfo pi)
        {
            MethodInfo mi = pi.GetGetMethod() ?? pi.GetSetMethod();
            if (mi != null)
            {
                return mi.IsPrivate;
            }

            return true;
        }

        internal static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }
    }
}
