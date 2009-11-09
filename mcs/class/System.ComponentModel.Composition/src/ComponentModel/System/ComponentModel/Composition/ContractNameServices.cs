// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Internal;

namespace System.ComponentModel.Composition
{
    internal static class ContractNameServices
    {
        const char NamespaceSeparator = '.';
        const char ArrayOpeningBracket = '[';
        const char ArrayClosingBracket = ']';
        const char ArraySeparator = ',';
        const char PointerSymbol = '*';
        const char ReferenceSymbol = '&';
        const char GenericArityBackQuote = '`';
        const char NestedClassSeparator = '+';
        const char ContractNameGenericOpeningBracket = '(';
        const char ContractNameGenericClosingBracket = ')';
        const char ContractNameGenericArgumentSeparator = ',';
        const char CustomModifiersSeparator = ' ';

        [ThreadStatic]
        private static Dictionary<Type, string> typeIdentityCache;

        private static Dictionary<Type, string> TypeIdentityCache
        {
            get
            {
                return typeIdentityCache = typeIdentityCache ?? new Dictionary<Type, string>();
            }
        }

        internal static string GetTypeIdentity(Type type)
        {
            Assumes.NotNull(type);
            string typeIdentity = null;

            if (!TypeIdentityCache.TryGetValue(type, out typeIdentity))
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(Delegate)))
                {
                    MethodInfo method = type.GetMethod("Invoke");
                    typeIdentity = ContractNameServices.GetTypeIdentityFromMethod(method);
                }
                else
                {
                    StringBuilder typeIdentityStringBuilder = new StringBuilder();
                    WriteTypeWithNamespace(typeIdentityStringBuilder, type);
                    typeIdentity = typeIdentityStringBuilder.ToString();
                }

