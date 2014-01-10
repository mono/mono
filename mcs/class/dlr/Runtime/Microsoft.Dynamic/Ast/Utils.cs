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

using System;
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    [Flags]
    public enum ExpressionAccess {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
    }

    public static partial class Utils {
        /// <summary>
        /// Determines whether specified expression type represents an assignment.
        /// </summary>
        /// <returns>
        /// True if the expression type represents an assignment.
        /// </returns>
        /// <remarks>
        /// Note that some other nodes can also assign to variables, members or array items:
        /// MemberInit, NewArrayInit, Call with ref params, New with ref params, Dynamic with ref params.
        /// </remarks>
        public static bool IsAssignment(this ExpressionType type) {
            return IsWriteOnlyAssignment(type) || IsReadWriteAssignment(type);
        }

        public static bool IsWriteOnlyAssignment(this ExpressionType type) {
            return type == ExpressionType.Assign;
        }

        public static bool IsReadWriteAssignment(this ExpressionType type) {
            switch (type) {
                // unary:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PreIncrementAssign:

                // binary - compound:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if the left child of the given expression is read or written to or both.
        /// </summary>
        public static ExpressionAccess GetLValueAccess(this ExpressionType type) {
            if (type.IsReadWriteAssignment()) {
                return ExpressionAccess.ReadWrite;
            }

            if (type.IsWriteOnlyAssignment()) {
                return ExpressionAccess.Write;
            }

            return ExpressionAccess.Read;
        }

        public static bool IsLValue(this ExpressionType type) {
            // see Expression.RequiresCanWrite
            switch (type) {
                case ExpressionType.Index:
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                    return true;
            }

            return false;
        }
    }
}
