// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xaml.MS.Impl;

namespace System.Xaml.Schema
{
    internal static class CollectionReflector
    {
        private static Type[] s_typeOfObjectArray;
        private static Type[] s_typeOfTwoObjectArray;
        private static MethodInfo s_getEnumeratorMethod;
        private static MethodInfo s_listAddMethod;
        private static MethodInfo s_dictionaryAddMethod;

        // Collection Lookup algorithm:
        // If the type is an array, it's an array. :)
        // Else if it implements (or is) IDictionary or IDictionary<K,V>, it is a dictionary.
        // Else if it implements (or is) IList or ICollection<T>, it is a collection.
        // Else if it implements IEnumerable, or has a method 'IEnumerator GetEnumerable()':
        //   If it has a method Add(x,y) it is a dictionary
        //   Else if has a method Add(x) it is a collection
        // Else it is none.
        // Note that the options are mutually exclusive: arrays and dictionaries are not collections.
        internal static XamlCollectionKind LookupCollectionKind(Type type, out MethodInfo addMethod)
        {
            addMethod = null;

            // Check for array first; it's fast and easy
            if (type.IsArray)
            {
                return XamlCollectionKind.Array;
            }

            // Dictionaries and Collections must implement IEnumerable or have method 
            // GetEnumerator() where return type is assignable to IEnumerator
            bool isIEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
            if (!isIEnumerable && LookupEnumeratorMethod(type) == null)
            {
                return XamlCollectionKind.None;
            }

            // Many dictionaries are also collections, so check for dictionary first, then collection
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return XamlCollectionKind.Dictionary;
            }
            if (TryGetIDictionaryAdder(type, out addMethod))
            {
                return XamlCollectionKind.Dictionary;
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                return XamlCollectionKind.Collection;
            }
            if (TryGetICollectionAdder(type, out addMethod))
            {
                return XamlCollectionKind.Collection;
            }

            // If the type doesn't match any of the interfaces, check for Add methods
            if (TryGetDictionaryAdder(type, false /*mayBeIDictionary*/, out addMethod))
            {
                return XamlCollectionKind.Dictionary;
            }
            if (TryGetCollectionAdder(type, false /*mayBeICollection*/, out addMethod))
            {
                return XamlCollectionKind.Collection;
            }

            return XamlCollectionKind.None;
        }

        internal static MethodInfo LookupAddMethod(Type type, XamlCollectionKind collectionKind)
        {
            MethodInfo result = null;
            switch (collectionKind)
            {
                case XamlCollectionKind.Collection:
                    bool isCollection = TryGetCollectionAdder(type, true /*mayBeICollection*/, out result);
                    if (isCollection && result == null)
                    {
                        throw new XamlSchemaException(SR.Get(SRID.AmbiguousCollectionItemType, type));
                    }
                    break;
                case XamlCollectionKind.Dictionary:
                    bool isDictionary = TryGetDictionaryAdder(type, true /*mayBeIDictionary*/, out result);
                    if (isDictionary && result == null)
                    {
                        throw new XamlSchemaException(SR.Get(SRID.AmbiguousDictionaryItemType, type));
                    }
                    break;
            }
            return result;
        }

        // Returns true if the type is an ICollection<T>. Additionally, if only one <T> is
        // implemented, returns the Add method for that type.
        private static bool TryGetICollectionAdder(Type type, out MethodInfo addMethod)
        {
            bool hasMoreThanOneICollection = false;
            Type genericICollection = GetGenericInterface(type, typeof(ICollection<>), out hasMoreThanOneICollection);
            if (genericICollection != null)
            {
                addMethod = genericICollection.GetMethod(KnownStrings.Add);
                return true;
            }
            else
            {
                addMethod = null;
                return hasMoreThanOneICollection;
            }
        }

