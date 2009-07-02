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


using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif

#if CODEPLEX_40
namespace System.Linq.Expressions {
#else
namespace Microsoft.Linq.Expressions {
#endif
    internal enum AnalyzeTypeIsResult {
        KnownFalse,
        KnownTrue,
        KnownAssignable, // need null check only
        Unknown,         // need full runtime check
    }

    internal static class ConstantCheck {

        internal static bool IsNull(Expression e) {
            switch (e.NodeType) {
                case ExpressionType.Constant:
                    return ((ConstantExpression)e).Value == null;

                case ExpressionType.TypeAs:
                    var typeAs = (UnaryExpression)e;
                    // if the TypeAs check is guarenteed to fail, then its result will be null
                    return AnalyzeTypeIs(typeAs) == AnalyzeTypeIsResult.KnownFalse;
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
        /// If the result of a unary TypeAs expression is known statically, this
        /// returns the result, otherwise it returns null, meaning we'll need
        /// to perform the IsInst instruction at runtime.
        /// 
        /// The result of this function must be equivalent to IsInst, or
        /// null.
        /// </summary>
        internal static AnalyzeTypeIsResult AnalyzeTypeIs(UnaryExpression typeAs) {
            Debug.Assert(typeAs.NodeType == ExpressionType.TypeAs);
            return AnalyzeTypeIs(typeAs.Operand, typeAs.Type);
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

            //
            // If we couldn't statically assign and the type is sealed, no
            // value at runtime can make isinst succeed
            //
            if (nnOperandType.IsSealed) {
                return AnalyzeTypeIsResult.KnownFalse;
            }

            // Otherwise we need a full runtime check
            return AnalyzeTypeIsResult.Unknown;
        }
    }
}
