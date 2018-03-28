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

using System;
using System.Diagnostics;
using System.Dynamic.Utils;

#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif

    internal enum AnalyzeTypeIsResult {
        KnownFalse,
        KnownTrue,
        KnownAssignable, // need null check only
        Unknown,         // need full runtime check
    }

    internal static class ConstantCheck {

        internal static bool IsNull(Expression e) {
            if (e.NodeType == ExpressionType.Constant) {
                return ((ConstantExpression)e).Value == null;
            }
            return false;
        }


        /// <summary>
        /// If the result of a TypeBinaryExpression is known statically, this
        /// returns the result, otherwise it returns null, meaning we'll need
        /// to perform the IsInst instruction at runtime.
        /// 
        /// The result of this function must be equivalent to IsInst, or
        /// null.
        /// </summary>
        internal static AnalyzeTypeIsResult AnalyzeTypeIs(TypeBinaryExpression typeIs) {
            return AnalyzeTypeIs(typeIs.Expression, typeIs.TypeOperand);
        }

        /// <summary>
        /// If the result of an isinst opcode is known statically, this
        /// returns the result, otherwise it returns null, meaning we'll need
        /// to perform the IsInst instruction at runtime.
        /// 
        /// The result of this function must be equivalent to IsInst, or
        /// null.
        /// </summary>
        private static AnalyzeTypeIsResult AnalyzeTypeIs(Expression operand, Type testType) {
            Type operandType = operand.Type;

            // Oddly, we allow void operands
            // This is LinqV1 behavior of TypeIs
            if (operandType == typeof(void)) {
                return AnalyzeTypeIsResult.KnownFalse;
            }

            //
            // Type comparisons treat nullable types as if they were the
            // underlying type. The reason is when you box a nullable it
            // becomes a boxed value of the underlying type, or null.
            //
            Type nnOperandType = operandType.GetNonNullableType();
            Type nnTestType = testType.GetNonNullableType();

            //
            // See if we can determine the answer based on the static types
            //
            // Extensive testing showed that Type.IsAssignableFrom,
            // Type.IsInstanceOfType, and the isinst instruction were all
            // equivalent when used against a live object
            //
            if (nnTestType.IsAssignableFrom(nnOperandType)) {
                // If the operand is a value type (other than nullable), we
                // know the result is always true.
                if (operandType.IsValueType && !operandType.IsNullableType()) {
                    return AnalyzeTypeIsResult.KnownTrue;
                }

                // For reference/nullable types, we need to compare to null at runtime
                return AnalyzeTypeIsResult.KnownAssignable;
            }

            // We used to have an if IsSealed, return KnownFalse check here.
            // but that doesn't handle generic types & co/contravariance correctly.
            // So just use IsInst, which we know always gives us the right answer.

            // Otherwise we need a full runtime check
            return AnalyzeTypeIsResult.Unknown;
        }
    }
}