        // Returns true if the type is a collection. Additionally, if the item type could be
        // determined unambiguously, returns the Add method for that type.
        private static bool TryGetCollectionAdder(Type type, bool mayBeICollection, out MethodInfo addMethod)
        {
            bool hasMoreThanOneICollection = false;
            if (mayBeICollection)
            {
                // Look for ICollection<T> implementation
                if (TryGetICollectionAdder(type, out addMethod))
                {
                    if (addMethod != null)
                    {
                        return true;
                    }
                    else
                    {
                        hasMoreThanOneICollection = true;
                    }
                }
            }

            // If type has one and only one Add() taking one parameter, that parameter is the item type
            // Else if it implements IList, the item type is Object
            bool hasMoreThanOneAddMethod = false;
            addMethod = GetAddMethod(type, 1, out hasMoreThanOneAddMethod);
            if (addMethod == null && typeof(IList).IsAssignableFrom(type))
            {
                addMethod = IListAddMethod;
            }
            if (addMethod != null)
            {
                return true;
            }

            // If type has more than one Add() taking one parameter, or more than one ICollection<T>,
            // and no non-generic IList, we require Add(object), or else we return null.
            if (hasMoreThanOneAddMethod || hasMoreThanOneICollection)
            {
                addMethod = GetMethod(type, KnownStrings.Add, TypeOfObjectArray);
                return true;
            }

            // No Add methods, no ICollection... not a collection
            return false;
        }

        // Returns true if the type is an IDictionary<K,V>. Additionally, if only one <K,V> is
        // implemented, returns the Add method for those types.
        private static bool TryGetIDictionaryAdder(Type type, out MethodInfo addMethod)
        {
            bool hasMoreThanOneIDictionary = false;
            Type genericIDictionary = GetGenericInterface(type, typeof(IDictionary<,>), out hasMoreThanOneIDictionary);
            if (genericIDictionary != null)
            {
                addMethod = GetPublicMethod(genericIDictionary, KnownStrings.Add, 2);
                return true;
            }
            else
            {
                addMethod = null;
                return hasMoreThanOneIDictionary;
            }
        }

        // Returns true if the type is a dictionary. Additionally, if the key and value could be
        // determined unambiguously, returns the Add method for those types.
        private static bool TryGetDictionaryAdder(Type type, bool mayBeIDictionary, out MethodInfo addMethod)
        {
            bool hasMoreThanOneIDictionary = false;
            if (mayBeIDictionary)
            {
                // Look for IDictionary<K,V> implementation
                if (TryGetIDictionaryAdder(type, out addMethod))
                {
                    if (addMethod != null)
                    {
                        return true;
                    }
                    else
                    {
                        hasMoreThanOneIDictionary = true;
                    }
                }
            }

            // If type has one and only one Add() taking two parameters, they are key and value
            // Else if it implements non-generic IDictionary, key and item types are Object
            bool hasMoreThanOneAddMethod = false;
            addMethod = GetAddMethod(type, 2, out hasMoreThanOneAddMethod);
            if (addMethod == null && typeof(IDictionary).IsAssignableFrom(type))
            {
                addMethod = IDictionaryAddMethod;
            }
            if (addMethod != null)
            {
                return true;
            }

            // If type has more than one Add() taking two parameters, or more than one IDictionary<K,V>,
            // we require an Add(object, object), or else we return null.
            if (hasMoreThanOneAddMethod || hasMoreThanOneIDictionary)
            {
                addMethod = GetMethod(type, KnownStrings.Add, TypeOfTwoObjectArray);
                return true;
            }

            // No Add methods, no IDictionary... not a dictionary
            return false;
        }

        internal static MethodInfo GetAddMethod(Type type, Type contentType)
        {
            return GetMethod(type, KnownStrings.Add, new Type[] { contentType });
        }

