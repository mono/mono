/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    [StructLayout(LayoutKind.Sequential)]
    internal struct VariantArray1 {
        public Variant Element0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VariantArray2 {
        public Variant Element0, Element1;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VariantArray4 {
        public Variant Element0, Element1, Element2, Element3;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VariantArray8 {
        public Variant Element0, Element1, Element2, Element3, Element4, Element5, Element6, Element7;
    }

    //
    // Helper for getting the right VariantArray struct for a given number of
    // arguments. Will generate a struct if needed.
    //
    // We use this because we don't have stackalloc or pinning in Expression
    // Trees, so we can't create an array of Variants directly.
    //
    internal static class VariantArray {
        // Don't need a dictionary for this, it will have very few elements
        // (guarenteed less than 28, in practice 0-2)
        private static readonly List<Type> _generatedTypes = new List<Type>(0);

        internal static MemberExpression GetStructField(ParameterExpression variantArray, int field) {
            return Expression.Field(variantArray, "Element" + field);
        }

        internal static Type GetStructType(int args) {
            Debug.Assert(args >= 0);
            if (args <= 1) return typeof(VariantArray1);
            if (args <= 2) return typeof(VariantArray2);
            if (args <= 4) return typeof(VariantArray4);
            if (args <= 8) return typeof(VariantArray8);

            int size = 1;
            while (args > size) {
                size *= 2;
            }

            lock (_generatedTypes) {
                // See if we can find an existing type
                foreach (Type t in _generatedTypes) {
                    int arity = int.Parse(t.Name.Substring("VariantArray".Length), CultureInfo.InvariantCulture);
                    if (size == arity) {
                        return t;
                    }
                }

                // Else generate a new type
                Type type = CreateCustomType(size).MakeGenericType(new Type[]{typeof(Variant)});
                _generatedTypes.Add(type);
                return type;
            }
        }

        private static Type CreateCustomType(int size) {
            var attrs = TypeAttributes.NotPublic | TypeAttributes.SequentialLayout;
            TypeBuilder type = UnsafeMethods.DynamicModule.DefineType("VariantArray" + size, attrs, typeof(ValueType));
            var T = type.DefineGenericParameters(new string[] { "T" })[0];
            for (int i = 0; i < size; i++) {
                type.DefineField("Element" + i, T, FieldAttributes.Public);
            }
            return type.CreateType();
        }
    }
}

#endif