                TypeIdentityCache.Add(type, typeIdentity);
            }

            return typeIdentity;
        }

        internal static string GetTypeIdentityFromMethod(MethodInfo method)
        {
            StringBuilder methodNameStringBuilder = new StringBuilder();

            WriteTypeWithNamespace(methodNameStringBuilder, method.ReturnType);

            methodNameStringBuilder.Append("(");

            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i != 0)
                {
                    methodNameStringBuilder.Append(",");
                }

                WriteTypeWithNamespace(methodNameStringBuilder, parameters[i].ParameterType);
            }
            methodNameStringBuilder.Append(")");

            return methodNameStringBuilder.ToString();
        }
       
        private static void WriteTypeWithNamespace(StringBuilder typeName, Type type)
        {
            // Writes type with namesapce
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                typeName.Append(type.Namespace);
                typeName.Append(NamespaceSeparator);
            }
            WriteType(typeName, type);
        }

        private static void WriteType(StringBuilder typeName, Type type)
        {
            // Writes type name
            if (type.IsGenericType)
            {
                //
                // Reflection format stores all the generic arguments (including the ones for parent types) on the leaf type.
                // These arguments are placed in a queue and are written out based on generic arity (`X) of each type
                //
                Queue<Type> genericTypeArguments = new Queue<Type>(type.GetGenericArguments());
                WriteGenericType(typeName, type, type.IsGenericTypeDefinition, genericTypeArguments);
                Assumes.IsTrue(genericTypeArguments.Count == 0, "Expecting genericTypeArguments queue to be empty.");
            }
            else
            {
                WriteNonGenericType(typeName, type);
            }
        }

        private static void WriteNonGenericType(StringBuilder typeName, Type type)
        {
            //
            // Writes non-generic type
            //
            if (type.DeclaringType != null)
            {
                WriteType(typeName, type.DeclaringType);
                typeName.Append(NestedClassSeparator);
            }
            if (type.IsArray)
            {
                WriteArrayType(typeName, type);
            }
            else if (type.IsPointer)
            {
                WritePointerType(typeName, type);
            }
            else if (type.IsByRef)
            {
                WriteByRefType(typeName, type);
            }
            else
            {
                typeName.Append(type.Name);
            }
        }

        private static void WriteArrayType(StringBuilder typeName, Type type)
        {
            //
            // Writes array type  e.g <TypeName>[]
            // Note that jagged arrays are stored in reverse order
            // e.g. C#: Int32[][,]  Reflection: Int32[,][]
            // we are following C# order for arrays
            //
            Type rootElementType = FindArrayElementType(type);
            WriteType(typeName, rootElementType);
            Type elementType = type;
            do
            {
                WriteArrayTypeDimensions(typeName, elementType);
            }
            while ((elementType = elementType.GetElementType()) != null && elementType.IsArray);
        }

        private static void WritePointerType(StringBuilder typeName, Type type)
        {
            //
            // Writes pointer type  e.g <TypeName>*
            //
            WriteType(typeName, type.GetElementType());
            typeName.Append(PointerSymbol);
        }

        private static void WriteByRefType(StringBuilder typeName, Type type)
        {
            //
            // Writes by ref type e.g <TypeName>&
            //
            WriteType(typeName, type.GetElementType());
            typeName.Append(ReferenceSymbol);
        }

        private static void WriteArrayTypeDimensions(StringBuilder typeName, Type type)
        {
            //
            // Writes array type dimensions e.g. [,,]
            //
            typeName.Append(ArrayOpeningBracket);
            int rank = type.GetArrayRank();
            for (int i = 1; i < rank; i++)
            {
                typeName.Append(ArraySeparator);
            }
            typeName.Append(ArrayClosingBracket);
        }

        private static void WriteGenericType(StringBuilder typeName, Type type, bool isDefinition, Queue<Type> genericTypeArguments)
        {
            //
            // Writes generic type including parent generic types
            // genericTypeArguments contains type arguments obtained from the most nested type
            // isDefinition parameter indicates if we are dealing with generic type definition
            //
            if (type.DeclaringType != null)
            {
                if (type.DeclaringType.IsGenericType)
                {
                    WriteGenericType(typeName, type.DeclaringType, isDefinition, genericTypeArguments);
                }
                else
                {
                    WriteNonGenericType(typeName, type.DeclaringType);
                }
                typeName.Append(NestedClassSeparator);
            }
            WriteGenericTypeName(typeName, type, isDefinition, genericTypeArguments);
        }

        private static void WriteGenericTypeName(StringBuilder typeName, Type type, bool isDefinition, Queue<Type> genericTypeArguments)
        {
            //
            // Writes generic type name, e.g. generic name and generic arguments
            //
            Assumes.IsTrue(type.IsGenericType, "Expecting type to be a generic type");
            int genericArity = GetGenericArity(type);
            string genericTypeName = FindGenericTypeName(type.GetGenericTypeDefinition().Name);
            typeName.Append(genericTypeName);
            WriteTypeArgumentsString(typeName, genericArity, isDefinition, genericTypeArguments);
        }

        private static void WriteTypeArgumentsString(StringBuilder typeName, int argumentsCount, bool isDefinition, Queue<Type> genericTypeArguments)
        {
            //
            // Writes type arguments in brackets, e.g. (<contract_name1>, <contract_name2>, ...)
            //
            if (argumentsCount == 0)
            {
                return;
            }
            typeName.Append(ContractNameGenericOpeningBracket);
            for (int i = 0; i < argumentsCount; i++)
            {
                Assumes.IsTrue(genericTypeArguments.Count > 0, "Expecting genericTypeArguments to contain at least one Type");
                Type genericTypeArgument = genericTypeArguments.Dequeue();
                if (!isDefinition)
                {
                    WriteTypeWithNamespace(typeName, genericTypeArgument);
                }
                typeName.Append(ContractNameGenericArgumentSeparator);
            }
            typeName.Remove(typeName.Length - 1, 1);
            typeName.Append(ContractNameGenericClosingBracket);
        }

        //internal for testability
        internal static void WriteCustomModifiers(StringBuilder typeName, string customKeyword, Type[] types)
        {
            //
            // Writes custom modifiers in the format: customKeyword(<contract_name>,<contract_name>,...)
            //
            typeName.Append(CustomModifiersSeparator);
            typeName.Append(customKeyword);
            Queue<Type> typeArguments = new Queue<Type>(types);
            WriteTypeArgumentsString(typeName, types.Length, false, typeArguments);
            Assumes.IsTrue(typeArguments.Count == 0, "Expecting genericTypeArguments queue to be empty.");
        }

        private static Type FindArrayElementType(Type type)
        {
            //
            // Gets array element type by calling GetElementType() until the element is not an array
            //
            Type elementType = type;
            while ((elementType = elementType.GetElementType()) != null && elementType.IsArray) { }
            return elementType;
        }

        private static string FindGenericTypeName(string genericName)
        {
            //
            // Gets generic type name omitting the backquote and arity indicator
            // List`1 -> List
            // Arity indicator is returned as output parameter
            //
            int indexOfBackQuote = genericName.IndexOf(GenericArityBackQuote);
            if (indexOfBackQuote > -1)
            {
                genericName = genericName.Substring(0, indexOfBackQuote);
            }
            return genericName;
        }

        private static int GetGenericArity(Type type)
        {
            if (type.DeclaringType == null)
            {
                return type.GetGenericArguments().Length;
            }

            // The generic arity is equal to the difference in the number of generic arguments
            // from the type and the declaring type.

            int delclaringTypeGenericArguments = type.DeclaringType.GetGenericArguments().Length;
            int typeGenericArguments = type.GetGenericArguments().Length;

            Assumes.IsTrue(typeGenericArguments >= delclaringTypeGenericArguments);

            return typeGenericArguments - delclaringTypeGenericArguments;
        }
    }
}