        internal static MethodInfo GetEnumeratorMethod(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return IEnumerableGetEnumeratorMethod;
            }
            else
            {
                return LookupEnumeratorMethod(type);
            }
        }

        internal static MethodInfo GetIsReadOnlyMethod(Type collectionType, Type itemType)
        {
            Type genericICollection = typeof(ICollection<>).MakeGenericType(itemType);
            if (genericICollection.IsAssignableFrom(collectionType))
            {
                MethodInfo isReadOnlyMethod = genericICollection.GetProperty(KnownStrings.IsReadOnly).GetGetMethod();
                return isReadOnlyMethod;
            }
            return null;
        }

        private static MethodInfo LookupEnumeratorMethod(Type type)
        {
            MethodInfo result = GetMethod(type, KnownStrings.GetEnumerator, Type.EmptyTypes);
            if ((result != null) && !typeof(IEnumerator).IsAssignableFrom(result.ReturnType))
            {
                result = null;
            }
            return result;
        }

        private static Type GetGenericInterface(Type type, Type interfaceType, out bool hasMultiple)
        {
            Type result = null;
            hasMultiple = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
            {
                return type;
            }
            foreach (Type currentInterface in type.GetInterfaces())
            {
                if (currentInterface.IsGenericType && currentInterface.GetGenericTypeDefinition() == interfaceType)
                {
                    if (result != null)
                    {
                        // More than one genericType<T> implemented
                        hasMultiple = true;
                        return null;
                    }
                    result = currentInterface;
                }
            }
            return result;
        }

        private static MethodInfo GetAddMethod(Type type, int paramCount, out bool hasMoreThanOne)
        {
            MethodInfo result = null;
            MemberInfo[] addMembers = type.GetMember(KnownStrings.Add, MemberTypes.Method, GetBindingFlags(type));
            if (addMembers != null)
            {
                foreach (MemberInfo mi in addMembers)
                {
                    MethodInfo method = (MethodInfo)mi;
                    if (!TypeReflector.IsPublicOrInternal(method))
                    {
                        continue;
                    }
                    ParameterInfo[] paramInfos = method.GetParameters();
                    if (paramInfos == null || paramInfos.Length != paramCount)
                    {
                        continue;
                    }
                    if (result != null)
                    {
                        // More than one Add method
                        hasMoreThanOne = true;
                        return null;
                    }
                    result = method;
                }
            }
            hasMoreThanOne = false;
            return result;
        }

        private static BindingFlags GetBindingFlags(Type type)
        {
            // We don't support internal collection impl on public type, because then the fundamental
            // characteristics of the type (collection vs not) would change based on visibility
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            if (!type.IsVisible)
            {
                flags |= BindingFlags.NonPublic;
            }
            return flags;
        }

        private static MethodInfo GetMethod(Type type, string name, Type[] argTypes)
        {
            MethodInfo result = type.GetMethod(name, GetBindingFlags(type), null, argTypes, null);
            if (result != null && !TypeReflector.IsPublicOrInternal(result))
            {
                result = null;
            }
            return result;
        }

        private static MethodInfo GetPublicMethod(Type type, string name, int argCount)
        {
            foreach (MemberInfo mi in type.GetMember(name, MemberTypes.Method, 
                BindingFlags.Instance | BindingFlags.Public))
            {
                MethodInfo method = (MethodInfo)mi;
                if (method.GetParameters().Length == argCount)
                {
                    return method;
                }
            }
            return null;
        }

        private static Type[] TypeOfObjectArray
        {
            get
            {
                if (s_typeOfObjectArray == null)
                {
                    s_typeOfObjectArray = new Type[] { typeof(object) };
                }
                return s_typeOfObjectArray;
            }
        }

        private static Type[] TypeOfTwoObjectArray
        {
            get
            {
                if (s_typeOfTwoObjectArray == null)
                {
                    s_typeOfTwoObjectArray = new Type[] { typeof(object), typeof(object) };
                }
                return s_typeOfTwoObjectArray;
            }
        }

        private static MethodInfo IEnumerableGetEnumeratorMethod
        {
            get
            {
                if (s_getEnumeratorMethod == null)
                {
                    s_getEnumeratorMethod = typeof(IEnumerable).GetMethod(KnownStrings.GetEnumerator);
                }
                return s_getEnumeratorMethod;
            }
        }

        private static MethodInfo IListAddMethod
        {
            get
            {
                if (s_listAddMethod == null)
                {
                    s_listAddMethod = typeof(IList).GetMethod(KnownStrings.Add);
                }
                return s_listAddMethod;
            }
        }

        private static MethodInfo IDictionaryAddMethod
        {
            get
            {
                if (s_dictionaryAddMethod == null)
                {
                    s_dictionaryAddMethod = typeof(IDictionary).GetMethod(KnownStrings.Add);
                }
                return s_dictionaryAddMethod;
            }
        }
    }
}
