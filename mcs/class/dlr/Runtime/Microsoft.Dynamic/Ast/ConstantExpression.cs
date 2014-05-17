/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

#if FEATURE_NUMERICS
using BigInt = System.Numerics.BigInteger;
using Complex = System.Numerics.Complex;
#endif

using System;
using System.Reflection;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public static partial class Utils {
        private static readonly ConstantExpression TrueLiteral = Expression.Constant(true, typeof(bool));
        private static readonly ConstantExpression FalseLiteral = Expression.Constant(false, typeof(bool));
        private static readonly ConstantExpression NullLiteral = Expression.Constant(null, typeof(object));
        private static readonly ConstantExpression EmptyStringLiteral = Expression.Constant(String.Empty, typeof(string));
        private static readonly ConstantExpression[] IntCache = new ConstantExpression[100];

        /// <summary>
        /// Wraps the given value in a WeakReference and returns a tree that will retrieve
        /// the value from the WeakReference.
        /// </summary>
        public static MemberExpression WeakConstant(object value) {
            System.Diagnostics.Debug.Assert(!(value is Expression));
            return Expression.Property(
                Constant(new WeakReference(value)),
                typeof(WeakReference).GetDeclaredProperty("Target")
            );
        }

        public static ConstantExpression Constant(object value, Type type) {
            return Expression.Constant(value, type);
        }

        // The helper API should return ConstantExpression after SymbolConstantExpression goes away
        public static Expression Constant(object value) {
            if (value == null) {
                return NullLiteral;
            }

#if !MONO_INTERPRETER
            BigInteger bi = value as BigInteger;
            if ((object)bi != null) {
                return BigIntegerConstant(bi);
#endif

#if FEATURE_NUMERICS
            if (value is BigInt)
                return BigIntConstant((BigInt)value);
            if (value is Complex)
                return ComplexConstant((Complex)value);
#endif

#if !MONO_INTERPRETER
            if (value is Complex64)
                return Complex64Constant((Complex64)value);
#endif
            if (value is Type) {
                return Expression.Constant(value, typeof(Type));
            } else if (value is ConstructorInfo) {
                return Expression.Constant(value, typeof(ConstructorInfo));
            } else if (value is EventInfo) {
                return Expression.Constant(value, typeof(EventInfo));
            } else if (value is FieldInfo) {
                return Expression.Constant(value, typeof(FieldInfo));
            } else if (value is MethodInfo) {
                return Expression.Constant(value, typeof(MethodInfo));
            } else if (value is PropertyInfo) {
                return Expression.Constant(value, typeof(PropertyInfo));
            } else {
                Type t = value.GetType();
                if (!t.GetTypeInfo().IsEnum) {
                    switch (t.GetTypeCode()) {
                        case TypeCode.Boolean:
                            return (bool)value ? TrueLiteral : FalseLiteral;
                        case TypeCode.Int32:
                            int x = (int)value;
                            int cacheIndex = x + 2;
                            if (cacheIndex >= 0 && cacheIndex < IntCache.Length) {
                                ConstantExpression res;
                                if ((res = IntCache[cacheIndex]) == null) {
                                    IntCache[cacheIndex] = res = Constant(x, typeof(int));
                                }
                                return res;
                            }
                            break;
                        case TypeCode.String:
                            if (String.IsNullOrEmpty((string)value)) {
                                return EmptyStringLiteral;
                            }
                            break;
                    }
                }
                return Expression.Constant(value);
            }
        }

#if !MONO_INTERPRETER
        private static Expression BigIntegerConstant(BigInteger value) {
            int ival;
            if (value.AsInt32(out ival)) {
                return Expression.Call(
                    new Func<int, BigInteger>(BigInteger.Create).GetMethodInfo(),
                    Constant(ival)
                );
            }

            long lval;
            if (value.AsInt64(out lval)) {
                return Expression.Call(
                    new Func<long, BigInteger>(BigInteger.Create).GetMethodInfo(),
                    Constant(lval)
                );
            }

#if !FEATURE_NUMERICS
            return Expression.Call(
                new Func<int, uint[], BigInteger>(CompilerHelpers.CreateBigInteger).Method,
                Constant((int)value.Sign),
                CreateArray<uint>(value.GetWords())
            );
#else
            return Expression.Call(
                new Func<bool, byte[], BigInteger>(CompilerHelpers.CreateBigInteger).GetMethodInfo(),
                Constant(value.Sign < 0),
                CreateArray<byte>(value.Abs().ToByteArray())
            );
        }

        private static Expression BigIntConstant(BigInt value) {
            int ival;
            if (value.AsInt32(out ival)) {
                return Expression.Call(
                    new Func<int, BigInt>(CompilerHelpers.CreateBigInt).GetMethodInfo(),
                    Constant(ival)
                );
            }

            long lval;
            if (value.AsInt64(out lval)) {
                return Expression.Call(
                    new Func<long, BigInt>(CompilerHelpers.CreateBigInt).GetMethodInfo(),
                    Constant(lval)
                );
            }

            return Expression.Call(
                new Func<bool, byte[], BigInt>(CompilerHelpers.CreateBigInt).GetMethodInfo(),
                Constant(value.Sign < 0),
                CreateArray<byte>(value.Abs().ToByteArray())
            );
#endif
        }
#endif
        private static Expression CreateArray<T>(T[] array) {
            // TODO: could we use blobs?
            Expression[] init = new Expression[array.Length];
            for (int i = 0; i < init.Length; i++) {
                init[i] = Constant(array[i]);
            }
            return Expression.NewArrayInit(typeof(T), init);
        }

#if FEATURE_NUMERICS
        private static Expression ComplexConstant(Complex value) {
            if (value.Real != 0.0) {
                if (value.Imaginary() != 0.0) {
                    return Expression.Call(
                        new Func<double, double, Complex>(MathUtils.MakeComplex).GetMethodInfo(),
                        Constant(value.Real),
                        Constant(value.Imaginary())
                    );
                } else {
                    return Expression.Call(
                        new Func<double, Complex>(MathUtils.MakeReal).GetMethodInfo(),
                        Constant(value.Real)
                    );
                }
            } else {
                return Expression.Call(
                    new Func<double, Complex>(MathUtils.MakeImaginary).GetMethodInfo(),
                    Constant(value.Imaginary())
                );
            }
        }
#endif

#if !MONO_INTERPRETER
        private static Expression Complex64Constant(Complex64 value) {
            if (value.Real != 0.0) {
                if (value.Imag != 0.0) {
                    return Expression.Call(
                        new Func<double, double, Complex64>(Complex64.Make).GetMethodInfo(),
                        Constant(value.Real),
                        Constant(value.Imag)
                    );
                } else {
                    return Expression.Call(
                        new Func<double, Complex64>(Complex64.MakeReal).GetMethodInfo(),
                        Constant(value.Real)
                    );
                }
            } else {
                return Expression.Call(
                    new Func<double, Complex64>(Complex64.MakeImaginary).GetMethodInfo(),
                    Constant(value.Imag)
                );
            }
        }
#endif
    }
}
